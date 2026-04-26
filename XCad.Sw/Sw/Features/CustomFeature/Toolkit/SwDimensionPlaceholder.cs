//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using XCad.Sw.Annotations;
using XCad.Sw.Features.CustomFeature.Services;

namespace XCad.Sw.Features.CustomFeature.Toolkit {
    /// <summary>
    /// This is a mock implementation of display SOLIDWORKS dimension
    /// It is used in <see cref="IParameterConverter.ConvertDisplayDimensions(XCad.Documents.ISwDocument, ISwMacroFeature, XCad.Annotations.ISwDimension[])"/>
    /// for supporting the backward compatibility of macro feature parameters
    /// </summary>
    internal class SwDimensionPlaceholder : SwDimension {
        internal SwDimensionPlaceholder() : base(null, null, null) {
        }

        public override double Value {
            get => double.NaN;
            set => base.Value = value;
        }
    }
}