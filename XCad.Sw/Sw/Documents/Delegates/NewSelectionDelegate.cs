//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

namespace XCad.Sw.Documents.Delegates {
    /// <summary>
    /// Delegate for the <see cref="ISwSelectionCollection.NewSelection"/> event
    /// </summary>
    /// <param name="doc">Document where selection is done</param>
    /// <param name="selObject">Selected object</param>
    public delegate void NewSelectionDelegate(ISwDocument doc, ISwSelObject selObject);
}
