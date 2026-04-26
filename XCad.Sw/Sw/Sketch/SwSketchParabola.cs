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
    public interface ISwSketchParabola : ISwSketchSegment {
        ISketchParabola Parabola { get; }
    }

    internal class SwSketchParabola : SwSketchSegment, ISwSketchParabola {
        public ISketchParabola Parabola => (ISketchParabola)Segment;

        public override ISwSketchPoint StartPoint => OwnerDocument.CreateObjectFromDispatch<SwSketchPoint>(Parabola.IGetStartPoint2());
        public override ISwSketchPoint EndPoint => OwnerDocument.CreateObjectFromDispatch<SwSketchPoint>(Parabola.IGetEndPoint2());

        internal SwSketchParabola(ISketchParabola parabola, SwDocument doc, SwApplication app, bool created)
            : base((ISketchSegment)parabola, doc, app, created) {
        }

        internal SwSketchParabola(SwSketchBase ownerSketch, SwDocument doc, SwApplication app) : base(ownerSketch, doc, app) {
        }

        protected override ISketchSegment CreateSketchEntity() {
            throw new NotImplementedException();
        }
    }
}
