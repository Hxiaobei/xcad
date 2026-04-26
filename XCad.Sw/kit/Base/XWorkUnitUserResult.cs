//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using XCad.Sw.Base;

namespace XCad.kit.Base {
    public class XWorkUnitUserResult<TRes> : IXWorkUnitUserResult<TRes> {
        public TRes Result { get; }

        public XWorkUnitUserResult(TRes result) {
            Result = result;
        }
    }
}
