//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

namespace XCad.Sw.Documents.Structures {
    /// <summary>
    /// Argument of the item deletion event
    /// </summary>
    public class ItemDeleteArgs {
        /// <summary>
        /// Specifies if the deleting operation needs to be cancelled
        /// </summary>
        public bool Cancel { get; set; }
    }
}
