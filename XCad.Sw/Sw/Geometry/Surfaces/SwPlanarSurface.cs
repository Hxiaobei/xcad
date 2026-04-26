//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using XCad.Structures;
using XCad.Sw.Documents;


namespace XCad.Sw.Geometry.Surfaces {
    public interface ISwPlanarSurface : ISwSurface, IXPlanarSurface {
    }

    internal class SwPlanarSurface : SwSurface, ISwPlanarSurface {
        internal SwPlanarSurface(ISurface surface, SwDocument doc, SwApplication app) : base(surface, doc, app) {
        }

        public Plane Plane {
            get {
                var planeParams = Surface.PlaneParams as double[];

                var rootPt = new Vec3d(planeParams[3], planeParams[4], planeParams[5]);
                var normVec = new Vec3d(planeParams[0], planeParams[1], planeParams[2]);
                var refVec = normVec.CreateAnyPerpendicular();

                return new Plane(rootPt, normVec, refVec);
            }
        }
    }
}
