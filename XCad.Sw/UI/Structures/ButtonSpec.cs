//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.ComponentModel;
using XCad.Sw.Base.Attributes;

namespace XCad.UI.Structures {
    /// <summary>
    /// Specification of the generic button
    /// </summary>
    public class ButtonSpec {
        /// <summary>
        /// User id if this button
        /// </summary>
        public int UserId { get; }

        /// <summary>
        /// Title of the button
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Tooltip of the button
        /// </summary>
        public string Tooltip { get; set; }

        /// <summary>
        /// Icon associated with the button
        /// </summary>
        public IXImage Icon { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="userId">Button user id</param>
        public ButtonSpec(int userId) {
            UserId = userId;
        }
    }

    internal static class ButtonSpecExtension {
        internal static void InitFromEnum<TEnum>(this ButtonSpec btn, TEnum btnEnum)
            where TEnum : Enum {
            btn.Title = btnEnum.TryGetAttribute<DisplayNameAttribute>()?.DisplayName ?? btnEnum.ToString();
            btn.Tooltip = btnEnum.TryGetAttribute<DescriptionAttribute>()?.Description ?? btn.Title;
            btn.Icon = btnEnum.TryGetAttribute<IconAttribute>()?.Icon ?? Defaults.Icon;
        }
    }
}
