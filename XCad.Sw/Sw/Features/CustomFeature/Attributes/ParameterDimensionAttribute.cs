//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using XCad.Sw.Features.CustomFeature.Enums;

namespace XCad.Sw.Features.CustomFeature.Attributes {
    /// <summary>
    /// Specifies that the current property is a dimension of the macro feature.
    /// The value if the property is the current value of the dimension.
    /// This property is bi-directional: it will update the value of the dimension
    /// when changed within the <see cref="ISwMacroFeatureDefinition.OnRebuild(ISwApplication, Documents.ISwDocument, ISwMacroFeature)"/>
    /// as well as will contain the current value of the dimension when it got modified by the user in the
    /// graphics area
    /// </summary>
    /// <remarks>This should only be used for numeric properties</remarks>
    [AttributeUsage(AttributeTargets.Property)]
    public class ParameterDimensionAttribute : Attribute {
        public CustomFeatureDimensionType_e DimensionType { get; private set; }

        /// <summary>
        /// Marks this property as dimension and assigns the dimension type
        /// </summary>
        /// <param name="dimType">Type of the dimension</param>
        public ParameterDimensionAttribute(CustomFeatureDimensionType_e dimType) {
            DimensionType = dimType;
        }
    }
}