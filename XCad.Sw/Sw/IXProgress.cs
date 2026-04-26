//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;

namespace XCad.Sw {
    /// <summary>
    /// Enables the display of progress bar and status
    /// </summary>
    public interface IXProgress : IDisposable, IProgress<double> {
        /// <summary>
        /// Sets status of the operation
        /// </summary>
        /// <param name="status">Status messae</param>
        void SetStatus(string status);
    }
}
