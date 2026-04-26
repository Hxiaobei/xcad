//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;

namespace XCad.Sw.UI.Commands.Exceptions {
    public class ComControlHostException : Exception {
        public ComControlHostException(string progId)
            : base($"Failed to create COM control from '{progId}'. Make sure that COM component is properly registered") {
        }
    }
}
