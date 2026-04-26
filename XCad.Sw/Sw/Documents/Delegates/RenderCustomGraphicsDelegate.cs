//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using XCad.kit.Graphics;

namespace XCad.Sw.Documents.Delegates {
    /// <summary>
    /// Delegate of <see cref="IXModelView.RenderCustomGraphics"/> event
    /// </summary>
    /// <param name="sender">Model view which sends this event</param>
    /// <param name="context">Custom graphics context</param>
    public delegate bool RenderCustomGraphicsDelegate(ISwModelView sender, OglGraphicsContext context);
}
