using System.Collections.Generic;
using System.Linq;
using SolidWorks.Interop.sldworks;

namespace XCad.Sw.Extensions {

    public static class SwIEnumerableExtension {

        public static IEnumerable<IFeature> GetSubFeats(this IFeature feat) {
            for(var sub = feat.IGetFirstSubFeature(); sub != null; sub = sub.IGetNextSubFeature())
                yield return sub;
        }
        public static IEnumerable<IFace2> GetFaces2(this IBody2 body) {
            for(var face = body.IGetFirstFace(); face != null; face = face.IGetNextFace()) {
                yield return face;
            }
        }
        public static IEnumerable<IFeature> GetFeatures(this IComponent2 component) {
            for(var feat = component.FirstFeature(); feat != null; feat = feat.IGetNextFeature())
                yield return feat;
        }
        public static IEnumerable<ILoop2> GetLoops2(this IFace2 face) {
            for(var loop = face.IGetFirstLoop(); loop != null; loop = loop.IGetNext())
                yield return loop;
        }
        public static IEnumerable<ICoEdge> GetCoEdges2(this ILoop2 loop) {

            var firstCoEdge = loop.IGetFirstCoEdge();
            var thisCoEdge = firstCoEdge;
            do {
                yield return thisCoEdge;
                thisCoEdge = thisCoEdge.IGetNext();
            }
            while(!thisCoEdge.Equals(firstCoEdge));
        }
        public static IEnumerable<IFeature> GetFeatures(this IModelDoc2 model) {
            if(model == null) yield break;
            for(var feat = model.IFirstFeature(); feat != null; feat = feat.IGetNextFeature())
                yield return feat;
        }
        public static IEnumerable<IDisplayDimension> GetDisplayDimensions(this IFeature feature) {
            if(feature == null) yield break;
            for(var dispDim = feature.GetFirstDisplayDimension() as DisplayDimension; dispDim != null; dispDim = feature.GetNextDisplayDimension(dispDim) as DisplayDimension)
                yield return dispDim;
        }
        public static IEnumerable<IComponent2> GetChildren2(this IComponent2 component, bool isAllChild = false) {
            var childComps = component.GetChildren().ToSwArray<IComponent2>();
            foreach(var comp in childComps) {
                if(isAllChild) GetChildren2(comp, isAllChild);
                yield return comp;
            }
        }
        public static IEnumerable<ISketchPoint> GetSkPts(this ISketch sketch) {
            var sketchPoints = sketch.IEnumSketchPoints();

            var count = sketch.GetSketchPointsCount2();
            for(var i = 0; i <= count; i++) {
                int fetched = 1;
                sketchPoints.Next(i, out var rgelt, ref fetched);
                yield return rgelt;
            }
        }
        public static IEnumerable<object> GetSelectedObj(this ISelectionMgr selMgr, int mark = -1)
            => Enumerable.Range(1, selMgr.GetSelectedObjectCount2(mark))
                  .Select(i => selMgr.GetSelectedObject6(i, mark));
        public static IEnumerable<IComponent2> GetSelectedComponent(this ISelectionMgr selMgr, int mark = -1)
            => Enumerable.Range(1, selMgr.GetSelectedObjectCount2(mark))
                  .Select(i => selMgr.GetSelectedObjectsComponent4(i, mark))
                  .OfType<IComponent2>();

        public static IEnumerable<ModelDoc2> GetEnums(this IEnumDocuments2 enums) {
            int n = 0;
            while(true) {
                enums.Next(1, out var @enum, ref n);
                if(n == 0) break;
                yield return @enum;
            }
        }
        public static IEnumerable<ISketchSegment> GetEnums(this IEnumSketchSegments enums) {
            int n = 0;
            while(true) {
                enums.Next(1, out var @enum, ref n);
                if(n == 0) break;
                yield return @enum;
            }
        }
        public static IEnumerable<ISketchPoint> GetEnums(this IEnumSketchPoints enums) {
            int n = 0;
            while(true) {
                enums.Next(1, out var @enum, ref n);
                if(n == 0) break;
                yield return @enum;
            }
        }
        public static IEnumerable<ISketchHatch> GetEnums(this IEnumSketchHatches enums) {
            int n = 0;
            while(true) {
                enums.Next(1, out var @enum, ref n);
                if(n == 0) break;
                yield return @enum;
            }
        }
        public static IEnumerable<IComponent2> GetEnums(this IEnumComponents2 enums) {
            int n = 0;
            while(true) {
                enums.Next(1, out var @enum, ref n);
                if(n == 0) break;
                yield return @enum;
            }
        }
        public static IEnumerable<ModelView> GetEnums(this IEnumModelViews enums) {
            int n = 0;
            while(true) {
                enums.Next(1, out var @enum, ref n);
                if(n == 0) break;
                yield return @enum;
            }
        }
        public static IEnumerable<IDisplayDimension> GetEnums(this IEnumDisplayDimensions enums) {
            int n = 0;
            while(true) {
                enums.Next(1, out var @enum, ref n);
                if(n == 0) break;
                yield return @enum;
            }
        }
        public static IEnumerable<IDrSection> GetEnums(this IEnumDrSections enums) {
            int n = 0;
            while(true) {
                enums.Next(1, out var @enum, ref n);
                if(n == 0) break;
                yield return @enum;
            }
        }
        public static IEnumerable<IBody2> GetEnums(this IEnumBodies2 enums) {
            int n = 0;
            while(true) {
                enums.Next(1, out var @enum, ref n);
                if(n == 0) break;
                yield return @enum;
            }
        }
        public static IEnumerable<IFace2> GetEnums(this IEnumFaces2 enums) {
            int n = 0;
            while(true) {
                enums.Next(1, out var @enum, ref n);
                if(n == 0) break;
                yield return @enum;
            }
        }
        public static IEnumerable<ILoop2> GetEnums(this IEnumLoops2 enums) {
            int n = 0;
            while(true) {
                enums.Next(1, out var @enum, ref n);
                if(n == 0) break;
                yield return @enum;
            }
        }
        public static IEnumerable<IEdge> GetEnums(this IEnumEdges enums) {
            int n = 0;
            while(true) {
                enums.Next(1, out var @enum, ref n);
                if(n == 0) break;
                yield return @enum;
            }
        }
        public static IEnumerable<ICoEdge> GetEnums(this IEnumCoEdges enums) {
            int n = 0;
            while(true) {
                enums.Next(1, out var @enum, ref n);
                if(n == 0) break;
                yield return @enum;
            }
        }
    }
}
