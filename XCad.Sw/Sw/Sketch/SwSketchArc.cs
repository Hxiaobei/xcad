//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using SolidWorks.Interop.sldworks;
using XCad.Structures;
using XCad.Sw.Documents;
using XCad.Sw.Features;
using XCad.Sw.Geometry.Wires;

namespace XCad.Sw.Sketch {
    public interface ISwSketchCircle : IXCircle, ISwSketchSegment {

        ISketchArc Arc { get; }
    }

    public interface ISwSketchArc : ISwSketchCircle {
        /// <summary>
        /// Start point of the arc
        /// </summary>
        Vec3d Start { get; set; }

        /// <summary>
        /// End point of the arc
        /// </summary>
        Vec3d End { get; set; }
    }

    internal class SwSketchCircle : SwSketchSegment, ISwSketchCircle {
        public ISketchArc Arc => (ISketchArc)Segment;

        public override ISwSketchPoint StartPoint => OwnerDocument.CreateObjectFromDispatch<SwSketchPoint>(Arc.IGetStartPoint2());
        public override ISwSketchPoint EndPoint => OwnerDocument.CreateObjectFromDispatch<SwSketchPoint>(Arc.IGetEndPoint2());

        public Circle Geometry {
            get {
                var centerPt = CreatePoint((ISketchPoint)Arc.GetCenterPoint2());
                var diam = Arc.GetRadius() * 2;

                var norm = (double[])Arc.GetNormalVector();

                return new Circle(new Axis(centerPt, new Vec3d(norm)), diam);
            }
            set {
                Arc.SetRadius(value.Diameter / 2);
                SetPoint((ISketchPoint)Arc.GetCenterPoint2(), value.CenterAxis.Point);
                //TODO: implement changing of the axis
            }
        }

        internal SwSketchCircle(ISketchArc arc, SwDocument doc, SwApplication app, bool created)
            : base((ISketchSegment)arc, doc, app, created) {
        }

        internal SwSketchCircle(SwSketchBase ownerSketch, SwDocument doc, SwApplication app) : base(ownerSketch, doc, app) {
        }

        protected override ISketchSegment CreateSketchEntity() {
            throw new NotImplementedException();
        }

        protected Vec3d CreatePoint(ISketchPoint pt) => new(pt.X, pt.Y, pt.Z);

        protected void SetPoint(ISketchPoint pt, Vec3d coord) {
            pt.X = coord.X;
            pt.Y = coord.Y;
            pt.Z = coord.Z;
        }
    }

    internal class SwSketchArc : SwSketchCircle, ISwSketchArc {
        public Vec3d Start {
            get => CreatePoint((ISketchPoint)Arc.GetStartPoint2());
            set => SetPoint((ISketchPoint)Arc.GetStartPoint2(), value);
        }

        public Vec3d End {
            get => CreatePoint((ISketchPoint)Arc.GetEndPoint2());
            set => SetPoint((ISketchPoint)Arc.GetEndPoint2(), value);
        }

        internal SwSketchArc(ISketchArc arc, SwDocument doc, SwApplication app, bool created) : base(arc, doc, app, created) {
        }

        internal SwSketchArc(SwSketchBase ownerSketch, SwDocument doc, SwApplication app) : base(ownerSketch, doc, app) {
        }
    }
}
