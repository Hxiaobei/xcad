//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Drawing;

using XCad.kit.Base;
using XCad.UI;

namespace XCad.Sw.UI.Commands.Toolkit.Structures {
    internal class CommandGroupIcon : IIcon {
        protected readonly IXImage m_Icon;

        private static readonly Color m_CommandTransparencyKey
                    = Color.FromArgb(192, 192, 192);

        public virtual Color TransparencyKey => m_CommandTransparencyKey;

        public bool IsPermanent => false;

        public IconImageFormat_e Format => IconImageFormat_e.Bmp;

        internal CommandGroupIcon(IXImage icon) {
            m_Icon = icon;
            IconSizes = new IIconSpec[]
            {
                new IconSpec(m_Icon, new Size(16, 16)),
                new IconSpec(m_Icon, new Size(24, 24))
            };
        }

        public virtual IIconSpec[] IconSizes { get; }
    }
}