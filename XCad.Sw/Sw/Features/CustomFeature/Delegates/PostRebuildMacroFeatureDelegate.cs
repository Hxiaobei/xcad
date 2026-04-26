//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using XCad.Sw.Documents;

namespace XCad.Sw.Features.CustomFeature.Delegates {
    /// <summary>
    /// Delegate of <see cref="SwMacroFeatureDefinition.PostRebuild"/> event
    /// </summary>
    /// <param name="app">Application</param>
    /// <param name="model">Document</param>
    /// <param name="feature">Feature</param>
    public delegate void PostRebuildMacroFeatureDelegate(ISwApplication app, ISwDocument model, ISwMacroFeature feature);

    /// <inheritdoc/>
    /// <param name="parameters">Parameters</param>
    public delegate void PostRebuildMacroFeatureDelegate<TParams>(ISwApplication app, ISwDocument model, ISwMacroFeature<TParams> feature, TParams parameters)
        where TParams : class;
}
