//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using XCad.Sw.Documents.Services;

namespace XCad.Sw.Documents.Attributes {
    /// <summary>
    /// This attribute can be used on the <see cref="IDocumentHandler"/> implementation to specify the scope where this handler should be created
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class DocumentHandlerFilterAttribute : Attribute {
        /// <summary>
        /// Handler scope
        /// </summary>
        public Type[] Filters { get; }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="filters">Filters for document handler (e.g. <see cref="IXPart"/></param>, <see cref="ISwAssembly"/>, <see cref="ISwDrawing"/>)
        public DocumentHandlerFilterAttribute(params Type[] filters) {
            Filters = filters;
        }
    }
}
