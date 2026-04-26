//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Drawing;
using System.IO;

using XCad.kit.Base;

namespace XCad.Sw.Features.CustomFeature.Toolkit {
    internal static class MacroFeatureIconInfo {
        internal const string DEFAULT_ICON_FOLDER = "Xarial\\xCad.Sw\\{0}\\Icons";
        internal const string RegularName = "Regular";
        internal const string SuppressedName = "Suppressed";
        internal const string HighlightedName = "Highlighted";

        internal static IconImageFormat_e Format { get; } = IconImageFormat_e.Bmp;
        internal static Size Size { get; } = new Size(16, 18);
        internal static Size SizeHighResSmall { get; } = new Size(20, 20);
        internal static Size SizeHighResMedium { get; } = new Size(32, 32);
        internal static Size SizeHighResLarge { get; } = new Size(40, 40);

        internal static string GetLocation(Type macroFeatType) {
            var iconFolderName = string.Format(DEFAULT_ICON_FOLDER, macroFeatType.FullName);
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), iconFolderName);
        }

        internal static string[] GetIcons(Type macroFeatType, bool highRes) {
            var loc = GetLocation(macroFeatType);
            return highRes
                ? new[]
                {
                    Path.Combine(loc, IconSpec.CreateFileName(RegularName, SizeHighResSmall, Format)),
                    Path.Combine(loc, IconSpec.CreateFileName(SuppressedName, SizeHighResSmall, Format)),
                    Path.Combine(loc, IconSpec.CreateFileName(HighlightedName, SizeHighResSmall, Format)),
                    Path.Combine(loc, IconSpec.CreateFileName(RegularName, SizeHighResMedium, Format)),
                    Path.Combine(loc, IconSpec.CreateFileName(SuppressedName, SizeHighResMedium, Format)),
                    Path.Combine(loc, IconSpec.CreateFileName(HighlightedName, SizeHighResMedium, Format)),
                    Path.Combine(loc, IconSpec.CreateFileName(RegularName, SizeHighResLarge, Format)),
                    Path.Combine(loc, IconSpec.CreateFileName(SuppressedName, SizeHighResLarge, Format)),
                    Path.Combine(loc, IconSpec.CreateFileName(HighlightedName, SizeHighResLarge, Format))
                }
            : new[]
                {
                    Path.Combine(loc, IconSpec.CreateFileName(RegularName, Size, Format)),
                    Path.Combine(loc, IconSpec.CreateFileName(SuppressedName, Size, Format)),
                    Path.Combine(loc, IconSpec.CreateFileName(HighlightedName, Size, Format))
                };


        }
    }
}