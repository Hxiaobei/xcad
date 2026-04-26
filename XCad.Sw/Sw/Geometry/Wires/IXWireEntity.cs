//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using XCad.Sw.Base;

namespace XCad.Sw.Geometry.Wires {
    /// <summary>
    /// Represents the common entity for <see cref="IXPoint"/> and <see cref="IXSegment"/>
    /// </summary>
    public interface IXWireEntity : IXTransaction, ISwObject {
    }
}
