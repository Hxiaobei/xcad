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

namespace XCad.Sw.Geometry.Wires {
    /// <summary>
    /// Represents a line segment
    /// </summary>
    public interface IXLine : IXSegment {
        /// <summary>
        /// Geometry of this line
        /// </summary>
        Line Geometry { get; set; }
    }
}
