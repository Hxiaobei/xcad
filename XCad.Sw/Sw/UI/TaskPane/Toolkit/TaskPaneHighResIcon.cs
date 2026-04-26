//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Drawing;
using XCad.kit.Base;
using XCad.UI;

namespace XCad.Sw.UI.TaskPane.Toolkit {
    internal class TaskPaneHighResIcon : TaskPaneIcon {
        internal TaskPaneHighResIcon(IXImage icon) : base(icon) {
            IconSizes = new IIconSpec[]
            {
                new IconSpec(m_Icon, new Size(20, 20)),
                new IconSpec(m_Icon, new Size(32, 32)),
                new IconSpec(m_Icon, new Size(40, 40)),
                new IconSpec(m_Icon, new Size(64, 64)),
                new IconSpec(m_Icon, new Size(96, 96)),
                new IconSpec(m_Icon, new Size(128, 128))
            };
        }

        public override IIconSpec[] IconSizes { get; }
    }
}