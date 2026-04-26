//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

namespace XCad.Sw.Enums {
    /// <summary>
    /// Position of tooltip for <see cref="ISwApplication.ShowTooltip(string, System.Drawing.Point, MessageBoxIcon_e)"/>
    /// </summary>
    public enum TooltipArrowPosition_e {
        LeftTop = 0,
        LeftBottom = 1,
        RightTop = 2,
        RightBottom = 3,
        UpTopLeft = 4,
        UpTopRight = 5,
        DownBottomLeft = 6,
        DownBottomRight = 7,
        LeftOrRightTop = 8,
        LeftOrRightBottom = 9,
        LeftOrRight = 10,
        UpOrDownLeft = 11,
        UpOrDownRight = 12,
        UpOrDown = 13,
        None = 14,
        Unknown = 15
    }
}
