//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using XCad.Sw.Exceptions;

namespace XCad.Sw.Documents.Exceptions {
    public class SpeedPakConfigurationComponentsException : Exception, IUserException {
        public SpeedPakConfigurationComponentsException() : base("Components cannot be extracted from the SpeedPak configuration") {
        }
    }
}
