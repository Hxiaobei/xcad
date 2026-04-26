//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using SolidWorks.Interop.sldworks;
using XCad.Structures;
using XCad.Sw.Documents;
using XCad.Sw.Extensions;
using XCad.Sw.Features;
using XCad.Sw.Geometry.Surfaces;
using XCad.Sw.Utils;
using Vec3d = XCad.Structures.Vec3d;

namespace XCad.Sw.Geometry {
    public interface ISwFace : ISwEntity, IHasColor, ISwRegion {
        bool Sense { get; }
        double Area { get; }
        IFace2 Face { get; }
        ISwSurface Definition { get; }

        /// <summary>
        /// Returns the feature which owns this face
        /// </summary>
        ISwFeature Feature { get; }

        /// <summary>
        /// Projects the specified point onto the surface
        /// </summary>
        /// <param name="point">Input point</param>
        /// <param name="direction">Projection direction</param>
        /// <param name="projectedPoint">Projected point or null</param>
        /// <returns>True if projected point is found, false - if not</returns>
        bool TryProjectPoint(Vec3d point, Vec3d direction, out Vec3d projectedPoint);

        /// <summary>
        /// Finds the boundary of this face
        /// </summary>
        /// <param name="uMin">Minimum u-parameter</param>
        /// <param name="uMax">Maximum u-parameter</param>
        /// <param name="vMin">Minimum v-parameter</param>
        /// <param name="vMax">Maximum v-parameter</param>
        void GetUVBoundary(out double uMin, out double uMax, out double vMin, out double vMax);

        /// <summary>
        /// Finds u and v parameters of the face based on the point location
        /// </summary>
        /// <param name="point">Point</param>
        /// <param name="uParam">U-parameter</param>
        /// <param name="vParam">V-parameter</param>
        void CalculateUVParameter(Vec3d point, out double uParam, out double vParam);
    }

    internal class SwFaceAdjacentEntitiesRepository : SwEntityRepository {
        private readonly SwFace m_Face;

        internal SwFaceAdjacentEntitiesRepository(SwFace face) {
            m_Face = face;
        }

        protected override IEnumerable<ISwEntity> IterateEntities(bool faces, bool edges, bool vertices, bool silhouetteEdges) {
            IEnumerable<IVertex> EnumerateVertices(IEdge edge) {
                var startVertex = edge.IGetStartVertex();

                if(startVertex != null) {
                    yield return startVertex;
                }

                var endVertex = edge.IGetEndVertex();

                if(endVertex != null)//vertex is null for the closed curves
                {
                    yield return endVertex;
                }
            }

            if(edges) {
                foreach(var edge in (m_Face.Face.GetEdges().ToSwArray<IEdge>())) {
                    yield return m_Face.OwnerApplication.CreateObjectFromDispatch<ISwEdge>(edge, m_Face.OwnerDocument);
                }
            }

            if(vertices) {
                foreach(var vertex in m_Face.Face.GetEdges().ToSwArray<IEdge>().SelectMany(EnumerateVertices).Distinct()) {
                    yield return m_Face.OwnerApplication.CreateObjectFromDispatch<ISwVertex>(vertex, m_Face.OwnerDocument);
                }
            }
        }
    }

    internal abstract class SwFace : SwEntity, ISwFace {
        public IFace2 Face { get; }

        protected SwFace(IFace2 face, SwDocument doc, SwApplication app) : base((IEntity)face, doc, app) {
            Face = face;
            AdjacentEntities = new SwFaceAdjacentEntitiesRepository(this);
        }

        public override ISwBody Body => OwnerApplication.CreateObjectFromDispatch<ISwBody>(Face.GetBody(), OwnerDocument);

        public override ISwEntityRepository AdjacentEntities { get; }

        public double Area => Face.GetArea();

        private IComponent2 GetSwComponent() => (Face as IEntity).GetComponent() as IComponent2;

        public System.Drawing.Color? Color {
            get => SwColorHelper.GetColor(GetSwComponent(),
                (o, c) => Face.GetMaterialPropertyValues2((int)o, c) as double[]);
            set => SwColorHelper.SetColor(value, GetSwComponent(),
                (m, o, c) => Face.SetMaterialPropertyValues2(m, (int)o, c),
                (o, c) => Face.RemoveMaterialProperty2((int)o, c));
        }

        public override bool IsAlive {
            get {
                if(base.IsAlive) {
                    //some of the faces may be broken and have negative area. Working with these faces may resut in the SOLIDWORKS crash
                    return Face.GetArea() > 0;
                } else {
                    return false;
                }
            }
        }

        public ISwSurface Definition => OwnerApplication.CreateObjectFromDispatch<SwSurface>(Face.IGetSurface(), OwnerDocument);

        private IEnumerable<ISwLoop> IterateLoops() {
            var loops = Face.GetLoops().ToSwArray<ILoop2>();

            foreach(var loop in loops) {
                yield return OwnerApplication.CreateObjectFromDispatch<ISwLoop>(loop, OwnerDocument);
            }
        }

        public ISwFeature Feature {
            get {
                var feat = Face.IGetFeature();

                if(feat != null) {
                    return OwnerDocument.CreateObjectFromDispatch<ISwFeature>(feat);
                } else {
                    return null;
                }
            }
        }

        public bool Sense => Face.FaceInSurfaceSense();

        public ISwLoop OuterLoop {
            get => IterateLoops().First(l => l.Loop.IsOuter());
            set => throw new NotSupportedException();
        }

        public ISwLoop[] InnerLoops {
            get => IterateLoops().Where(l => !l.Loop.IsOuter()).ToArray();
            set => throw new NotSupportedException();
        }

        public override Vec3d FindClosestPoint(Vec3d point)
            => new Vec3d(((double[])Face.GetClosestPointOn(point.X, point.Y, point.Z)).Take(3).ToArray());

        public bool TryProjectPoint(Vec3d point, Vec3d direction, out Vec3d projectedPoint) {
            var dirVec = direction.ToSwVec();
            var startPt = point.ToSwPt();

            var resPt = Face.GetProjectedPointOn(startPt, dirVec);

            if(resPt != null) {
                projectedPoint = resPt.ToXa();
                return true;
            } else {
                projectedPoint = Vec3d.NaN;
                return false;
            }
        }

        public void GetUVBoundary(out double uMin, out double uMax, out double vMin, out double vMax) {
            var uvBounds = (double[])Face.GetUVBounds();

            uMin = uvBounds[0];
            uMax = uvBounds[1];
            vMin = uvBounds[2];
            vMax = uvBounds[3];
        }

        public void CalculateUVParameter(Vec3d point, out double uParam, out double vParam) {
            var uvParam = (double[])Face.ReverseEvaluate(point.X, point.Y, point.Z);

            if(uvParam != null) {
                uParam = uvParam[0];
                vParam = uvParam[1];
            } else {
                throw new NullReferenceException("Failed to extract UV parameters of the face. This may indicate that input point does not lie on the face");
            }
        }
    }

    public interface ISwPlanarFace : ISwFace, ISwPlanarRegion {
        new ISwPlanarSurface Definition { get; }
        Vec3d GetNormal { get; }
    }

    internal class SwPlanarFace : SwFace, ISwPlanarFace {
        ISwLoop ISwRegion.OuterLoop { get => OuterLoop; set => OuterLoop = value; }
        ISwLoop[] ISwRegion.InnerLoops { get => InnerLoops.ToSwArray<ISwLoop>(); set => InnerLoops = value; }

        public SwPlanarFace(IFace2 face, SwDocument doc, SwApplication app) : base(face, doc, app) {
        }

        public new ISwPlanarSurface Definition => OwnerApplication.CreateObjectFromDispatch<SwPlanarSurface>(Face.IGetSurface(), OwnerDocument);

        public Plane Plane => Definition.Plane;

        public ISwPlanarSheetBody PlanarSheetBody {
            get {
                var sheetBody = Face.CreateSheetBody();
                return OwnerApplication.CreateObjectFromDispatch<ISwPlanarSheetBody>(sheetBody, OwnerDocument);
            }
        }

        public Vec3d GetNormal => Definition.Plane.Normal * (Sense ? -1 : 1);
    }

    public interface ISwCylindricalFace : ISwFace {
        new ISwCylindricalSurface Definition { get; }
    }

    internal class SwCylindricalFace : SwFace, ISwCylindricalFace {
        public SwCylindricalFace(IFace2 face, SwDocument doc, SwApplication app) : base(face, doc, app) { }

        public new ISwCylindricalSurface Definition
            => OwnerApplication.CreateObjectFromDispatch<SwCylindricalSurface>(Face.IGetSurface(), OwnerDocument);
    }

    public interface ISwToroidalFace : ISwFace { new IXToroidalSurface Definition { get; } }

    internal class SwToroidalFace : SwFace, ISwToroidalFace {
        public SwToroidalFace(IFace2 face, SwDocument doc, SwApplication app) : base(face, doc, app) {
        }
        IXToroidalSurface ISwToroidalFace.Definition => (IXToroidalSurface)base.Definition;
    }
}
