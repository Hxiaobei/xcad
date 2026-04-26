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
    /// Defines the group of buttons
    /// </summary>
    public class ButtonGroupSpec {
        /// <summary>
        /// Title of the group
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Help text (tooltip) of the group
        /// </summary>
        public string Tooltip { get; set; }

        /// <summary>
        /// Group icon
        /// </summary>
        public IXImage Icon { get; set; }
    }

    internal static class ButtonGroupSpecExtension {
        internal static void InitFromEnum<TEnum>(this ButtonGroupSpec btnGrp)
     where TEnum : Enum {
            var cmdGroupType = typeof(TEnum);
            btnGrp.Icon = cmdGroupType.TryGetAttribute<IconAttribute>()?.Icon ?? Defaults.Icon;
            btnGrp.Title = cmdGroupType.TryGetAttribute<DisplayNameAttribute>()?.DisplayName ?? cmdGroupType.Name;
            btnGrp.Tooltip = cmdGroupType.TryGetAttribute<DescriptionAttribute>()?.Description ?? cmdGroupType.Name;
        }
    }
}
