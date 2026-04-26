//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using XCad.Sw;
using XCad.Sw.Enums;

namespace XCad.kit {
    public class Font : IFont {
        public string Name { get; }
        public double? Size { get; }
        public double? SizeInPoints { get; }
        public FontStyle_e Style { get; }

        public Font(string name, double? size, double? sizeInPoints, FontStyle_e style) {
            Name = name;
            Size = size;
            SizeInPoints = sizeInPoints;
            Style = style;
        }
    }
}
