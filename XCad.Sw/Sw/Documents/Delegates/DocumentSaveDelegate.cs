//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using XCad.Sw.Documents.Enums;
using XCad.Sw.Documents.Structures;

namespace XCad.Sw.Documents.Delegates {
    /// <summary>
    /// Delegate for <see cref="ISwDocument.Saving"/> event
    /// </summary>
    /// <param name="doc">Document being saved</param>
    /// <param name="type">Save type</param>
    /// <param name="args">Savig arguments</param>
    public delegate void DocumentSaveDelegate(ISwDocument doc, DocumentSaveType_e type, DocumentSaveArgs args);
}
