using System;
using System.Collections.Generic;
using System.Linq;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using XCad.Structures;
using XCad.Sw.Extensions;

namespace XCad.Sw.Geometry {
    public enum RuledOperation {
        Tangent = 0,
        Normal = 1,
        Tapered = 2,
        Perpendicular = 3,
        Seep = 4,
    }

    /// <summary>
    /// Geometric extension methods and utilities
    /// </summary>
    public static class GeometryEx {
        private static int m_Error;
        //private static IModeler Modeler => SwUtils.Modeler;
        // Wait, SwUtils.Math is IMathUtility. Modeler is SwUtils.Modeler.
        // Let's check SwUtils once more.
        // SwUtils has Math (IMathUtility) and Modeler (IModeler).

        public static double PackIntPair(int lower, int upper) {
            var bytes = new byte[8];
            BitConverter.GetBytes(lower).CopyTo(bytes, 0);
            BitConverter.GetBytes(upper).CopyTo(bytes, 4);
            return BitConverter.ToDouble(bytes, 0);
        }

        public static void UnpackIntPair(double value, out int lower, out int upper) {
            var bytes = BitConverter.GetBytes(value);
            lower = BitConverter.ToInt32(bytes, 0);
            upper = BitConverter.ToInt32(bytes, 4);
        }

        public static ICurve CreateLine(Vec3d sPt, Vec3d ePt) {
            var dir = ePt - sPt;
            var curve = (ICurve)SwUtils.Modeler.CreateLine(sPt.ToArray(), dir.ToArray());
            return curve.CreateTrimmedCurve2(sPt.X, sPt.Y, sPt.Z, ePt.X, ePt.Y, ePt.Z);
        }

        public static ICurve CreateArc(Vec3d center, Vec3d normal, double radius, Vec3d startPt, Vec3d endPt) {
            var arc = (ICurve)SwUtils.Modeler.CreateArc(center.ToArray(), normal.ToArray(), radius, startPt.ToArray(), endPt.ToArray());
            return arc.CreateTrimmedCurve2(startPt.X, startPt.Y, startPt.Z, endPt.X, endPt.Y, endPt.Z);
        }

        public static ICurve CreateCircle(Vec3d center, Vec3d normal, double radius) {
            var vX = normal.CreateAnyPerpendicular();
            Vec3d startPt = center + vX * radius;

            return (ICurve)SwUtils.Modeler.CreateArc(center.ToArray(), normal.ToArray(), radius, startPt.ToArray(), startPt.ToArray());
        }

        public static ICurve[] CreateRectangle(Vec3d center, Vec3d normal, double width, double height) {
            var xDir = normal.CreateAnyPerpendicular();
            var yDir = normal.Cross(xDir).Normalize();

            var halfW = width / 2;
            var halfH = height / 2;

            var pt1 = center + (xDir.Scale(-halfW)) + (yDir.Scale(-halfH));
            var pt2 = center + (xDir.Scale(halfW)) + (yDir.Scale(-halfH));
            var pt3 = center + (xDir.Scale(halfW)) + (yDir.Scale(halfH));
            var pt4 = center + (xDir.Scale(-halfW)) + (yDir.Scale(halfH));

            return new[] { CreateLine(pt1, pt2), CreateLine(pt2, pt3), CreateLine(pt3, pt4), CreateLine(pt4, pt1) };
        }

        public static ICurve[] CreateTrapezoid(Vec3d center, Vec3d normal, double lowerBase, double upperBase, double height) {
            var xDir = normal.CreateAnyPerpendicular();
            var yDir = normal.Cross(xDir).Normalize();

            var halfHeight = height / 2;

            var lowerLeft = center + xDir.Scale(-lowerBase / 2) + yDir.Scale(-halfHeight);
            var lowerRight = center + xDir.Scale(lowerBase / 2) + yDir.Scale(-halfHeight);
            var upperRight = center + xDir.Scale(upperBase / 2) + yDir.Scale(halfHeight);
            var upperLeft = center + xDir.Scale(-upperBase / 2) + yDir.Scale(halfHeight);

            return new[] { CreateLine(lowerLeft, lowerRight), CreateLine(lowerRight, upperRight),
                CreateLine(upperRight, upperLeft), CreateLine(upperLeft, lowerLeft) };
        }

        public static ICurve[] CreateSlot(Vec3d center, Vec3d normal, double length, double radius) {
            var xDir = normal.CreateAnyPerpendicular();
            var yDir = normal.Cross(xDir).Normalize();

            var halfL = length / 2;

            var arcCenter1 = center + xDir.Scale(-halfL);
            var arcCenter2 = center + xDir.Scale(halfL);

            var ptTop1 = arcCenter1 + yDir.Scale(radius);
            var ptBot1 = arcCenter1 + yDir.Scale(-radius);
            var ptTop2 = arcCenter2 + yDir.Scale(radius);
            var ptBot2 = arcCenter2 + yDir.Scale(-radius);

            return new[] { CreateArc(arcCenter1, normal, radius, ptTop1, ptBot1),
                           CreateLine(ptBot1, ptBot2),
                           CreateArc(arcCenter2, normal, radius, ptBot2, ptTop2),
                           CreateLine(ptTop2, ptTop1) };
        }

        public static ICurve[] CreatePolygon(Vec3d center, Vec3d normal, double radius, int sides) {
            if(sides < 3) throw new ArgumentException("多边形至少需要 3 条边");

            var xDir = normal.CreateAnyPerpendicular();
            var yDir = normal.Cross(xDir).Normalize();

            var points = new Vec3d[sides];
            for(int i = 0; i < sides; i++) {
                double angle = 2 * Math.PI * i / sides;
                points[i] = center + xDir.Scale(Math.Cos(angle) * radius) + yDir.Scale(Math.Sin(angle) * radius);
            }

            var curves = new ICurve[sides];
            for(int i = 0; i < sides; i++) {
                curves[i] = CreateLine(points[i], points[(i + 1) % sides]);
            }

            return curves;
        }

        public static ISurface CreatePlanar(Vec3d pt, Vec3d dir, Vec3d xAxis)
           => (ISurface)SwUtils.Modeler.CreatePlanarSurface2(pt.ToArray(), dir.ToArray(), xAxis.ToArray());

        public static ISurface CreatePlanar(Vec3d pt, Vec3d dir)
           => SwUtils.Modeler.CreatePlanarSurface(pt.ToArray(), dir.ToArray()) as Surface;

        public static ICurve CreateBsplineCurve(IReadOnlyCollection<double[]> controlPoints, IEnumerable<double> knotVectorU, int order, bool isPeriodic) {
            // SW control points in CreateBsplineCurve are 4D (X, Y, Z, W) if it's a rational spline
            // Or 3D if non-rational. Usually passed as SelectMany.
            var controlPointsList = controlPoints.SelectMany(p => p).ToArray();

            var props = new[] { PackIntPair(order, controlPoints.First().Length), PackIntPair(controlPoints.Count, isPeriodic ? 1 : 0) };

            return (Curve)SwUtils.Modeler.CreateBsplineCurve(props, knotVectorU, controlPointsList);
        }

        public static IBody2 CreateTrimmedSheet(ISurface planar, IEnumerable<ICurve> curves, bool preserveAnalytic, double tol = 1e-6)
          => (IBody2)planar.CreateTrimmedSheet5(curves.ToArray(), preserveAnalytic, tol);

        public static IBody2 CreateBox(Vec3d center, Vec3d axis, double xLen, double yLen, double zLen)
            => SwUtils.Modeler.CreateBodyFromBox3(new[] { center.X, center.Y, center.Z,
                                                     axis.X, axis.Y, axis.Z ,
                                                     xLen, yLen, zLen ,});

        public static IBody2 CreateSphereBody(Vec3d center, double radius) {
            var axis = Vec3d.UnitZ;
            var refAxis = Vec3d.UnitY;

            var sphere = (ISurface)SwUtils.Modeler.CreateSphericalSurface2(center.ToArray(), axis.ToArray(), refAxis.ToArray(), radius);
            var swSurfPara = sphere.Parameterization2();

            var uvrange = new[]
            {
                swSurfPara.UMin,
                swSurfPara.UMax,
                swSurfPara.VMin,
                swSurfPara.VMax
            };

            return (IBody2)SwUtils.Modeler.CreateSheetFromSurface(sphere, uvrange);
        }

        public static IBody2 CreateSheet(this IBody2 body) => SwUtils.Modeler.CreateSheetFromFaces(body.GetFaces2().ToArray());

        public static IBody2 CreateSheet(this IEnumerable<IFace2> faces) => SwUtils.Modeler.CreateSheetFromFaces(faces.ToArray());

        public static IBody2 ThickenSheet(this IBody2 sheet, in double thickness, swThickenDirection_e dir)
            => SwUtils.Modeler.ThickenSheet((Body2)sheet, thickness, (int)dir);

        public static IBody2 CreateSolid(this IBody2 body, IFace2[] faces = null, swCreateFacesBodyAction_e action = swCreateFacesBodyAction_e.swCreateFacesBodyActionCap) {
            if(faces == null) faces = body.GetFaces2().ToArray();
            return (IBody2)SwUtils.Modeler.CreateBodyFromFaces2(faces.Length, faces, (int)action, false, out _);
        }

        public static IBody2[] SutureBody(IEnumerable<IBody2> sheets, swSheetSewingOption_e options, double tol = 1e-6)
            => SwUtils.Modeler.CreateBodiesFromSheets2(sheets.ToArray(), (int)options, tol, ref m_Error).ToSwArray<IBody2>();

        public static IBody2 CreateExtrudedBody(this IBody2 sheet, Vec3d dir, double length)
            => SwUtils.Modeler.CreateExtrudedBody((Body2)sheet, dir.ToSwVec(), length);

        public static bool CreatePlanarSheet(this IBody2 tempBody, IEnumerable<Vec3d> pts) {
            var arr = new double[pts.Count() * 3];
            int i = 0;

            foreach(var p in pts) {
                arr[i++] = p.X;
                arr[i++] = p.Y;
                arr[i++] = p.Z;
            }

            return tempBody.CreatePlanarTrimSurfaceDLL(arr, null);
        }

        public static IBody2 CreateBlend(IBody2 body, IFace2 face1, double r1, IFace2 face2, double r2, Vec3d start, Vec3d end)
            => body.CreateBlendSurface(face1.IGetSurface(), r1, face2.IGetSurface(), r2,
                start.ToArray(), end.ToArray(), 0, null, 0, null) as IBody2;

        public static IBody2 CreateRuled(IModelDoc2 doc, IEnumerable<IEdge> edges, RuledOperation ruledType, double length,
            bool pullDirection, bool flipDirection, bool sewSurface, double angle,
            bool useInputVector, Vec3d directionVector, bool connectBodies) {
            SwUtils.Modeler.CreateRuledSurfaceFromEdge(
                 (ModelDoc2)doc,
                 edges.ToArray(),
                 (int)ruledType,
                 length,
                 pullDirection,
                 flipDirection,
                 sewSurface,
                 angle,
                 useInputVector,
                 directionVector.X,
                 directionVector.Y,
                 directionVector.Z,
                 connectBodies,
                 out var ruledBody);
            return (IBody2)ruledBody;
        }

        public static void Create(this IBody2 body) => body.CreateBaseFeature(body);

        public static IEnumerable<IBody2> Boolean(this IBody2 target, IBody2 tool, swBodyOperationType_e opType)
            => target.IOperations2((int)opType, (Body2)tool, out _)?.GetEnums();

        public static IBody2[] Boolean2(this IBody2 target, IBody2[] tools, swBodyOperationType_e opType, Dictionary<IFace2, IFace2> match = null, double tol = 1e-6) {
            int numFaces = match?.Count ?? 0;

            var result = target.MatchedBoolean4(
                (int)opType,
                tools,
                numFaces,
                numFaces > 0 ? match.Keys.ToArray() : null,
                numFaces > 0 ? match.Values.ToArray() : null,
                tol,
                out _);

            return result?.ToSwArray<IBody2>();
        }

        public static void Add(this IBody2 target, IBody2 tool) => target.ICombineVolumes((Body2)tool);

        public static bool ReplaceSurfaces(IFace2[] faces, ISurface[] surfaces, int[] senses = null, double tol = 1e-6) {
            if(faces.Length == 0 || surfaces.Length == 0) return false;

            if(surfaces.Length != faces.Length) {
                if(surfaces.Length != 1) return false;
                surfaces = Enumerable.Repeat((Surface)surfaces[0].Copy(), faces.Length).ToArray();
            }
            if(senses == null) senses = Enumerable.Repeat(0, faces.Length).ToArray();
            return SwUtils.Modeler.ReplaceSurfaces(faces.Length, faces, surfaces, senses, tol);
        }

        public static IBody2 Copy3(this IBody2 body, bool PreserveFaceIDs = false)
            => (IBody2)body.Copy2(PreserveFaceIDs);

        public static List<List<IFace2>> GroupFacesBySharedEdges(IFace2[] faces) {
            var adjacency = new Dictionary<IFace2, HashSet<IFace2>>(new FaceEqualityComparer());
            var edgeToFaces = new Dictionary<int, List<IFace2>>();
            var eId = 0;
            var fId = 0;
            foreach(var face in faces) {
                face.SetFaceId(++fId);
                if(!adjacency.ContainsKey(face)) adjacency[face] = new HashSet<IFace2>();

                var edges = face.EnumEdges()?.GetEnums();
                if(edges == null) continue;

                foreach(var edge in edges) {
                    var edgeId = edge.GetID();
                    if(edgeId == 0) {
                        edgeId = ++eId;
                        edge.SetId(edgeId);
                    }
                    if(!edgeToFaces.ContainsKey(edgeId)) {
                        edgeToFaces[edgeId] = new List<IFace2>();
                    }
                    foreach(var connectedFace in edgeToFaces[edgeId]) {
                        adjacency[face].Add(connectedFace);
                        adjacency[connectedFace].Add(face);
                    }

                    edgeToFaces[edgeId].Add(face);
                }
            }

            var groups = new List<List<IFace2>>();
            var visited = new HashSet<IFace2>(new FaceEqualityComparer());

            foreach(var face in faces) {
                if(visited.Contains(face)) continue;
                var currentGroup = new List<IFace2>();
                var queue = new Queue<IFace2>();

                queue.Enqueue(face);
                visited.Add(face);

                while(queue.Count > 0) {
                    var current = queue.Dequeue();
                    currentGroup.Add(current);

                    if(adjacency.TryGetValue(current, out var neighbors)) {
                        foreach(var neighbor in neighbors) {
                            if(!visited.Contains(neighbor)) {
                                visited.Add(neighbor);
                                queue.Enqueue(neighbor);
                            }
                        }
                    }
                }
                groups.Add(currentGroup);
            }

            return groups;
        }

        private class FaceEqualityComparer : IEqualityComparer<IFace2> {
            public bool Equals(IFace2 x, IFace2 y) => x.IIsSame((Face2)y);
            public int GetHashCode(IFace2 obj) => obj.GetFaceId();
        }
    }
}
