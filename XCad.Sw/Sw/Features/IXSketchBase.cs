//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************
using XCad.Sw.Sketch;

namespace XCad.Sw.Features {
    /// <summary>
    /// Represents the base sketch 2D or 3D
    /// </summary>
    public interface IXSketchBase : ISwFeature {
        /// <summary>
        /// List of sketch entitites (segments and points)
        /// </summary>
        ISwSketchEntityCollection Entities { get; }

        /// <summary>
        /// Manages the blank state (hidden/visible) of the sketch
        /// </summary>
        bool IsBlank { get; set; }
    }
}