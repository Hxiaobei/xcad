//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************


//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using XCad.Structures;

namespace XCad.Sw.Geometry.Surfaces {
    /// <summary>
    /// Represents the specific cylindrical surface
    /// </summary>
    public interface IXCylindricalSurface : IXSurface {
        /// <summary>
        /// Axis of this cylindrical face
        /// </summary>
        Axis Axis { get; }

        /// <summary>
        /// Radius of cylindrical face
        /// </summary>
        double Radius { get; }
    }
}
