//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using SolidWorks.Interop.sldworks;
using XCad.Structures;
using XCad.Sw.Documents;
using XCad.Sw.Exceptions;
using XCad.Sw.Extensions;
using XCad.Sw.Geometry;
using XCad.Sw.Sketch;

namespace XCad.Sw.Features {
    public interface ISwSketch2D : ISwSketchBase {
        /// <summary>
        /// Returns the plane of this sketch
        /// </summary>
        Plane Plane { get; }

        /// <summary>
        /// Entity where this sketch is based on
        /// </summary>
        ISwPlanarRegion ReferenceEntity { get; set; }

        IEnumerable<ISwSketchRegion> Regions { get; }
    }

    internal class SwSketch2DEditor : SwSketchEditorBase<SwSketch2D> {
        public SwSketch2DEditor(SwSketch2D sketch, ISketch swSketch) : base(sketch, swSketch) {
        }

        protected override void StartEdit() => Target.OwnerDocument.Model.SketchManager.InsertSketch(true);
        protected override void EndEdit(bool cancel) => Target.OwnerDocument.Model.SketchManager.InsertSketch(!cancel);
    }

    internal class SwSketch2D : SwSketchBase, ISwSketch2D {
        internal const string TypeName = "ProfileFeature";

        IEnumerable<ISwSketchRegion> ISwSketch2D.Regions => Regions;

        internal SwSketch2D(IFeature feat, SwDocument doc, SwApplication app, bool created)
            : base(feat, doc, app, created) {
        }

        internal SwSketch2D(ISketch sketch, SwDocument doc, SwApplication app, bool created)
            : base(sketch, doc, app, created) {
        }

        public IEnumerable<ISwSketchRegion> Regions {
            get {
                var regs = Sketch.GetSketchRegions().ToSwArray<ISketchRegion>();

                if(regs != null) {
                    foreach(var reg in regs) {
                        yield return OwnerDocument.CreateObjectFromDispatch<ISwSketchRegion>(reg);
                    }
                }
            }
        }

        public Plane Plane {
            get {
                var transform = Sketch.ModelToSketchTransform.IInverse().ToXa();

                var x = new Vec3d(1, 0, 0).MulVector(transform);
                var z = new Vec3d(0, 0, 1).MulVector(transform);
                var origin = new Vec3d(0, 0, 0).MulPoint(transform);

                return new Plane(origin, z, x);
            }
        }

        public ISwPlanarRegion ReferenceEntity {
            get {
                if(IsCommitted) {
                    int entType = -1;
                    return (ISwPlanarRegion)OwnerDocument.CreateObjectFromDispatch<ISwEntity>(Sketch.GetReferenceEntity(ref entType));
                } else {
                    return m_Creator.CachedProperties.Get<ISwPlanarRegion>();
                }
            }
            set {
                if(!IsCommitted) {
                    m_Creator.CachedProperties.Set(value);
                } else {
                    throw new CommitedElementReadOnlyParameterException();
                }
            }
        }

        protected override ISketch CreateSketch() {
            var ent = (ISwEntity)ReferenceEntity;

            if(ent == null) {
                throw new Exception("Reference entity is not specified");
            }

            ent.Select(false);

            OwnerModelDoc.InsertSketch2(true);

            return OwnerModelDoc.SketchManager.ActiveSketch;
        }

        protected internal override IEditor<IXSketchBase> CreateSketchEditor(ISketch sketch) => new SwSketch2DEditor(this, sketch);
    }
}