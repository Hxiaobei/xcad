//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;

namespace XCad.Sw.UI.Toolkit {
    /// <summary>
    /// Indicates that this item will be disposed when main add-in is unloaded
    /// </summary>
    internal interface IAutoDisposable : IDisposable {
        event Action<IAutoDisposable> Disposed;
    }
}
