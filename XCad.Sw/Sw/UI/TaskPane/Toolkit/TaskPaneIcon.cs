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
    internal class TaskPaneIcon : IIcon {
        protected readonly IXImage m_Icon;

        public virtual Color TransparencyKey => Color.White;

        public bool IsPermanent => false;

        public IconImageFormat_e Format => IconImageFormat_e.Bmp;

        internal TaskPaneIcon(IXImage icon) {
            m_Icon = icon;
            IconSizes = new IIconSpec[]
            {
                new IconSpec(m_Icon, new Size(16, 18))
            };
        }

        public virtual IIconSpec[] IconSizes { get; }
    }
}