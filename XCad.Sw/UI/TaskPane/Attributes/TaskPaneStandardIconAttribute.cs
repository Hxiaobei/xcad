//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using XCad.UI.TaskPane.Enums;

namespace XCad.UI.TaskPane.Attributes {
    public class TaskPaneStandardIconAttribute : Attribute {
        public TaskPaneStandardIcons_e StandardIcon { get; }

        public TaskPaneStandardIconAttribute(TaskPaneStandardIcons_e standardIcon) {
            StandardIcon = standardIcon;
        }
    }
}
