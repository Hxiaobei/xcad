//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using XCad.UI.PropertyPage.Structures;

namespace XCad.UI.PropertyPage.Base {
    /// <summary>
    /// Represents the base control for items source
    /// </summary>
    public interface IItemsControl : IControl {
        /// <summary>
        /// Items of this control
        /// </summary>
        ItemsControlItem[] Items { get; set; }
    }
}
