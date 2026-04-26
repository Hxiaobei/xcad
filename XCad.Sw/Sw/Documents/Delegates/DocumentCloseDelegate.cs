//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************


//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

namespace XCad.Sw.Documents.Delegates {
    /// <summary>
    /// Type of the closing document event used in <see cref="DocumentCloseDelegate"/>
    /// </summary>
    public enum DocumentCloseType_e {
        /// <summary>
        /// Document is closed and unloaded from the memory
        /// </summary>
        Destroy,

        /// <summary>
        /// Document is closing but remains in the memory (e.g. in drawing or assembly)
        /// </summary>
        Hide
    }

    /// <summary>
    /// Delegate for <see cref="ISwDocument.Closing"/> notification
    /// </summary>
    /// <param name="doc">Document being closed</param>
    /// <param name="type">Closing type</param>
    public delegate void DocumentCloseDelegate(ISwDocument doc, DocumentCloseType_e type);
}
