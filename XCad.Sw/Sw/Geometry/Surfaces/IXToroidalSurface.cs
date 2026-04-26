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
    /// Toroidal surface
    /// </summary>
    public interface IXToroidalSurface : IXSurface {
        /// <summary>
        /// Axis of toroidal surface
        /// </summary>
        Axis Axis { get; }

        /// <summary>
        /// Major radius
        /// </summary>
        double MajorRadius { get; }

        /// <summary>
        /// Minor radius
        /// </summary>
        double MinorRadius { get; }
    }
}
