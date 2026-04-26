//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;

namespace XCad.Sw.Documents.Delegates {
    /// <summary>
    /// Delegate used in the <see cref="XDocumentDependenciesExtension.ReplaceAll(IXDocumentDependencies, ReplaceReferencePathProviderDelegate, Func{string, string})"/>
    /// </summary>
    /// <param name="srcPath">Path to be replaced</param>
    /// <returns>Replacement path (can be the same if reference does not need to be replaced)</returns>
    public delegate string ReplaceReferencePathProviderDelegate(string srcPath);
}
