//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using SolidWorks.Interop.sldworks;
using XCad.Sw.Documents;
using XCad.Sw.Features;

namespace XCad.Sw.Sketch {
    public interface ISwSketchText : ISwSketchSegment {
        ISketchText TextSegment { get; }
    }

    internal class SwSketchText : SwSketchSegment, ISwSketchText {
        public ISketchText TextSegment => (ISketchText)Segment;

        public override ISwSketchPoint StartPoint => throw new NotSupportedException();
        public override ISwSketchPoint EndPoint => throw new NotSupportedException();

        internal SwSketchText(ISketchText textSeg, SwDocument doc, SwApplication app, bool created)
            : base((ISketchSegment)textSeg, doc, app, created) {
        }

        internal SwSketchText(SwSketchBase ownerSketch, SwDocument doc, SwApplication app) : base(ownerSketch, doc, app) {
        }

        protected override ISketchSegment CreateSketchEntity() {
            throw new NotImplementedException();
        }
    }
}
