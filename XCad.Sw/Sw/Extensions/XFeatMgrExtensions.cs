using XCad.Sw.Features;
using XCad.Sw.Features.CustomFeature;

namespace XCad.Sw.Extensions {
    /// <summary>
    /// Additional methods for <see cref="ISwFeatureRepository"/>
    /// </summary>
    public static class XFeatMgrExtensions {
        /// <summary>
        /// Starts the insertion of the custom feature with page editor
        /// </summary>
        /// <typeparam name="TDef">Defintion</typeparam>
        /// <typeparam name="TParams">Parameters</typeparam>
        /// <typeparam name="TPage">Page</typeparam>
        /// <param name="featMgr">Feature repository</param>
        public static void CreateCustomFeature<TDef, TParams, TPage>(this ISwFeatureManager featMgr)
            where TParams : class, new()
            where TPage : class
            where TDef : class, ISwMacroFeatureDefinition<TParams, TPage>, new()
            => featMgr.CreateCustomFeature<TDef, TParams, TPage>(new TParams());
    }
}