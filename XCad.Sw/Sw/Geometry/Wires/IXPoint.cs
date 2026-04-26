//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using XCad.Structures;

namespace XCad.Sw.Geometry.Wires {
    /// <summary>
    /// Represents the point entity
    /// </summary>
    public interface IXPoint : IXWireEntity {
        /// <summary>
        /// Coodinate of the point
        /// </summary>
        Vec3d Coordinate { get; set; }
    }
}
