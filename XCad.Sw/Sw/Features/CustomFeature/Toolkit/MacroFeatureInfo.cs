//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace XCad.Sw.Features.CustomFeature.Toolkit {
    internal static class MacroFeatureInfo {
        internal static string GetBaseName<TMacroFeature>()
            where TMacroFeature : SwMacroFeatureDefinition {
            return GetBaseName(typeof(TMacroFeature));
        }

        internal static string GetBaseName(Type macroFeatType) {
            if(!typeof(SwMacroFeatureDefinition).IsAssignableFrom(macroFeatType)) {
                throw new InvalidCastException(
                    $"{macroFeatType.FullName} must inherit {typeof(SwMacroFeatureDefinition).FullName}");
            }

            return macroFeatType.TryGetAttribute<DisplayNameAttribute>()?.DisplayName ?? macroFeatType.Name;
        }

        internal static string GetProgId<TMacroFeature>()
            where TMacroFeature : SwMacroFeatureDefinition
            => GetProgId(typeof(TMacroFeature));


        internal static string GetProgId(Type macroFeatType) {
            if(!typeof(SwMacroFeatureDefinition).IsAssignableFrom(macroFeatType)) {
                throw new InvalidCastException(
                    $"{macroFeatType.FullName} must inherit {typeof(SwMacroFeatureDefinition).FullName}");
            }
            return macroFeatType.TryGetAttribute<ProgIdAttribute>()?.Value ?? macroFeatType.FullName;
        }
    }
}