//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;

namespace XCad.Sw.Documents.Services {
    /// <summary>
    /// Document handler to be used in <see cref="ISwDocumentCollection.RegisterHandler{THandler}(Func{THandler})"/> documents manager
    /// </summary>
    public interface IDocumentHandler : IDisposable {
        /// <summary>
        /// Called when model document is initialized (created)
        /// </summary>
        /// <param name="app">Pointer to application</param>
        /// <param name="doc">Pointer to this model document</param>
        void Init(ISwApplication app, ISwDocument doc);
    }
}
