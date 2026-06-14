
using System;
using XCad.Sw.Features.CustomFeature;

namespace XCad.kit.CustomFeature {
    /// <summary>
    /// Represents the custom attribute of the <see cref="ISwMacroFeature"/>
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="name">Attribute name</param>
    /// <param name="type">Attribute data type</param>
    /// <param name="value">Attribute value</param>
    public class CustomFeatureAttribute(string name, Type type, object value) {
        /// <summary>
        /// Name of the attribute
        /// </summary>
        public string Name { get; } = name;

        /// <summary>
        /// Data type of the attribute
        /// </summary>
        public Type Type { get; } = type;

        /// <summary>
        /// Value of the attribute
        /// </summary>
        public object Value { get; } = value;
    }
}