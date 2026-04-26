//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using XCad.Sw.Exceptions;

namespace XCad.Sw.Documents.Exceptions {
    /// <summary>
    /// Indicates that the default templaet cannot be found for the next document
    /// </summary>
    public class DefaultTemplateNotFoundException : Exception, IUserException {
        /// <summary>
        /// Default constructor
        /// </summary>
        public DefaultTemplateNotFoundException() : base("Failed to find the location of default document template") {
        }
    }
}
