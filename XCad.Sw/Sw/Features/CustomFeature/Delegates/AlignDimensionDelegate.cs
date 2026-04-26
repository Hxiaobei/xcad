//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************


using XCad.Sw.Annotations;

namespace XCad.Sw.Features.CustomFeature.Delegates {
    /// <summary>
    /// Handler function to align specific dimension of <see cref="ISwMacroFeatureDefinition{TParams}"></see> within the <see cref="ISwMacroFeatureDefinition.OnRebuild(ISwApplication, Documents.ISwDocument, ISwMacroFeature)"/>/>
    /// </summary>
    /// <typeparam name="TData">Type of the data</typeparam>
    /// <param name="paramName">Name of the parameter in the data model which corresponds to this dimension</param>
    /// <param name="dim">Dimension to align</param>
    public delegate void AlignDimensionDelegate<TData>(string paramName, ISwDimension dim)
        where TData : class;
}