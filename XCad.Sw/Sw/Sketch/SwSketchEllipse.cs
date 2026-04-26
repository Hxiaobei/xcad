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
    public interface ISwSketchEllipse : ISwSketchSegment {
        ISketchEllipse Ellipse { get; }
    }

    internal class SwSketchEllipse : SwSketchSegment, ISwSketchEllipse {
        public ISketchEllipse Ellipse => (ISketchEllipse)Segment;

        public override ISwSketchPoint StartPoint => OwnerDocument.CreateObjectFromDispatch<SwSketchPoint>(Ellipse.IGetStartPoint2());
        public override ISwSketchPoint EndPoint => OwnerDocument.CreateObjectFromDispatch<SwSketchPoint>(Ellipse.IGetEndPoint2());

        internal SwSketchEllipse(ISketchEllipse ellipse, SwDocument doc, SwApplication app, bool created)
            : base((ISketchSegment)ellipse, doc, app, created) {
            if(doc == null) {
                throw new ArgumentNullException(nameof(doc));
            }
        }

        internal SwSketchEllipse(SwSketchBase ownerSketch, SwDocument doc, SwApplication app) : base(ownerSketch, doc, app) {
        }

        protected override ISketchSegment CreateSketchEntity() {
            throw new NotImplementedException();
        }
    }
}
