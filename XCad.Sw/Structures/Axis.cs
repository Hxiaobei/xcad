using System.Diagnostics;

namespace XCad.Structures {
    /// <summary>
    /// Represents axis - direction through the point
    /// </summary>
    [DebuggerDisplay("{" + nameof(Point) + "} - {" + nameof(Direction) + "}")]
    public class Axis {
        /// <summary>
        /// Reference point of this axis
        /// </summary>
        public Vec3d Point { get; set; }

        /// <summary>
        /// Direction of this axis
        /// </summary>
        public Vec3d Direction { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="refPt">Reference point of this exis</param>
        /// <param name="dir">Direction of the exis</param>
        public Axis(Vec3d refPt, Vec3d dir) {
            Point = refPt;
            Direction = dir;
        }
    }
}
