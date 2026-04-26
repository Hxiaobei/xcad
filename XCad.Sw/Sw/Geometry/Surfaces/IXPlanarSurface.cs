//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using XCad.Structures;

namespace XCad.Sw.Geometry.Surfaces {
    /// <summary>
    /// Represents specific planar surface
    /// </summary>
    public interface IXPlanarSurface : IXSurface {
        /// <summary>
        /// Plane defining this planar surface
        /// </summary>
        Plane Plane { get; }
    }
}
