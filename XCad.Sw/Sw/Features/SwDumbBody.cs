//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Linq;
using System.Threading;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using XCad.Sw.Documents;
using XCad.Sw.Extensions;
using XCad.Sw.Geometry;

namespace XCad.Sw.Features {
    internal class SwDumbBody : SwFeature {
        internal SwDumbBody(IFeature feat, SwDocument doc, SwApplication app, bool created) : base(feat, doc, app, created) {
        }

        public ISwBody BaseBody {
            get {
                if(IsCommitted) {
                    var face = Feature.GetFaces().ToSwArray<IFace2>().First();
                    var body = face.IGetBody();
                    return OwnerDocument.CreateObjectFromDispatch<ISwBody>(body);
                } else {
                    return m_Creator.CachedProperties.Get<ISwBody>();
                }
            }
            set {
                if(value == null) {
                    throw new ArgumentNullException("Body cannot be null");
                }

                if(!value.Body.IsTemporaryBody()) {
                    throw new InvalidCastException("Only temp bodies can be set to the feature");
                }

                if(IsCommitted) {
                    if(!Feature.SetBody2(value.Body, true)) {
                        throw new Exception("Failed to chnage the feature body");
                    }
                } else {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        protected override IFeature InsertFeature(CancellationToken cancellationToken) {
            IPartDoc part = null;

            if(OwnerDocument is ISwPart p) {
                part = p.Part;
            } else if(OwnerDocument is ISwAssembly a) {
                if(a.EditingComponent?.ReferencedDocument is ISwPart c) {
                    part = c.Part;
                }
            }

            if(part != null) {
                var feat = (IFeature)part.CreateFeatureFromBody3(BaseBody.Body, false, (int)swCreateFeatureBodyOpts_e.swCreateFeatureBodySimplify);

                if(feat != null) {
                    return feat;
                } else {
                    throw new Exception("Failed to create feature from body");
                }
            } else {
                throw new Exception("This feature can only be inserted into a part");
            }
        }
    }
}
