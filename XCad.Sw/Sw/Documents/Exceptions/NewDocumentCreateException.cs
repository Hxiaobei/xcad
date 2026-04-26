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
    /// Exception indicates that new document cannot be created
    /// </summary>
    public class NewDocumentCreateException : Exception, IUserException {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="templateName">Name of the document template</param>
        public NewDocumentCreateException(string templateName)
            : base($"Failed to create new document from the template: {templateName}") {
        }
    }
}
