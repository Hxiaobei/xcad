//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Threading;
using SolidWorks.Interop.sldworks;
using XCad.Structures;
using XCad.Sw.Documents;
using XCad.Sw.Geometry.Exceptions;
using XCad.Sw.Geometry.Wires;

namespace XCad.Sw.Geometry.Curves {
    public interface ISwLineCurve : IXLine, ISwCurve {
    }

    internal class SwLineCurve : SwCurve, ISwLineCurve {
        internal SwLineCurve(ICurve curve, SwDocument doc, SwApplication app, bool isCreated)
            : base(curve, doc, app, isCreated) {
        }

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
                    throw new CommitedSegmentReadOnlyParameterException();
                } else {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        protected override ICurve[] Create(CancellationToken cancellationToken) {
            var geom = Geometry;

            var line = SwUtils.Modeler.CreateLine(geom.StartPoint.ToArray(), (geom.StartPoint - geom.EndPoint).ToArray()) as ICurve;
            line = line.CreateTrimmedCurve2(geom.StartPoint.X, geom.StartPoint.Y, geom.StartPoint.Z, geom.EndPoint.X, geom.EndPoint.Y, geom.EndPoint.Z)
                ?? throw new NullReferenceException("Failed to create line");
            return new ICurve[] { line };
        }
    }
}
