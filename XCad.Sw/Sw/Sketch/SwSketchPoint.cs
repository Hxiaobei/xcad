//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Threading;
using SolidWorks.Interop.sldworks;
using XCad.kit.Services;
using XCad.kit.Utils;
using XCad.Structures;
using XCad.Sw.Documents;
using XCad.Sw.Enums;
using XCad.Sw.Features;
using XCad.Sw.Geometry.Wires;

namespace XCad.Sw.Sketch {
    public interface ISwSketchPoint : IXPoint, ISwSketchEntity {
        ISketchPoint Point { get; }
    }

    internal class SwSketchPoint : SwSketchEntity, ISwSketchPoint {
        protected readonly IElementCreator<ISketchPoint> m_Creator;

        protected readonly ISketchManager m_SketchMgr;

        public override bool IsCommitted => m_Creator.IsCreated;

        public ISketchPoint Point => m_Creator.Element;

        public override bool IsAlive => this.CheckIsAlive(() => Point.GetID());

        public override object Dispatch => Point;

        public override IXSketchBase OwnerSketch => m_OwnerSketch;

        private SwSketchBase m_OwnerSketch;

        internal SwSketchPoint(ISketchPoint pt, SwDocument doc, SwApplication app, bool created) : base(pt, doc, app) {
            m_SketchMgr = doc.Model.SketchManager;

            m_Creator = new ElementCreator<ISketchPoint>(CreatePoint, pt, created);

            if(pt != null) {
                SetOwnerSketch(pt);
            }
        }

        internal SwSketchPoint(SwSketchBase ownerSketch, SwDocument doc, SwApplication app) : this(null, doc, app, false) {
            m_OwnerSketch = ownerSketch;
        }

        public override void Commit(CancellationToken cancellationToken) => m_Creator.Create(cancellationToken);

        public override System.Drawing.Color? Color {
            get {
                if(IsCommitted) {
                    return GetColor();
                } else {
                    return m_Creator.CachedProperties.Get<System.Drawing.Color?>();
                }
            }
            set {
                if(IsCommitted) {
                    SetColor(Point, value);
                } else {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        public Vec3d Coordinate {
            get {
                if(m_Creator.IsCreated) {
                    return new Vec3d(Point.X, Point.Y, Point.Z);
                } else {
                    return m_Creator.CachedProperties.Get<Vec3d>();
                }
            }
            set {
                if(m_Creator.IsCreated) {
                    if(m_SketchMgr.ActiveSketch != Point.GetSketch()) {
                        throw new Exception("You must set the sketch into editing mode in order to modify the cooridinate");
                    }

                    Point.SetCoords(value.X, value.Y, value.Z);
                } else {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        private void SetColor(ISketchPoint pt, System.Drawing.Color? color) {
            int colorRef = 0;

            if(color.HasValue) {
                colorRef = ColorUtils.ToColorRef(color.Value);
            }

            pt.Color = colorRef;
        }

        private System.Drawing.Color? GetColor() => ColorUtils.FromColorRef(Point.Color);

        private ISketchPoint CreatePoint(CancellationToken cancellationToken) {
            using(var editor = !m_OwnerSketch.IsEditing ? m_OwnerSketch.Edit() : null) {
                var pt = m_SketchMgr.CreatePoint(Coordinate.X, Coordinate.Y, Coordinate.Z);

                SetColor(pt, m_Creator.CachedProperties.Get<System.Drawing.Color?>(nameof(Color)));

                SetOwnerSketch(pt);

                return pt;
            }
        }

        protected override string GetFullName() {
            if(OwnerApplication.IsVersionNewerOrEqual(SwVersion_e.Sw2015)) {
                if(OwnerModelDoc.ISelectionManager.GetSelectByIdSpecification(Point, out string name, out _, out _)) {
                    return name;
                } else {
                    throw new Exception("Failed to get the selection specification of the point");
                }
            } else {
                throw new NotSupportedException("Point name extraction is supported in SOLIDWORKS 2015 or newer");
            }
        }

        private void SetOwnerSketch(ISketchPoint pt) {
            m_OwnerSketch = OwnerDocument.CreateObjectFromDispatch<SwSketchBase>(pt.GetSketch());
        }
    }
}