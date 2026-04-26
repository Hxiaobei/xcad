//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

namespace XCad.Sw.Documents.Enums {
    /// <summary>
    /// Saving type of the document in the <see cref="ISwDocument.Saving"/> event
    /// </summary>
    public enum DocumentSaveType_e {
        /// <summary>
        /// Document is saving to the current path
        /// </summary>
        SaveCurrent,

        /// <summary>
        /// Saving as new document
        /// </summary>
        SaveAs
    }
}
