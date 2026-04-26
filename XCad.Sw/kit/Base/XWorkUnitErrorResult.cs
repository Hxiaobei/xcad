//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using XCad.Sw.Base;

namespace XCad.kit.Base {
    public class XWorkUnitErrorResult : IXWorkUnitErrorResult {
        public Exception Error { get; }

        public XWorkUnitErrorResult(Exception error) {
            Error = error;
        }
    }
}
