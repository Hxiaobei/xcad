//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Drawing;
using XCad.Sw.Base;
using XCad.Sw.Enums;

namespace XCad.Sw.Extensions {
    /// <summary>
    /// Collection of additional functions of <see cref="ISwApplication"/>
    /// </summary>
    public static class XApplicationExtension {
        private class TooltipSpec : ITooltipSpec {
            public string Title { get; }
            public string Message { get; }
            public Point Position { get; }
            public TooltipArrowPosition_e ArrowPosition { get; }

            internal TooltipSpec(string title, string msg, Point pt, TooltipArrowPosition_e arrPos) {
                Title = title;
                Message = msg;
                Position = pt;
                ArrowPosition = arrPos;
            }
        }

        /// <summary>
        /// Displays the tooltip
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="title">Tooltip title</param>
        /// <param name="msg">Tooltip content (message)</param>
        /// <param name="pt">Tooltip position</param>
        /// <param name="arrPos">Arrow position of the tooltip</param>
        public static void ShowTooltip(this ISwApplication app, string title, string msg, Point pt, TooltipArrowPosition_e arrPos)
            => app.ShowTooltip(new TooltipSpec(title, msg, pt, arrPos));
    }
}
