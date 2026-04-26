//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;

namespace XCad.Sw.Exceptions {
    public class AppStartCancelledByUserException : Exception {
        public AppStartCancelledByUserException() : base("Application start is cancelled by user") {
        }
    }
}
