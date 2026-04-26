//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;

namespace XCad.Sw.Documents.Exceptions {
    public class UnloadedDocumentPreviewOnlySheetException : NotSupportedException {
        public UnloadedDocumentPreviewOnlySheetException()
            : base("Active sheet of uncommitted document can only be used to extract preview") {
        }
    }
}
