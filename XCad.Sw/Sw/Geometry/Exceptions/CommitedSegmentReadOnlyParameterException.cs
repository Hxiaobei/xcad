//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using XCad.Sw.Exceptions;

namespace XCad.Sw.Geometry.Exceptions {
    public class CommitedSegmentReadOnlyParameterException : CommitedElementReadOnlyParameterException {
        public CommitedSegmentReadOnlyParameterException() : base("Parameter cannot be modified after entity is committed") {
        }
    }
}
