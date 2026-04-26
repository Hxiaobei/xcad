//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Drawing;
using System.Threading;
using SolidWorks.Interop.sldworks;
using XCad.kit.Services;
using XCad.kit.Utils;
using XCad.Structures;
using XCad.Sw.Documents;
using XCad.Sw.Features;
using XCad.Sw.Geometry.Curves;
using XCad.Sw.Geometry.Wires;

namespace XCad.Sw.Sketch {
    public interface ISwSketchSegment : ISwSelObject, ISwSketchEntity, IXSegment {

        ISwCurve Definition { get; }

        bool IsConstruction { get; }

        new ISwSketchPoint StartPoint { get; }

        new ISwSketchPoint EndPoint { get; }

        ISketchSegment Segment { get; }
    }

    internal abstract class SwSketchSegment : SwSketchEntity, ISwSketchSegment {
        IXPoint IXSegment.StartPoint => StartPoint;
        IXPoint IXSegment.EndPoint => EndPoint;

        protected readonly IElementCreator<ISketchSegment> m_Creator;

        protected readonly ISketchManager m_SketchMgr;

        public ISketchSegment Segment => m_Creator.Element;

        public override bool IsCommitted => m_Creator.IsCreated;

        public override object Dispatch => Segment;

        public override bool IsAlive => this.CheckIsAlive(() => Segment.GetID());

        private SwSketchBase m_OwnerSketch;

        protected SwSketchSegment(ISketchSegment seg, SwDocument doc, SwApplication app, bool created) : base(seg, doc, app) {
            if(doc == null) {
                throw new ArgumentNullException(nameof(doc));
            }

            m_SketchMgr = doc.Model.SketchManager;
            m_Creator = new ElementCreator<ISketchSegment>(CreateEntity, seg, created);

            if(seg != null) {
                SetOwnerSketch(seg);
            }
        }

        protected SwSketchSegment(SwSketchBase ownerSketch, SwDocument doc, SwApplication app) : this(null, doc, app, false) {
            m_OwnerSketch = ownerSketch;
        }

        public override void Commit(CancellationToken cancellationToken) => m_Creator.Create(cancellationToken);


        public override Color? Color {
            get {
                if(IsCommitted) {
                    return GetColor();
                } else {
                    return m_Creator.CachedProperties.Get<Color?>();
                }
            }
            set {
                if(IsCommitted) {
                    SetColor(Segment, value);
                } else {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        public ISwCurve Definition {
            get {
                var curve = Segment.IGetCurve();
                var startPt = StartPoint.Coordinate;
                var endPt = EndPoint.Coordinate;

                var blockTransform = GetTotalTransform(OwnerBlock);

                if(blockTransform != null) {
                    startPt = startPt.MulPoint(blockTransform);
                    endPt = endPt.MulPoint(blockTransform);
                }

                if(AssignedOwnerBlock != null) {
                    //NOTE: if block is assigned and this sketch entity is extracted form the definition block, it is required transform the curve to the sketch block instance space
                    curve.ApplyTransform(blockTransform.ToSw());
                }

                curve = curve.CreateTrimmedCurve2(startPt.X, startPt.Y, startPt.Z,
                    endPt.X, endPt.Y, endPt.Z) ?? throw new NullReferenceException("Failed to trim curve");
                var transform = Segment.GetSketch().ModelToSketchTransform.IInverse();

                curve.ApplyTransform(transform);

                return OwnerDocument.CreateObjectFromDispatch<SwCurve>(curve);

                Transform GetTotalTransform(ISwSketchBlockInstance skBlockInst) {
                    var ts = Transform.Identity;

                    while(skBlockInst != null) {
                        ts = ts.Multiply(skBlockInst.Transform);
                        skBlockInst = skBlockInst.OwnerBlock;
                    }

                    return ts;
                }
            }
        }



        public double Length => Definition.Length;

        public abstract ISwSketchPoint StartPoint { get; }
        public abstract ISwSketchPoint EndPoint { get; }

        public bool IsConstruction => Segment.ConstructionGeometry;

        public override IXSketchBase OwnerSketch => m_OwnerSketch;

        private void SetColor(ISketchSegment seg, Color? color) {
            int colorRef = 0;

            if(color.HasValue) {
                colorRef = ColorUtils.ToColorRef(color.Value);
            }

            seg.Color = colorRef;
        }

        private Color? GetColor() => ColorUtils.FromColorRef(Segment.Color);

        private ISketchSegment CreateEntity(CancellationToken cancellationToken) {
            using(var editor = !m_OwnerSketch.IsEditing ? m_OwnerSketch?.Edit() : null) {
                //NOTE: this entity can be created even if the IsCommited set to false as these are the cached entities created
                var seg = CreateSketchEntity();

                if(seg == null) {
                    throw new Exception("Failed to create sketch segment");
                }

                SetColor(seg, m_Creator.CachedProperties.Get<Color?>(nameof(Color)));

                SetOwnerSketch(seg);

                return seg;
            }
        }

        protected virtual ISketchSegment CreateSketchEntity()
            => throw new NotSupportedException();

        protected override string GetFullName() => this.Segment.GetName();

        private void SetOwnerSketch(ISketchSegment seg) {
            m_OwnerSketch = OwnerDocument.CreateObjectFromDispatch<SwSketchBase>(seg.GetSketch());
        }
    }
}
