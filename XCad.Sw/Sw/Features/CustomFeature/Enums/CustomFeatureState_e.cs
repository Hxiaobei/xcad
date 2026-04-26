//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;

namespace XCad.Sw.Features.CustomFeature.Enums {
    /// <summary>
    /// State of the <see cref="ISwMacroFeature"/> within the <see cref="ISwMacroFeatureDefinition.OnUpdateState(ISwApplication, Documents.ISwDocument, ISwMacroFeature)"/>
    /// </summary>
    [Flags]
    public enum CustomFeatureState_e {
        Default = 0,
        CannotBeDeleted = 1,
        NotEditable = 2,
        CannotBeSuppressed = 4,
        CannotBeReplaced = 8,
        EnableNote = 16,
        CannotBeRolledBack = 32
    }
}