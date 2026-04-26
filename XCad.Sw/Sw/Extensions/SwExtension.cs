using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using XCad.Structures;

namespace XCad.Sw.Extensions {
    public static class SwExtension {

        public static bool GetOpenPt(this ISketchSegment seg, out Vec3d stPt, out Vec3d edPt) {
            stPt = default; edPt = default;
            SketchPoint spt, ept;

            var skSegType = (swSketchSegments_e)seg.GetType();

            switch(skSegType) {
                case swSketchSegments_e.swSketchLINE:
                    var line = seg as ISketchLine;
                    spt = line.IGetStartPoint2();
                    ept = line.IGetEndPoint2();
                    break;
                case swSketchSegments_e.swSketchARC:
                    var arc = seg as ISketchArc;
                    if(arc.IsCircle() == 1) return false;
                    spt = arc.IGetStartPoint2();
                    ept = arc.IGetEndPoint2();
                    break;
                case swSketchSegments_e.swSketchELLIPSE:
                    var eli = seg as ISketchEllipse;
                    spt = eli.IGetStartPoint2();
                    ept = eli.IGetEndPoint2();
                    break;
                case swSketchSegments_e.swSketchSPLINE:
                    return false;
                case swSketchSegments_e.swSketchPARABOLA:
                    var para = seg as SketchParabola;
                    spt = para.IGetStartPoint2();
                    ept = para.IGetEndPoint2();
                    break;
                default:
                    return false;
            }

            stPt = spt.ToXa();
            edPt = ept.ToXa();

            return true;
        }

        public static bool IsSheetMetal(this IPartDoc part) {
            var bodies = part.GetBodies2(0, false).ToSwArray<Body2>();
            foreach(var body in bodies)
                if(body.IsSheetMetal()) return true;
            return false;
        }
        #region Ext

        /// <summary>
        /// 启用或禁用高效模式
        /// </summary>
        public static void SetPerformanceMode(this ISldWorks swApp, in bool suppress, IModelDoc2 model = null) {

            var enable = !suppress;
            if(model != null && model.GetTitle() == swApp.IActiveDoc2.GetTitle()) {
                // 图形界面刷新
                model.IActiveView.EnableGraphicsUpdate = enable;
                //
                model.FeatureManager.EnableFeatureTree = enable;
                //
                model.FeatureManager.EnableFeatureTreeWindow = enable;
            }
            swApp.DocumentVisible(enable, (int)swDocumentTypes_e.swDocPART);
            swApp.DocumentVisible(enable, (int)swDocumentTypes_e.swDocASSEMBLY);
            swApp.DocumentVisible(enable, (int)swDocumentTypes_e.swDocDRAWING);
            //后台处理
            swApp.EnableBackgroundProcessing = suppress;
            swApp.UserControl = enable;

            // 可选：隐藏 SolidWorks 主窗口
            //swApp.Visible = enable;
        }

        public static IDisplayDimension[] GetAllDispDims(this IModelDoc2 model) {
            var list = new List<IDisplayDimension>();
            foreach(var feat in model.GetFeatures()) {
                feat.GetDispDims(list);
            }
            return list.ToArray();
        }

        public static void GetDispDims(this IFeature feat, List<IDisplayDimension> list) {
            list.AddRange(feat.GetDisplayDimensions());
            foreach(var subFeat in feat.GetSubFeats())
                subFeat.GetDispDims(list);
        }

        #endregion

        #region Box 
        public static void GetBoxPt(this IPartDoc part, out Vec3d min, out Vec3d max) {

            if(part == null) {
                min = Vec3d.Zero;
                max = Vec3d.Zero;
                return;
            }
            var box = (double[])part.GetPartBox(true);
            min = new Vec3d(box[0], box[1], box[2]);
            max = new Vec3d(box[3], box[4], box[5]);
        }
        public static void GetBoxPt(this IAssemblyDoc asm, out Vec3d min, out Vec3d max) {

            if(asm == null) {
                min = Vec3d.Zero;
                max = Vec3d.Zero;
                return;
            }

            var box = (double[])asm.GetBox(0);
            min = new Vec3d(box[0], box[1], box[2]);
            max = new Vec3d(box[3], box[4], box[5]);
        }
        public static void GetBoxPt(this IFeature feat, out Vec3d min, out Vec3d max) {

            var box = new double[6];

            if(feat?.GetBox(box) != true) {
                min = Vec3d.Zero;
                max = Vec3d.Zero;
                return;
            }

            min = new Vec3d(box[0], box[1], box[2]);
            max = new Vec3d(box[3], box[4], box[5]);
        }
        public static void GetBoxPt(this IComponent2 comp, out Vec3d min, out Vec3d max) {
            min = Vec3d.Zero;
            max = Vec3d.Zero;
            var box = comp?.GetBox(false, false) as double[];
            if(box == null) return;

            min = new Vec3d(box[0], box[1], box[2]);
            max = new Vec3d(box[3], box[4], box[5]);
        }
        public static void GetBoxPt(this IBody2 body, out Vec3d min, out Vec3d max) {

            if(body == null) {
                min = Vec3d.Zero;
                max = Vec3d.Zero;
                return;
            }

            var box = (double[])body.GetBodyBox();
            min = new Vec3d(box[0], box[1], box[2]);
            max = new Vec3d(box[3], box[4], box[5]);
        }
        public static void GetBoxPt(this IFace2 face, out Vec3d min, out Vec3d max) {

            if(face == null) {
                min = Vec3d.Zero;
                max = Vec3d.Zero;
                return;
            }

            var box = (double[])face.GetBox();
            min = new Vec3d(box[0], box[1], box[2]);
            max = new Vec3d(box[3], box[4], box[5]);
        }

        #endregion

        #region Edge

        /// <summary>直线边长度（弦长）</summary>
        public static double GetChordLength(this IEdge edge) {

            var start = edge.IGetStartVertex();
            var end = edge.IGetEndVertex();

            if(start == null || end == null) return -1;

            return start.ToXa().Distance(end.ToXa());
        }

        /// <summary>曲线实际长度（适用于直线、圆弧等）</summary>
        public static double GetLength(this IEdge edge) {
            var param = edge.GetCurveParams3();
            return edge.IGetCurve().GetLength3(param.UMinValue, param.UMaxValue);
        }

        #endregion

        #region CoEdge
        public static Vec3d GetFaceNormalAtMidCoEdge(this ICoEdge coEdge) {

            var cps = (double[])coEdge.GetCurveParams();

            var pt = (double[])coEdge.Evaluate2((cps[7] + cps[6]) / 2, 0);

            var surface = coEdge.IGetLoop2().IGetFace().IGetSurface();

            var surParam = (double[])surface.EvaluateAtPoint(pt[0], pt[1], pt[2]);

            return new Vec3d(surParam[0], surParam[1], surParam[2]);
        }

        #endregion

        #region Face
        public static Vec3d GetNormal(this IFace2 face) {
            var n = new Vec3d((double[])face.Normal);
            return face.FaceInSurfaceSense() ? -n : n;
        }

        public static bool GetNormal(this IFace2 face, out Vec3d normal, out Vec3d rootPoint) {
            if(face.IGetSurface().IsPlane()) {
                var array = face.IGetSurface().PlaneParams as double[];
                if(array?.Any() == true) {
                    normal = new Vec3d(array[0], array[1], array[2]);
                    rootPoint = new Vec3d(array[3], array[4], array[5]);
                    return true;
                }
            }
            normal = Vec3d.Zero;
            rootPoint = Vec3d.Zero;
            return false;
        }

        public static Face2[] GetTangentFaces(this IFace2 face) {

            var faces = new List<Face2>();

            foreach(var loop in face.GetLoops2()) {

                foreach(var coEdge in loop.GetCoEdges2()) {

                    var thisNormal = coEdge.GetFaceNormalAtMidCoEdge();
                    var otherCoEdge = coEdge.IGetPartner();
                    var otherNormal = otherCoEdge.GetFaceNormalAtMidCoEdge();

                    if(thisNormal.IsParallel(otherNormal)) faces.Add(otherCoEdge.IGetLoop2().IGetFace());
                }
            }

            return faces.ToArray();
        }

        #endregion

        #region Curve
        public static bool GetCircleParams(this ICurve curve, out Vec3d center, out Vec3d axis, out double radius) {
            if(curve.IsCircle()) {
                var array = curve.CircleParams as double[];
                center = new Vec3d(array[0], array[1], array[2]);
                axis = new Vec3d(array[3], array[4], array[5]);
                radius = array[6];
                return true;
            }
            center = Vec3d.Zero; axis = Vec3d.Zero; radius = 0;
            return false;
        }

        public static bool GetLineParams(this ICurve curve, out Vec3d rootPoint, out Vec3d direction) {
            if(curve.IsLine()) {
                var array = curve.LineParams as double[];
                rootPoint = new Vec3d(array[0], array[1], array[2]);
                direction = new Vec3d(array[3], array[4], array[5]);
                return true;
            }
            rootPoint = Vec3d.Zero; direction = Vec3d.Zero;
            return false;
        }

        #endregion

        #region Surface
        public static bool GetConeParams(this ISurface surface, out Vec3d origin, out Vec3d axis, out double radius, out double halfAngle, out Vec3d refDirection) {
            origin = Vec3d.Zero;
            axis = Vec3d.Zero;
            radius = 0.0;
            halfAngle = 0.0;
            refDirection = Vec3d.Zero;

            if(!surface.IsCone())
                return false;

            double[] array = surface.ConeParams2 as double[];
            if(array == null || array.Length < 11)
                return false;

            origin = new Vec3d(array[0], array[1], array[2]);
            axis = new Vec3d(array[3], array[4], array[5]);
            radius = array[6];
            halfAngle = array[7];
            refDirection = new Vec3d(array[8], array[9], array[10]);
            return true;
        }

        public static bool GetCylinderParams(this ISurface surface, out Vec3d origin, out Vec3d axis, out double radius) {
            origin = Vec3d.Zero;
            axis = Vec3d.Zero;
            radius = 0.0;

            if(!surface.IsCylinder())
                return false;

            double[] array = surface.CylinderParams as double[];
            if(array == null || array.Length < 7)
                return false;

            origin = new Vec3d(array[0], array[1], array[2]);
            axis = new Vec3d(array[3], array[4], array[5]);
            radius = array[6];
            return true;
        }

        public static bool GetPlaneParams(this ISurface surface, out Vec3d normal, out Vec3d rootPoint) {
            normal = Vec3d.Zero;
            rootPoint = Vec3d.Zero;

            if(!surface.IsPlane())
                return false;

            double[] array = surface.PlaneParams as double[];
            if(array == null || array.Length < 6)
                return false;

            normal = new Vec3d(array[0], array[1], array[2]);
            rootPoint = new Vec3d(array[3], array[4], array[5]);
            return true;
        }

        public static bool GetSphereParams(this ISurface surface, out Vec3d center, out double radius) {
            center = Vec3d.Zero;
            radius = 0.0;

            if(!surface.IsSphere())
                return false;

            double[] array = surface.SphereParams as double[];
            if(array == null || array.Length < 4)
                return false;

            center = new Vec3d(array[0], array[1], array[2]);
            radius = array[3];
            return true;
        }

        public static bool GetTorusParams(this ISurface surface, out Vec3d center, out Vec3d axis, out double majorRadius, out double minorRadius) {
            center = Vec3d.Zero;
            axis = Vec3d.Zero;
            majorRadius = 0.0;
            minorRadius = 0.0;

            if(!surface.IsTorus())
                return false;

            double[] array = surface.TorusParams as double[];
            if(array == null || array.Length < 8)
                return false;

            center = new Vec3d(array[0], array[1], array[2]);
            axis = new Vec3d(array[3], array[4], array[5]);
            majorRadius = array[6];
            minorRadius = array[7];
            return true;
        }

        #endregion

        public static DispatchWrapper[] ToWrapper(this IEnumerable<object> objects)
            => objects.Select(o => new DispatchWrapper(o)).ToArray();

        public static bool GetExtreme(this Body2 swBody, Vec3d vector, out Vec3d point)
            => swBody.GetExtremePoint(vector.X, vector.Y, vector.Z, out point.X, out point.Y, out point.Z);
    }
}