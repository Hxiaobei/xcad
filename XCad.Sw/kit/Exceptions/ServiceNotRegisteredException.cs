//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;

namespace XCad.kit.Exceptions {
    public class ServiceNotRegisteredException : Exception {
        public ServiceNotRegisteredException(Type serviceType) : base($"Service '{serviceType.FullName}' is not registered") {
        }
    }
}
