//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Linq;
using SolidWorks.Interop.sldworks;
using XCad.Sw.Documents;
using XCad.Sw.Extensions;
using XCad.Sw.Features;
using XCad.Sw.Geometry.Wires;

namespace XCad.Sw.Sketch {
    public interface ISwSketchSpline : ISwSketchSegment, IXSegment {
        ISketchSpline Spline { get; }
    }

    internal class SwSketchSpline : SwSketchSegment, ISwSketchSpline {
        public ISketchSpline Spline => (ISketchSpline)Segment;

        public override ISwSketchPoint StartPoint => OwnerDocument.CreateObjectFromDispatch<SwSketchPoint>(Spline.GetPoints2().ToSwArray<object>().First());
        public override ISwSketchPoint EndPoint => OwnerDocument.CreateObjectFromDispatch<SwSketchPoint>(Spline.GetPoints2().ToSwArray<object>().Last());

        internal SwSketchSpline(ISketchSpline spline, SwDocument doc, SwApplication app, bool created)
            : base((ISketchSegment)spline, doc, app, created) {
        }

        internal SwSketchSpline(SwSketchBase ownerSketch, SwDocument doc, SwApplication app) : base(ownerSketch, doc, app) {
        }

        protected override ISketchSegment CreateSketchEntity() {
            throw new NotImplementedException();
        }
    }
}
