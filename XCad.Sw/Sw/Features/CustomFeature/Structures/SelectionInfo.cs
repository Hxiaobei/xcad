
using XCad.Structures;

namespace XCad.Sw.Features.CustomFeature.Structures {
    /// <summary>
    /// Information of the selection of <see cref="ISwMacroFeature"/>
    /// </summary>
    public class SelectionInfo {
        /// <summary>
        /// Selection
        /// </summary>
        public ISwSelObject Selection { get; }

        /// <summary>
        /// Transformation of this selection
        /// </summary>
        public Transform Transformation { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="selection">Selection</param>
        /// <param name="transformation">Transformation of the selection</param>
        public SelectionInfo(ISwSelObject selection, Transform transformation) {
            Selection = selection;
            Transformation = transformation;
        }
    }
}