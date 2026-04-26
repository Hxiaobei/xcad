//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;

namespace XCad.Sw.Features.CustomFeature.Exceptions {
    public class DefinitionTypeMismatch : InvalidCastException {
        public DefinitionTypeMismatch(Type defType, Type expectedType)
            : base($"{defType.FullName} must inherit {expectedType.FullName}") {
        }
    }
}
