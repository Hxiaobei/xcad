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
using XCad.Sw.Geometry;

namespace XCad.Sw.Sketch {
    public interface ISwSketchRegion : ISwPlanarRegion, ISwSelObject {
        ISketchRegion Region { get; }
    }

    internal class SwSketchRegion : SwSelObject, ISwSketchRegion {
        ISwLoop ISwRegion.OuterLoop { get => OuterLoop; set => OuterLoop = value; }
        ISwLoop[] ISwRegion.InnerLoops { get => InnerLoops; set => InnerLoops = value.ToSwArray<ISwLoop>(); }

        internal SwSketchRegion(ISketchRegion region, SwDocument doc, SwApplication app) : base(region, doc, app) {
            Region = region;
        }

        public ISketchRegion Region { get; }

        public Plane Plane {
            get {
                var transform = Region.Sketch.ModelToSketchTransform.IInverse().ToXa();

                var x = new Vec3d(1, 0, 0).MulVector(transform);
                var z = new Vec3d(0, 0, 1).MulVector(transform);
                var origin = new Vec3d(0, 0, 0).MulPoint(transform);

                return new Plane(origin, z, x);
            }
        }

        public ISwPlanarSheetBody PlanarSheetBody {
            get {
                var face = Region.GetFirstLoop().IGetFace();
                var sheetBody = face.CreateSheetBody();
                return OwnerApplication.CreateObjectFromDispatch<ISwPlanarSheetBody>(sheetBody, OwnerDocument);
            }
        }

        public ISwLoop OuterLoop {
            get => IterateLoops().First(l => l.Loop.IsOuter());
            set => throw new NotSupportedException();
        }

        public ISwLoop[] InnerLoops {
            get => IterateLoops().Where(l => !l.Loop.IsOuter()).ToArray();
            set => throw new NotSupportedException();
        }

        private IEnumerable<ISwLoop> IterateLoops() {
            var loop = Region.GetFirstLoop();

            while(loop != null) {
                yield return OwnerApplication.CreateObjectFromDispatch<ISwLoop>(loop, OwnerDocument);

                loop = loop.IGetNext();
            }
        }
    }
}
