//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.ComponentModel;
using System.Diagnostics;


namespace XCad.UI.PropertyPage.Structures {
    /// <summary>
    /// Represents the item in the <see cref="Base.IItemsControl"/>
    /// </summary>
    [DebuggerDisplay("{" + nameof(DisplayName) + "} [{" + nameof(Value) + "}]")]
    public class ItemsControlItem {
        private static string GetDisplayName(object value, string dispMembPath) {
            if(!string.IsNullOrEmpty(dispMembPath)) {
                var prps = dispMembPath.Split('.');

                var curVal = value;

                for(int i = 0; i < prps.Length; i++) {
                    curVal = GetPropertyValue(curVal, prps[i]);
                }

                return curVal?.ToString() ?? "";
            } else {
                if(value != null) {
                    string dispName = value.GetType().TryGetAttribute<DisplayNameAttribute>().DisplayName;

                    if(string.IsNullOrEmpty(dispName)) dispName = value.ToString();

                    return dispName;
                } else {
                    return "";
                }
            }
        }

        private static string GetDescription(object value)
           => value.GetType().TryGetAttribute<DescriptionAttribute>()?.Description ?? "";

        private static object GetPropertyValue(object value, string prpName) {
            if(value != null) {
                var prp = value.GetType().GetProperty(prpName);

                if(prp != null) {
                    return prp.GetValue(value, null);
                } else {
                    return null;
                }
            } else {
                return null;
            }
        }

        /// <summary>
        /// Display name of the item
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Value of the item
        /// </summary>
        public object Value { get; }

        /// <summary>
        /// Description of the item
        /// </summary>
        public string Description { get; }

        public ItemsControlItem(object value, string dispMembPath)
            : this(value, GetDisplayName(value, dispMembPath), GetDescription(value)) {
        }

        public ItemsControlItem(object value, string dispName, string desc) {
            Value = value;
            DisplayName = dispName;
            Description = desc;
        }
    }
}
