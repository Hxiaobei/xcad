//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using XCad.Sw.Exceptions;

namespace XCad.Sw.Documents.Exceptions {
    public class ConfigurationSpecificCutListNotSupportedException : NotSupportedException, IUserException {
        public ConfigurationSpecificCutListNotSupportedException()
            : base("Configuration specific cut-lists are not supported. Instead access cut-lists from an active configuration") {
        }
    }
}
