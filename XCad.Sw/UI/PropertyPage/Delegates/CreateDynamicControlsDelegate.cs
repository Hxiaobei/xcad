//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using XCad.UI.PropertyPage.Base;

namespace XCad.UI.PropertyPage.Delegates {
    /// <summary>
    /// Handler of dynamic controls in the proeprty page
    /// </summary>
    /// <param name="tag">Control tag</param>
    /// <returns>Dynamic control descriptors</returns>
    public delegate IControlDescriptor[] CreateDynamicControlsDelegate(object tag);
}
