//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using XCad.UI.PropertyPage.Enums;
using XCad.UI.PropertyPage.Structures;

namespace XCad.UI.PropertyPage.Delegates {
    /// <summary>
    /// Delegateof <see cref="IXPropertyPage{TDataModel}.Closing"/> event
    /// </summary>
    /// <param name="reason">Reason of closing</param>
    /// <param name="arg">Additional arguments to change the closing behavior</param>
    public delegate void PageClosingDelegate(PageCloseReasons_e reason, PageClosingArg arg);
}