//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using XCad.Structures;
using XCad.Sw.Documents;
using XCad.Sw.Features;
using XCad.Sw.Geometry.Wires;

namespace XCad.Sw.Sketch {
    public interface ISwSketchLine : IXLine, ISwSketchSegment {
        ISketchLine Line { get; }
    }

    internal class SwSketchLine : SwSketchSegment, ISwSketchLine {
        public ISketchLine Line => (ISketchLine)Segment;

        public override ISwSketchPoint StartPoint => OwnerDocument.CreateObjectFromDispatch<SwSketchPoint>(Line.IGetStartPoint2());
        public override ISwSketchPoint EndPoint => OwnerDocument.CreateObjectFromDispatch<SwSketchPoint>(Line.IGetEndPoint2());

        public Line Geometry {
            get {
                if(IsCommitted) {
                    return new Line(StartPoint.Coordinate, EndPoint.Coordinate);
                } else {
                    return m_Creator.CachedProperties.Get<Line>();
                }
            }
            set {
                if(IsCommitted) {
                    StartPoint.Coordinate = value.StartPoint;
                    EndPoint.Coordinate = value.EndPoint;
                } else {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        internal SwSketchLine(ISketchLine line, SwDocument doc, SwApplication app, bool created)
            : base((ISketchSegment)line, doc, app, created) {
        }

        internal SwSketchLine(SwSketchBase ownerSketch, SwDocument doc, SwApplication app) : base(ownerSketch, doc, app) {
        }

        protected override ISketchSegment CreateSketchEntity() {
            var geom = Geometry;

            var line = (ISketchLine)m_SketchMgr.CreateLine(
                geom.StartPoint.X,
                geom.StartPoint.Y,
                geom.StartPoint.Z,
                geom.EndPoint.X,
                geom.EndPoint.Y,
                geom.EndPoint.Z);

            return (ISketchSegment)line;
        }
    }
}