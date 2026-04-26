//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Drawing;

using XCad.kit.Base;
using XCad.UI;

namespace XCad.Sw.UI {
    internal class TooltipIcon : IIcon {
        internal IXImage Icon { get; }

        public Color TransparencyKey => Color.White;

        public bool IsPermanent => false;

        public IconImageFormat_e Format => IconImageFormat_e.Bmp;

        internal TooltipIcon(IXImage icon) {
            Icon = icon;

            IconSizes = new IIconSpec[]
            {
                new IconSpec(Icon, new Size(16, 16))
            };
        }

        public IIconSpec[] IconSizes { get; }
    }
}
