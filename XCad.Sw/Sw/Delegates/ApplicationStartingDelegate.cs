//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Diagnostics;

namespace XCad.Sw.Delegates {
    /// <summary>
    /// Delegate for <see cref="ISwApplication.Starting"/> event
    /// </summary>
    /// <param name="sender">Application which is starting</param>
    /// <param name="process">Application process</param>
    public delegate void ApplicationStartingDelegate(ISwApplication sender, Process process);
}
