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
    public interface ISwCylindricalSurface : ISwSurface, IXCylindricalSurface {

    }

    internal class SwCylindricalSurface : SwSurface, ISwCylindricalSurface {
        internal SwCylindricalSurface(ISurface surface, SwDocument doc, SwApplication app) : base(surface, doc, app) {
        }

        public Axis Axis {
            get {
                var cylParams = CylinderParams;

                return new Axis(
                    new Vec3d(cylParams[0], cylParams[1], cylParams[2]),
                    new Vec3d(cylParams[3], cylParams[4], cylParams[5]));
            }
        }

        public double Radius => CylinderParams[6];

        private double[] CylinderParams => Surface.CylinderParams as double[];
    }
}
