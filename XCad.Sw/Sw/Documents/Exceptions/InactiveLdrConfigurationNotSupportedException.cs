//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using XCad.Sw.Exceptions;

namespace XCad.Sw.Documents.Exceptions {
    public class InactiveLdrConfigurationNotSupportedException : NotSupportedException, IUserException {
        public InactiveLdrConfigurationNotSupportedException()
            : base("Inactive configuration of assembly opened in Large Design Review model is not supported and cannot be loaded") {
        }
    }
}
