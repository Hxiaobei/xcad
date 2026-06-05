using System.Collections.Generic;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using XCad.Sw.Base;
using XCad.Sw.Extensions;
using XCad.Sw.Geometry;

namespace XCad.Sw.Documents {
    public interface ISwPart : ISwDocument3D {
        IPartDoc Part { get; }

        /// <inheritdoc/>
        new ISwPartConfigurationCollection Configurations { get; }

        /// <summary>
        /// Bodies in this part document
        /// </summary>
        ISwBodyCollection Bodies { get; }
    }

    internal class SwPart : SwDocument3D, ISwPart {
        public IPartDoc Part => Model as IPartDoc;

        public ISwBodyCollection Bodies { get; }

        internal SwPart(IPartDoc part, SwApplication app, IXLogger logger, bool isCreated)
            : base((IModelDoc2)part, app, logger, isCreated) {
            Bodies = new SwPartBodyCollection(this);
        }

        internal protected override swDocumentTypes_e? DocumentType => swDocumentTypes_e.swDocPART;

        protected override SwAnnotationCollection CreateAnnotations() => new SwDocument3DAnnotationCollection(this);

        ISwPartConfigurationCollection ISwPart.Configurations => (ISwPartConfigurationCollection)Configurations;

        protected override SwConfigurationCollection CreateConfigurations()
            => new SwPartConfigurationCollection(this, OwnerApplication);

        protected override bool IsDocumentTypeCompatible(swDocumentTypes_e docType) => docType == swDocumentTypes_e.swDocPART;
    }

    internal class SwPartBodyCollection(SwPart rootDoc) : SwBodyCollection(rootDoc) {
        private readonly SwPart m_Part = rootDoc;

        protected override IEnumerable<IBody2> SelectSwBodies(swBodyType_e bodyType)
            => m_Part.Part.GetBodies2((int)bodyType, false).ToSwArray<IBody2>();
    }
}