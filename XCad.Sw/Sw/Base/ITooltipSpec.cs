//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Drawing;
using XCad.Sw.Enums;

namespace XCad.Sw.Base {
    /// <summary>
    /// Defines the specification of the tooltip used in the <see cref="ISwApplication.ShowTooltip(ITooltipSpec)"/>
    /// </summary>
    public interface ITooltipSpec {
        /// <summary>
        /// Title of tooltip
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Message to show in tooltip
        /// </summary>
        string Message { get; }

        /// <summary>
        /// Position of the tooltip
        /// </summary>
        Point Position { get; }

        /// <summary>
        /// Position of tooltip arrow
        /// </summary>
        TooltipArrowPosition_e ArrowPosition { get; }
    }
}
