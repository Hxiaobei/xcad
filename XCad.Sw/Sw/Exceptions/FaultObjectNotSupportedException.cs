//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using XCad.Sw.Features.CustomFeature;

namespace XCad.Sw.Exceptions {
    /// <summary>
    /// Exception is thrown for all properties and methods of <see cref="IFaultObject"/>
    /// </summary>
    public class FaultObjectNotSupportedException : NotSupportedException {
        /// <summary>
        /// Default constructor
        /// </summary>
        public FaultObjectNotSupportedException() : base("Accessing methods and properties of a fault object is not supported") {
        }
    }
}
