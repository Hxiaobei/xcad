using XCad.Sw.Base;

namespace XCad.Sw.Annotations {
    /// <summary>
    /// Indicates that this object can have dimensions
    /// </summary>
    public interface IDimensionable {
        /// <summary>
        /// Dimensions repository
        /// </summary>
        IXRepository<ISwDimension> Dimensions { get; }
    }
}
