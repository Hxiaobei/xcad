using System;
using System.Linq;
using XCad.Sw.Base;
using XCad.UI;
using XCad.UI.Structures;
using XCad.UI.TaskPane;
using XCad.UI.TaskPane.Attributes;

namespace XCad.Sw.Extensions {
    /// <summary>
    /// Additional methods for the <see cref="IXEx"/>
    /// </summary>
    public static class XExtension {
        private class XWorkUnitUserResult<TRes> : IXWorkUnitUserResult<TRes> {
            public TRes Result { get; }

            internal XWorkUnitUserResult(TRes result) {
                Result = result;
            }
        }

        /// <summary>
        /// Creates Task Pane from the enumeration definition
        /// </summary>
        /// <typeparam name="TControl">Type of control</typeparam>
        /// <typeparam name="TEnum">Enumeration defining the commands for Task Pane</typeparam>
        /// <param name="ext">Extension</param>
        /// <returns>Task Pane instance</returns>
        public static IXEnumTaskPane<TControl, TEnum> CreateTaskPane<TControl, TEnum>(this ISwAddInEx ext)
            where TEnum : Enum {
            var spec = new TaskPaneSpec();
            spec.InitFromEnum<TEnum>();
            spec.Buttons = Enum.GetValues(typeof(TEnum)).Cast<TEnum>().Select(
                c => {
                    var btn = new TaskPaneEnumButtonSpec<TEnum>(Convert.ToInt32(c));
                    btn.InitFromEnum(c);
                    btn.Value = c;
                    btn.StandardIcon = c.TryGetAttribute<TaskPaneStandardIconAttribute>()?.StandardIcon;
                    return btn;
                }).ToArray();

            return new EnumTaskPane<TControl, TEnum>(ext.CreateTaskPane<TControl>(spec));
        }

        /// <summary>
        /// Creates new popup window
        /// </summary>
        /// <typeparam name="TWindow">Type of window</typeparam>
        /// <param name="ext">Extension</param>
        /// <returns>Popup window</returns>
        public static IXPopupWindow<TWindow> CreatePopupWindow<TWindow>(this ISwAddInEx ext)
            where TWindow : new() => ext.CreatePopupWindow<TWindow>(new TWindow());

    }
}
