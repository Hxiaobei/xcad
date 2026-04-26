//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using XCad.Sw.Exceptions;

namespace XCad.Sw.Data.Exceptions {
    public class CustomPropertyUnloadedConfigException : Exception, IUserException {
        public CustomPropertyUnloadedConfigException()
            : base("Custom property is not added to unloaded configuration. Try activate configuration before adding the property") {
        }
    }
}
