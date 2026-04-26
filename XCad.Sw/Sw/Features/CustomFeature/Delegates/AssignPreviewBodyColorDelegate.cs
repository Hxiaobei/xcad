//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using XCad.Sw.Geometry;

namespace XCad.Sw.Features.CustomFeature.Delegates {
    /// <summary>
    /// Assigns the custom color to the preview body
    /// </summary>
    /// <param name="body">Body to assign preview to</param>
    /// <param name="color">Color of the preview body</param>
    public delegate void AssignPreviewBodyColorDelegate(ISwBody body, out System.Drawing.Color color);
}
