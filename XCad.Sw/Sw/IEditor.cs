//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;

namespace XCad.Sw {
    /// <summary>
    /// Represents the editor of the specific object
    /// </summary>
    public interface IEditor<out TEnt> : IDisposable
        where TEnt : ISwObject {
        /// <summary>
        /// Object being edited
        /// </summary>
        TEnt Target { get; }

        /// <summary>
        /// True to cancel editing
        /// </summary>
        bool Cancel { get; set; }
    }
}
