//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SolidWorks.Interop.sldworks;
using XCad.Structures;
using XCad.Sw.Documents;
using XCad.Sw.Extensions;
using XCad.Sw.Geometry.Wires;

namespace XCad.Sw.Geometry {
    public interface ISwVertex : ISwEntity, IXPoint {
        IVertex Vertex { get; }
    }

    internal class SwVertexAdjacentEntitiesRepository : SwEntityRepository {
        private readonly SwVertex m_Vertex;

        internal SwVertexAdjacentEntitiesRepository(SwVertex vertex) {
            m_Vertex = vertex;
        }

        protected override IEnumerable<ISwEntity> IterateEntities(bool faces, bool edges, bool vertices, bool silhouetteEdges) {
            if(edges) {
                foreach(IEdge edge in m_Vertex.Vertex.GetEdges().ToSwArray<IEdge>()) {
                    yield return m_Vertex.OwnerApplication.CreateObjectFromDispatch<SwEdge>(edge, m_Vertex.OwnerDocument);
                }
            }

            if(faces) {
                foreach(IFace2 face in m_Vertex.Vertex.GetAdjacentFaces().ToSwArray<IFace2>()) {
                    yield return m_Vertex.OwnerApplication.CreateObjectFromDispatch<SwFace>(face, m_Vertex.OwnerDocument);
                }
            }
        }
    }

    [DebuggerDisplay("{" + nameof(Coordinate) + "}")]
    internal class SwVertex : SwEntity, ISwVertex {
        public IVertex Vertex { get; }

        public Vec3d Coordinate {
            get => new Vec3d((double[])Vertex.GetPoint());
            set => throw new NotSupportedException("Coordinate of the vertex cannot be changed");
        }

        public override ISwBody Body => OwnerApplication.CreateObjectFromDispatch<ISwBody>(
            Vertex.GetEdges().ToSwArray<IEdge>().First().GetBody(), OwnerDocument);


        public override ISwEntityRepository AdjacentEntities { get; }

        public override Vec3d FindClosestPoint(Vec3d point)
            => new Vec3d(((double[])Vertex.GetClosestPointOn(point.X, point.Y, point.Z)).Take(3).ToArray());

        internal SwVertex(IVertex vertex, SwDocument doc, SwApplication app) : base((IEntity)vertex, doc, app) {
            Vertex = vertex;
            AdjacentEntities = new SwVertexAdjacentEntitiesRepository(this);
        }
    }
}
