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

namespace XCad.Sw.Documents.Delegates {
    /// <summary>
    /// Delegate of <see cref="ISwAssembly.ComponentDeleted"/> notification
    /// </summary>
    /// <param name="assembly">Assembly where component is deleted</param>
    /// <param name="component">Component deleted from the assembly. The pointer to the component may be disconnected from the client</param>
    public delegate void ComponentDeletedDelegate(ISwAssembly assembly, ISwComponent component);
}
