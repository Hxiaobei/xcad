//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;

namespace XCad.kit.PageBuilder.Exceptions {
    public class ConstructorNotFoundException : Exception {
        internal ConstructorNotFoundException(Type type, string message = "")
            : base($"Constructor for type {type.FullName} is not found. {message}") {
        }
    }
}