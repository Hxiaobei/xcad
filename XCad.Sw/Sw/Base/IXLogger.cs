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

using XCad.Sw.Base.Enums;

namespace XCad.Sw.Base {
    /// <summary>
    /// Logs the trace messages
    /// </summary>
    public interface IXLogger {
        /// <summary>
        /// Logs message
        /// </summary>
        /// <param name="msg">Message</param>
        /// <param name="severity">Type of the message</param>
        void Log(string msg, LoggerMessageSeverity_e severity = LoggerMessageSeverity_e.Information);
    }
}
