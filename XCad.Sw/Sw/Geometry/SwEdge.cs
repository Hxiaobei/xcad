//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Collections.Generic;
using System.Linq;
using SolidWorks.Interop.sldworks;
using XCad.Structures;
using XCad.Sw.Documents;
using XCad.Sw.Extensions;
using XCad.Sw.Geometry.Curves;
using XCad.Sw.Geometry.Wires;

namespace XCad.Sw.Geometry {

    public interface ISwEdge : ISwEntity, IXSegment {
        bool Sense { get; }
        IEdge Edge { get; }
        ISwCurve Definition { get; }
        new ISwVertex StartPoint { get; }
        new ISwVertex EndPoint { get; }
    }

    internal class SwEdgeAdjacentEntitiesRepository : SwEntityRepository {
        private readonly SwEdge m_Edge;

        internal SwEdgeAdjacentEntitiesRepository(SwEdge edge) {
            m_Edge = edge;
        }

        protected override IEnumerable<ISwEntity> IterateEntities(bool faces, bool edges, bool vertices, bool silhouetteEdges) {
            if(faces) {
                foreach(var face in m_Edge.Edge.GetTwoAdjacentFaces2().ToSwArray<IFace2>()) {
                    yield return m_Edge.OwnerApplication.CreateObjectFromDispatch<SwFace>(face, m_Edge.OwnerDocument);
                }
            }

            if(edges) {
                foreach(var coEdge in m_Edge.Edge.GetCoEdges().ToSwArray<ICoEdge>()) {
                    var edge = coEdge.GetEdge() as IEdge;
                    yield return m_Edge.OwnerApplication.CreateObjectFromDispatch<SwEdge>(edge, m_Edge.OwnerDocument);
                }
            }

            if(vertices) {
                var startVertex = m_Edge.StartPoint;

                if(startVertex != null) {
                    yield return startVertex;
                }

                var endVertex = m_Edge.EndPoint;

                if(endVertex != null) {
                    yield return endVertex;
                }
            }
        }
    }

    internal class SwEdge : SwEntity, ISwEdge {
        IXPoint IXSegment.StartPoint => StartPoint;
        IXPoint IXSegment.EndPoint => EndPoint;

        ISwVertex ISwEdge.StartPoint => StartPoint;
        ISwVertex ISwEdge.EndPoint => EndPoint;

        ISwCurve ISwEdge.Definition => Definition;

        public IEdge Edge { get; }

        public override ISwBody Body => OwnerApplication.CreateObjectFromDispatch<SwBody>(Edge.GetBody(), OwnerDocument);

        public override ISwEntityRepository AdjacentEntities { get; }

        public ISwCurve Definition => OwnerApplication.CreateObjectFromDispatch<SwCurve>(Edge.IGetCurve(), OwnerDocument);

        public ISwVertex StartPoint {
            get {
                var vertex = Edge.IGetStartVertex();

                if(vertex != null) {
                    return OwnerApplication.CreateObjectFromDispatch<ISwVertex>(vertex, OwnerDocument);
                } else {
                    return null;
                }
            }
        }

        public ISwVertex EndPoint {
            get {
                var vertex = Edge.IGetEndVertex();

                if(vertex != null) {
                    return OwnerApplication.CreateObjectFromDispatch<ISwVertex>(vertex, OwnerDocument);
                } else {
                    return null;
                }
            }
        }

        public double Length => Definition.Length;

        public bool Sense {
            get {
                var curveParams = Edge.GetCurveParams3();
                return curveParams.Sense;
            }
        }

        public override Vec3d FindClosestPoint(Vec3d point)
            => new Vec3d(((double[])Edge.GetClosestPointOn(point.X, point.Y, point.Z)).Take(3).ToArray());

        internal SwEdge(IEdge edge, SwDocument doc, SwApplication app) : base((IEntity)edge, doc, app) {
            Edge = edge;
            AdjacentEntities = new SwEdgeAdjacentEntitiesRepository(this);
        }
    }

    public interface ISwCircularEdge : ISwEdge {
        new ISwCircleCurve Definition { get; }
    }

    internal class SwCircularEdge : SwEdge {
        internal SwCircularEdge(IEdge edge, SwDocument doc, SwApplication app) : base(edge, doc, app) {
        }

        public new ISwCircleCurve Definition => OwnerApplication.CreateObjectFromDispatch<SwCircleCurve>(Edge.IGetCurve(), OwnerDocument);
    }

    public interface ISwLinearEdge : ISwEdge {
        new ISwLineCurve Definition { get; }
    }

    internal class SwLinearEdge : SwEdge, ISwLinearEdge {
        internal SwLinearEdge(IEdge edge, SwDocument doc, SwApplication app) : base(edge, doc, app) {
        }

        public new ISwLineCurve Definition => OwnerApplication.CreateObjectFromDispatch<SwLineCurve>(Edge.IGetCurve(), OwnerDocument);
    }
}