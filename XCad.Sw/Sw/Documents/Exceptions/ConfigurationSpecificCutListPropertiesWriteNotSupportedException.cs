//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using XCad.Sw.Exceptions;

namespace XCad.Sw.Documents.Exceptions {
    public class ConfigurationSpecificCutListPropertiesWriteNotSupportedException : NotSupportedException, IUserException {
        public ConfigurationSpecificCutListPropertiesWriteNotSupportedException()
            : base("Modifying configuration specific cut-list properties is not supported") {
        }
    }
}
