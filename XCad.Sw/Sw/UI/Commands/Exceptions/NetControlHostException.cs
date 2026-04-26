//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;

namespace XCad.Sw.UI.Commands.Exceptions {
    public class NetControlHostException : Exception {
        public NetControlHostException(IntPtr handle) : base($"Failed to host .NET control (handle {handle}) in task pane") {
        }
    }
}
