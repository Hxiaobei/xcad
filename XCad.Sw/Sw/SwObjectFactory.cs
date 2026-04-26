using System;
using System.IO;
using System.Linq;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using XCad.kit.Reflection;
using XCad.Sw.Annotations;
using XCad.Sw.Documents;
using XCad.Sw.Features;
using XCad.Sw.Features.CustomFeature;
using XCad.Sw.Geometry;
using XCad.Sw.Geometry.Curves;
using XCad.Sw.Geometry.Surfaces;
using XCad.Sw.Sketch;

namespace XCad.Sw {
    /// <summary>
    /// Factory for xCAD objects
    /// </summary>
    internal static class SwObjectFactory {
        internal static TObj FromDispatch<TObj>(object disp, SwDocument doc, SwApplication app)
            where TObj : ISwObject {

            ISwObject CreateDefault(object d)
                => typeof(ISwSelObject).IsAssignableFrom(typeof(TObj)) 
                ? new SwSelObject(d, doc, app) 
                : new SwObject(d, doc, app);

            return (TObj)FromDispatch(disp, doc, app, CreateDefault);
        }

        private static ISwObject FromDispatch(object disp, SwDocument doc, SwApplication app, Func<object, ISwObject> defaultHandler) {
            if(disp == null) throw new ArgumentException("Dispatch is null", nameof(disp));

            // 本地函数：创建临时实体（无文档引用）
            ISwObject CreateTempBody(IBody2 body, swBodyType_e bodyType) {
                switch(bodyType) {
                    case swBodyType_e.swSheetBody:
                        if(body.GetFaceCount() == 1 && body.IGetFirstFace().IGetSurface().IsPlane())
                            return new SwTempPlanarSheetBody(body, app);
                        else
                            return new SwTempSheetBody(body, app);
                    case swBodyType_e.swSolidBody:
                        return new SwTempSolidBody(body, app);
                    case swBodyType_e.swWireBody:
                        return new SwTempWireBody(body, app);
                    default:
                        throw new NotSupportedException();
                }
            }

            // 本地函数：创建持久实体（带文档引用）
            ISwObject CreatePersistentBody(IBody2 body, swBodyType_e bodyType) {
                switch(bodyType) {
                    case swBodyType_e.swSheetBody:
                        if(body.GetFaceCount() == 1 && body.IGetFirstFace().IGetSurface().IsPlane())
                            return new SwPlanarSheetBody(body, doc, app);
                        else
                            return new SwSheetBody(body, doc, app);
                    case swBodyType_e.swSolidBody:
                        return new SwSolidBody(body, doc, app);
                    case swBodyType_e.swWireBody:
                        return new SwWireBody(body, doc, app);
                    default:
                        throw new NotSupportedException();
                }
            }

            switch(disp) {
                case IEdge edge:
                    var edgeCurve = edge.IGetCurve();
                    if(edgeCurve.IsCircle())
                        return new SwCircularEdge(edge, doc, app);
                    if(edgeCurve.IsLine())
                        return new SwLinearEdge(edge, doc, app);
                    return new SwEdge(edge, doc, app);

                case IFace2 face:
                    var faceSurf = face.IGetSurface();
                    switch((swSurfaceTypes_e)faceSurf.Identity()) {
                        case swSurfaceTypes_e.PLANE_TYPE:
                            return new SwPlanarFace(face, doc, app);
                        case swSurfaceTypes_e.CYLINDER_TYPE:
                            return new SwCylindricalFace(face, doc, app);
                        default:
                            throw new NotSupportedException($"Not supported face type: {(swSurfaceTypes_e)faceSurf.Identity()}");
                    }

                case IVertex vertex:
                    return new SwVertex(vertex, doc, app);

                case ISilhouetteEdge silhouetteEdge:
                    return new SwSilhouetteEdge(silhouetteEdge, doc, app);

                case ISketch sketch:
                    if(sketch.Is3D())
                        return new SwSketch3D(sketch, doc, app, true);
                    else
                        return new SwSketch2D(sketch, doc, app, true);

                case IBody2 body:
                    var bodyType = (swBodyType_e)body.GetType();
                    bool isTemp = body.IsTemporaryBody();
                    if(isTemp)
                        return CreateTempBody(body, bodyType);
                    else
                        return CreatePersistentBody(body, bodyType);

                case ISketchSegment seg:
                    switch((swSketchSegments_e)seg.GetType()) {
                        case swSketchSegments_e.swSketchARC:
                            var arc = (ISketchArc)seg;
                            const int CIRCLE = 1;
                            if(arc.IsCircle() == CIRCLE)
                                return new SwSketchCircle(arc, doc, app, true);
                            else
                                return new SwSketchArc(arc, doc, app, true);
                        case swSketchSegments_e.swSketchELLIPSE:
                            return new SwSketchEllipse((ISketchEllipse)seg, doc, app, true);
                        case swSketchSegments_e.swSketchLINE:
                            return new SwSketchLine((ISketchLine)seg, doc, app, true);
                        case swSketchSegments_e.swSketchPARABOLA:
                            return new SwSketchParabola((ISketchParabola)seg, doc, app, true);
                        case swSketchSegments_e.swSketchSPLINE:
                            return new SwSketchSpline((ISketchSpline)seg, doc, app, true);
                        case swSketchSegments_e.swSketchTEXT:
                            return new SwSketchText((ISketchText)seg, doc, app, true);
                        default:
                            throw new NotSupportedException();
                    }

                case ISketchRegion skReg:
                    return new SwSketchRegion(skReg, doc, app);

                case ISketchPoint skPt:
                    return new SwSketchPoint(skPt, doc, app, true);

                case ISketchPicture skPict:
                    return new SwSketchPicture(skPict, doc, app, true);

                case IDisplayDimension dispDim:
                    return new SwDimension(dispDim, doc, app);

                // 以下类型暂未实现，保留注释
                //case INote note:
                //    return new SwNote(note, doc, app);
                //case IDrSection section:
                //    return new SwSectionLine(section, doc, app);
                //case IDetailCircle detailCircle:
                //    return new SwDetailCircle(detailCircle, doc, app);
                //case ITableAnnotation tableAnn:
                //    return new SwTable(tableAnn, doc, app);

                case IAnnotation ann:
                    switch((swAnnotationType_e)ann.GetType()) {
                        case swAnnotationType_e.swDisplayDimension:
                            return new SwDimension((IDisplayDimension)ann.GetSpecificAnnotation(), doc, app);
                        case swAnnotationType_e.swNote:
                        //    return new SwNote((INote)ann.GetSpecificAnnotation(), doc, app);
                        //case swAnnotationType_e.swTableAnnotation:
                        //    return new SwTable((ITableAnnotation)ann.GetSpecificAnnotation(), doc, app);
                        default:
                            return new SwAnnotation(ann, doc, app);
                    }

                case IConfiguration conf:
                    if(doc is SwAssembly assm)
                        return new SwAssemblyConfiguration(conf, assm, app, true);
                    else if(doc is SwPart part)
                        return new SwPartConfiguration(conf, part, app, true);
                    else
                        throw new Exception("Owner document must be 3D document or assembly");

                case IComponent2 comp:
                    var compRefModel = comp.GetModelDoc2();
                    if(compRefModel != null) {
                        if(compRefModel is IPartDoc)
                            return new SwPartComponent(comp, (SwAssembly)doc, app);
                        else if(compRefModel is IAssemblyDoc)
                            return new SwAssemblyComponent(comp, (SwAssembly)doc, app);
                        else
                            throw new NotSupportedException($"Unrecognized component type of '{comp.Name2}'");
                    } else {
                        var compFilePath = comp.GetPathName();
                        var ext = Path.GetExtension(compFilePath)?.ToLower();
                        if(ext == ".sldprt")
                            return new SwPartComponent(comp, (SwAssembly)doc, app);
                        else if(ext == ".sldasm")
                            return new SwAssemblyComponent(comp, (SwAssembly)doc, app);
                        else
                            throw new NotSupportedException($"Component '{comp.Name2}' file '{compFilePath}' is not recognized");
                    }

                //case ISheet sheet:
                //    return new SwSheet(sheet, (SwDrawing)doc, app);

                case ICurve curve:
                    switch((swCurveTypes_e)curve.Identity()) {
                        case swCurveTypes_e.LINE_TYPE:
                            return new SwLineCurve(curve, doc, app, true);
                        case swCurveTypes_e.CIRCLE_TYPE:
                            curve.GetEndParams(out _, out _, out bool isClosed, out _);
                            if(isClosed)
                                return new SwCircleCurve(curve, doc, app, true);
                            else
                                return new SwArcCurve(curve, doc, app, true);
                        default:
                            return new SwCurve(curve, doc, app, true);
                    }

                case ILoop2 loop:
                    return new SwLoop(loop, doc, app);

                case ISurface surf:
                    switch((swSurfaceTypes_e)surf.Identity()) {
                        case swSurfaceTypes_e.PLANE_TYPE:
                            return new SwPlanarSurface(surf, doc, app);
                        case swSurfaceTypes_e.CYLINDER_TYPE:
                            return new SwCylindricalSurface(surf, doc, app);
                        // 其他曲面类型暂未实现
                        default:
                            throw new NotSupportedException($"Not supported surface type: {(swSurfaceTypes_e)surf.Identity()}");
                    }

                case IModelView modelView:
                    return new SwModelView(modelView, doc, app);

                case ISketchBlockInstance skBlockInst:
                    return new SwSketchBlockInstance((IFeature)skBlockInst, doc, app, true);

                case ISketchBlockDefinition skBlockDef:
                    return new SwSketchBlockDefinition((IFeature)skBlockDef, doc, app, true);

                case IFeature feat:
                    switch(feat.GetTypeName()) {
                        case SwSketch2D.TypeName:
                            return new SwSketch2D(feat, doc, app, true);
                        case SwSketch3D.TypeName:
                            return new SwSketch3D(feat, doc, app, true);
                        case "CoordSys":
                            return new SwCoordinateSystem(feat, doc, app, true);
                        case SwOrigin.TypeName:
                            return new SwOrigin(feat, doc, app, true);
                        case SwPlane.TypeName:
                            return new SwPlane(feat, doc, app, true);
                        case "SketchBlockInst":
                            return new SwSketchBlockInstance(feat, doc, app, true);
                        case "SketchBlockDef":
                            return new SwSketchBlockDefinition(feat, doc, app, true);
                        case "SketchBitmap":
                            return new SwSketchPicture(feat, doc, app, true);
                        case "BaseBody":
                            return new SwDumbBody(feat, doc, app, true);
                        case "MacroFeature":
                            if(TryGetParameterType(feat, out Type paramType))
                                return SwMacroFeature<object>.CreateSpecificInstance(feat, doc, app, paramType);
                            else
                                return new SwMacroFeature(feat, doc, app, true);
                        default:
                            return new SwFeature(feat, doc, app, true);
                    }

                default:
                    return defaultHandler(disp);
            }
        }

        private static bool TryGetParameterType(IFeature feat, out Type paramType) {
            var featData = feat.GetDefinition() as IMacroFeatureData;
            var progId = featData?.GetProgId();

            if(!string.IsNullOrEmpty(progId)) {
                var type = Type.GetTypeFromProgID(progId);
                if(type != null && type.IsAssignableToGenericType(typeof(SwMacroFeatureDefinition<>))) {
                    paramType = type.GetArgumentsOfGenericType(typeof(SwMacroFeatureDefinition<>)).First();
                    return true;
                }
            }

            paramType = default;
            return false;
        }
    }
}