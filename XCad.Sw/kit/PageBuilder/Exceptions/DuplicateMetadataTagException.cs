//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;

namespace XCad.kit.PageBuilder.Exceptions {
    public class DuplicateMetadataTagException : Exception {
        public DuplicateMetadataTagException(object tag) : base($"'{tag?.ToString()}' tag already assigned to metadata") {
        }
    }
}
