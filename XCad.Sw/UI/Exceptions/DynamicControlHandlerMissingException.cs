//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Reflection;

namespace XCad.UI.Exceptions {
    /// <summary>
    /// Indicates that not handler for dynamic controls
    /// </summary>
    public class DynamicControlHandlerMissingException : Exception {
        /// <summary>
        /// Default constructor
        /// </summary>
        public DynamicControlHandlerMissingException(PropertyInfo prp)
            : base($"{prp.Name} property set as dynamic controls, but dynamic control creation handler is not set in") {
        }
    }
}
