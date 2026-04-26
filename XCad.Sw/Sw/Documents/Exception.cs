//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;

namespace XCad.Sw.Documents {
    public class DocumentAlreadyOpenedException : Exception {
        public DocumentAlreadyOpenedException(string path) : base($"{path} document already opened") {
        }
    }
}
