using XCad.Sw.Documents;
using XCad.Sw.Features.CustomFeature.Enums;

namespace XCad.Sw.Features.CustomFeature.Services {
    //TODO: implement

    /// <summary>
    /// Handler of each macro feature
    /// </summary>
    /// <remarks>The instance of the specific class will be created for each macro feature</remarks>
    public interface ICustomFeatureHandler {
        /// <summary>
        /// Called when macro feature is created or loaded first time
        /// </summary>
        /// <param name="app">Pointer to application</param>
        /// <param name="model">Pointer to model</param>
        /// <param name="feat">Pointer to feature</param>
        void Init(ISwApplication app, ISwDocument model, ISwFeature feat);

        /// <summary>
        /// Called when macro feature is deleted or model is closed
        /// </summary>
        /// <param name="reason">Reason of macro feature unload</param>
        void Unload(CustomFeatureUnloadReason_e reason);
    }
}