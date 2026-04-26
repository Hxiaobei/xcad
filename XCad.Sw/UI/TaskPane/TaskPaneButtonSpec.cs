//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using XCad.UI.Structures;
using XCad.UI.TaskPane.Enums;

namespace XCad.UI.TaskPane {
    public class TaskPaneButtonSpec : ButtonSpec {
        public TaskPaneStandardIcons_e? StandardIcon { get; set; }

        public TaskPaneButtonSpec(int userId) : base(userId) {
        }
    }

    internal class TaskPaneEnumButtonSpec<TEnum> : TaskPaneButtonSpec
        where TEnum : Enum {
        public TEnum Value { get; set; }

        public TaskPaneEnumButtonSpec(int userId) : base(userId) {
        }
    }
}
