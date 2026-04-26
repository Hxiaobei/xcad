//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using XCad.UI.Commands.Structures;

namespace XCad.UI.Commands.Delegates {
    /// <summary>
    /// Delegate of <see cref="IXCommandGroup.CommandClick"/>
    /// </summary>
    /// <param name="spec">Command specification</param>
    public delegate void CommandClickDelegate(CommandSpec spec);

    /// <summary>
    /// Delegate of specific <see cref="IXCommandGroup.CommandClick"/>
    /// </summary>
    /// <typeparam name="TCmdEnum">Enumaration type</typeparam>
    /// <param name="spec">Enumeration value</param>
    public delegate void CommandEnumClickDelegate<TCmdEnum>(TCmdEnum spec)
        where TCmdEnum : Enum;
}