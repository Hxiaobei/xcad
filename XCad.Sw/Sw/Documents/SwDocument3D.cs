//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using SolidWorks.Interop.sldworks;
using XCad.Sw.Base;

namespace XCad.Sw.Documents {
    public interface ISwDocument3D : ISwDocument {
        ISwConfigurationCollection Configurations { get; }
        new ISwModelViews3DCollection ModelViews { get; }
        TSelObject ConvertObject<TSelObject>(TSelObject obj) where TSelObject : ISwSelObject;
    }

    internal abstract class SwDocument3D : SwDocument, ISwDocument3D {
        public override ISwModelViewsCollection ModelViews => ((ISwDocument3D)this).ModelViews;
        ISwModelViews3DCollection ISwDocument3D.ModelViews => m_ModelViewsLazy.Value;
        internal SwDocument3D(IModelDoc2 model, SwApplication app, IXLogger logger, bool isCreated) : base(model, app, logger, isCreated) {
            m_Configurations = new Lazy<ISwConfigurationCollection>(CreateConfigurations);
            m_ModelViewsLazy = new Lazy<ISwModelViews3DCollection>(() => new SwModelViews3DCollection(this, app));
        }

        private Lazy<ISwConfigurationCollection> m_Configurations;
        private Lazy<ISwModelViews3DCollection> m_ModelViewsLazy;

        public ISwConfigurationCollection Configurations => m_Configurations.Value;

        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);

            if(disposing) {
                if(m_Configurations.IsValueCreated) {
                    m_Configurations.Value.Dispose();
                }
            }
        }

        protected abstract SwConfigurationCollection CreateConfigurations();

        public TSelObject ConvertObject<TSelObject>(TSelObject obj) where TSelObject : ISwSelObject
            => (TSelObject)ConvertObjectBoxed(obj);

        private ISwSelObject ConvertObjectBoxed(object obj) {
            if(obj is SwSelObject) {
                var disp = (obj as SwSelObject).Dispatch;
                var corrDisp = Model.Extension.GetCorresponding(disp);

                if(corrDisp != null) {
                    return this.CreateObjectFromDispatch<ISwSelObject>(corrDisp);
                } else {
                    throw new Exception("Failed to convert the pointer of the object");
                }
            } else {
                throw new InvalidCastException("Object is not SOLIDWORKS object");
            }
        }

        public override IXSaveOperation PreCreateSaveAsOperation(string filePath) => ((ISwDocument3D)this).PreCreateSaveAsOperation(filePath);
    }
}