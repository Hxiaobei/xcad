//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using XCad.Structures;
using XCad.Sw.Documents;

namespace XCad.Sw.Features {
    public interface ISwCoordinateSystem : ISwFeature {
        /// <summary>
        /// Transformation of this coordinate system
        /// </summary>
        Transform Transform { get; }
        ICoordinateSystemFeatureData CoordSys { get; }
    }

    internal class SwCoordinateSystem : SwFeature, ISwCoordinateSystem {
        public ICoordinateSystemFeatureData CoordSys { get; }

        internal SwCoordinateSystem(IFeature feat, SwDocument doc, SwApplication app, bool created) : base(feat, doc, app, created) {
            CoordSys = feat.GetDefinition() as ICoordinateSystemFeatureData;
        }

        public Transform Transform => CoordSys.Transform.ToXa();
    }
}
