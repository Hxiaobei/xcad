//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

namespace XCad.Sw {
    /// <summary>
    /// Indicates that object has name
    /// </summary>
    public interface IHasName : ISwObject {
        /// <summary>
        /// Name of this element
        /// </summary>
        string Name { get; set; }
    }
}
