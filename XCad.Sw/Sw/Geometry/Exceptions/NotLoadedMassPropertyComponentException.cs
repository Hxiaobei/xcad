//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using XCad.Sw.Documents;

namespace XCad.Sw.Geometry.Exceptions {
    /// <summary>
    /// SOLIDOWORKS API limitation of not-loaded components mass property calculation in SOLIDWORKS 2019 or older
    /// </summary>
    public class NotLoadedMassPropertyComponentException : NotSupportedException {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="comp">Component</param>
        public NotLoadedMassPropertyComponentException(ISwComponent comp)
            : base($"Reference document of the component '{comp.Name}' must be loaded in order to access this mass property in SOLIDWORKS 2019 or older") {
        }
    }
}
