//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Drawing;

namespace XCad.Sw {
    /// <summary>
    /// Identifies the visual object which can have color
    /// </summary>
    public interface IHasColor : ISwObject {
        /// <summary>
        /// Color of visual object
        /// </summary>
        Color? Color { get; set; }
    }
}
