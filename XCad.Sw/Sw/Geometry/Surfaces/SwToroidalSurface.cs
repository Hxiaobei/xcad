using SolidWorks.Interop.sldworks;
using XCad.Structures;
using XCad.Sw.Documents;

namespace XCad.Sw.Geometry.Surfaces {
    internal class SwToroidalSurface : SwSurface {
        public SwToroidalSurface(ISurface surface, SwDocument doc, SwApplication app) : base(surface, doc, app) {
        }

        public Axis Axis {
            get {
                var torParams = (double[])Surface.TorusParams;

                return new Axis(
                    new Vec3d(torParams[0], torParams[1], torParams[2]),
                    new Vec3d(torParams[3], torParams[4], torParams[5]));
            }
        }

        public double MajorRadius => TorusParams[6];

        public double MinorRadius => TorusParams[7];

        private double[] TorusParams => (double[])Surface.TorusParams;
    }
}
