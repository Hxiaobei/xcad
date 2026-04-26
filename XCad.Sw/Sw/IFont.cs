//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using XCad.Sw.Enums;

namespace XCad.Sw {
    /// <summary>
    /// Font
    /// </summary>
    public interface IFont {
        /// <summary>
        /// Face name of the font
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Size of the font in meters if <see cref="SizeInPoints"/> is null
        /// </summary>
        double? Size { get; }

        /// <summary>
        /// Size of the font in points if <see cref="Size"/> is null
        /// </summary>
        double? SizeInPoints { get; }

        /// <summary>
        /// Font style
        /// </summary>
        FontStyle_e Style { get; }
    }
}
