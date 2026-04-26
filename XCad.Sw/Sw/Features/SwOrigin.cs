//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Threading;
using SolidWorks.Interop.sldworks;
using XCad.Sw.Documents;

namespace XCad.Sw.Features {
    internal class SwOrigin : SwFeature, ISwFeature {
        internal const string TypeName = "OriginProfileFeature";

        internal SwOrigin(IFeature feat, SwDocument doc, SwApplication app, bool created) : base(feat, doc, app, created) {
        }

        public override bool IsUserFeature => false;

        protected override IFeature InsertFeature(CancellationToken cancellationToken)
            => throw new NotSupportedException("Origin is a default feature and cannot be created");
    }
}
