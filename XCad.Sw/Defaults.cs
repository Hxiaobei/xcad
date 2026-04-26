using XCad.Properties;
using XCad.UI;

namespace XCad {
    /// <summary>
    /// Collection of default objects
    /// </summary>
    public static class Defaults {
        /// <summary>
        /// Default icon
        /// </summary>
        public static IXImage Icon => new BaseImage(Resources.default_icon);
    }
}