using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using XCad.kit.Services;
using XCad.kit.Utils;
using XCad.Structures;
using XCad.Sw.Base;
using XCad.Sw.Documents;
using XCad.Sw.Enums;
using XCad.Sw.Exceptions;
using XCad.Sw.Extensions;
using XCad.Sw.Geometry.Curves;
using XCad.Sw.Geometry.Wires;
using XCad.Sw.Utils;

namespace XCad.Sw.Geometry {
    public interface ISwBody : ISwSelObject,
        ISupportsResilience<ISwBody>,
        IHasColor,
        IXTransaction,
        IDisposable {
        IBody2 Body { get; }
        ISwComponent Component { get; }
        string Name { get; }
        bool Visible { get; set; }
        IEnumerable<ISwFace> Faces { get; }
        IEnumerable<ISwEdge> Edges { get; }
        ISwBody Copy();
        void Transform(Transform transform);
        void Preview(ISwObject context, Color color);
    }

    [DebuggerDisplay("{" + nameof(Name) + "}")]
    internal class SwBody : SwSelObject, ISwBody {
        private enum DisplayBodyResult_e {
            Success = 0,
            NotTempBody = 1,
            InvalidComponent = 2,
            NotPart = 3
        }
        ISwObject ISupportsResilience.CreateResilient() => CreateResilient();

        public virtual IBody2 Body {
            get {
                if(IsResilient) {
                    try {
                        if(string.IsNullOrEmpty(m_Body.Name) && !m_Body.IsTemporaryBody())
                            throw new Exception("Permanent body is not alive");
                    } catch {
                        var body = (IBody2)OwnerDocument.Model.Extension.GetObjectByPersistReference3(m_PersistId, out _);

                        if(body != null) {
                            m_Body = body;
                        } else {
                            throw new NullReferenceException("Pointer to the body cannot be restored");
                        }
                    }
                }

                return m_Body;
            }
        }

        public override object Dispatch => Body;

        public virtual bool Visible {
            get => Body.Visible;
            set {
                if(Body.IsTemporaryBody()) {
                    if(!value) {
                        if(m_CurrentPreviewContext != null) {
                            Body.Hide(m_CurrentPreviewContext);
                            m_CurrentPreviewContext = null;
                        } else {
                            throw new NotSupportedException("Body was not previewed");
                        }
                    } else {
                        throw new NotSupportedException($"Use {nameof(Preview)} method to show hide body");
                    }
                } else {
                    Body.HideBody(!value);
                }
            }
        }

        public string Name => Body.Name;

        public ISwComponent Component {
            get {
                if((Body.IGetFirstFace() as IEntity)?.GetComponent() is IComponent2 comp) {
                    return OwnerDocument.CreateObjectFromDispatch<ISwComponent>(comp);
                }

                return IsResilient ? m_PersistComponent : null;
            }
        }

        public Color? Color {
            get => SwColorHelper.FromMaterialProperties(Body.MaterialPropertyValues2 as double[]);
            set {
                if(value.HasValue) {
                    var matPrps = SwColorHelper.ToMaterialProperties(value.Value);
                    Body.MaterialPropertyValues2 = matPrps;
                } else {
                    SwColorHelper.GetColorScope(Component?.Component,
                        out var confOpts, out var confs);

                    Body.RemoveMaterialProperty((int)confOpts, confs);
                }
            }
        }

        public IEnumerable<ISwFace> Faces {
            get {
                var face = Body.IGetFirstFace();

                while(face != null) {
                    yield return OwnerApplication.CreateObjectFromDispatch<ISwFace>(face, OwnerDocument);
                    face = face.IGetNextFace();
                }
            }
        }

        public IEnumerable<ISwEdge> Edges {
            get {
                var edges = Body.GetEdges().ToSwArray<IEdge>();

                if(edges != null) {
                    foreach(IEdge edge in edges) {
                        yield return OwnerApplication.CreateObjectFromDispatch<ISwEdge>(edge, OwnerDocument);
                    }
                }
            }
        }


        public bool IsResilient { get; private set; }

        private byte[] m_PersistId;
        private ISwComponent m_PersistComponent;

        protected IBody2 m_Body;

        private object m_CurrentPreviewContext;

        internal SwBody(IBody2 body, SwDocument doc, SwApplication app)
            : base(body, doc, app ?? doc?.OwnerApplication) {
            m_Body = body;
        }

        internal override void Select(bool append, ISelectData selData) {
            if(!Body.Select2(append, (SelectData)selData)) {
                throw new Exception("Failed to select body");
            }
        }

        public ISwBody Copy() {
            var body = OwnerApplication.IsVersionNewerOrEqual(SwVersion_e.Sw2019) ? (IBody2)Body.Copy2(true) : Body.ICopy();
            return OwnerApplication.CreateObjectFromDispatch<ISwBody>(body, OwnerDocument);
        }

        public void Transform(Transform transform) {
            if(!Body.ApplyTransform(transform.ToSw())) {
                if(!Body.IsTemporaryBody()) {
                    throw new NotSupportedException($"Only temp bodies or bodies within the context of macro feature regeneration are supported. Use {nameof(Copy)} method");
                }

                throw new Exception("Failed to apply transform to the body");
            }
        }

        public virtual ISwBody CreateResilient() {
            if(OwnerDocument == null) {
                throw new NullReferenceException("Owner document is not set");
            }

            var id = (byte[])OwnerDocument.Model.Extension.GetPersistReference3(Body);

            if(id != null) {
                var body = OwnerDocument.CreateObjectFromDispatch<SwBody>(Body);
                body.MakeResilient(id);
                return body;
            }

            throw new Exception("Failed to create resilient body");
        }

        private void MakeResilient(byte[] persistId) {
            IsResilient = true;
            m_PersistId = persistId;
            m_PersistComponent = Component;
        }

        public virtual void Preview(ISwObject context, Color color) {
            switch(context) {
                case ISwPart part:
                    Preview(part.Model, color, false);
                    break;

                case ISwComponent comp:
                    Preview(comp.Component, color, false);
                    break;

                default:
                    throw new NotSupportedException("Only ISwPart or ISwComponent is supported as the context");
            }
        }

        private void Preview(object context, Color color, bool selectable) {
            var opts = selectable
                ? swTempBodySelectOptions_e.swTempBodySelectable
                : swTempBodySelectOptions_e.swTempBodySelectOptionNone;

            var res = (DisplayBodyResult_e)Body.Display3(context, ColorUtils.ToColorRef(color), (int)opts);

            if(res != DisplayBodyResult_e.Success) {
                throw new Exception($"Failed to render preview body: {res}");
            }

            var hasAlpha = color.A < 255;

            if(hasAlpha) {
                //COLORREF does not encode alpha channel, so assigning the color via material properties
                Color = color;
            }

            m_CurrentPreviewContext = context;
        }

        public void Dispose() {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing) {
            if(disposing) {
                if(m_Body != null && m_Body.IsTemporaryBody()) {
                    Marshal.FinalReleaseComObject(m_Body);
                }
            }

            m_Body = null;
        }
    }

    public interface ISwSheetBody : ISwBody {
    }

    internal class SwSheetBody : SwBody, ISwSheetBody {
        internal SwSheetBody(IBody2 body, SwDocument doc, SwApplication app) : base(body, doc, app) {
        }
    }

    public interface ISwPlanarSheetBody : ISwSheetBody, ISwPlanarRegion {
    }

    internal class SwPlanarSheetBody : SwSheetBody, ISwPlanarSheetBody {
        ISwLoop ISwRegion.OuterLoop { get => OuterLoop; set => throw new NotSupportedException(); }
        ISwLoop[] ISwRegion.InnerLoops { get => InnerLoops; set => throw new NotSupportedException(); }

        internal SwPlanarSheetBody(IBody2 body, SwDocument doc, SwApplication app) : base(body, doc, app) {
        }

        public virtual Plane Plane => this.GetPlane();

        public virtual ISwPlanarSheetBody PlanarSheetBody => (ISwPlanarSheetBody)this.Copy();

        public ISwLoop OuterLoop { get => this.GetOuterLoop(); set => throw new NotImplementedException(); }
        public ISwLoop[] InnerLoops { get => this.GetInnerLoops(); set => throw new NotImplementedException(); }
    }

    internal static class ISwPlanarSheetBodyExtension {
        internal static Plane GetPlane(this ISwPlanarSheetBody body) {
            var planarFace = ((SwObject)body).OwnerApplication.CreateObjectFromDispatch<SwPlanarFace>(
                body.Body.IGetFirstFace(), ((SwObject)body).OwnerDocument);

            return planarFace.Definition.Plane;
        }

        internal static SwLoop GetOuterLoop(this ISwPlanarSheetBody body)
            => IterateLoops((SwFace)body.Faces.First()).First(l => l.Loop.IsOuter());

        internal static SwLoop[] GetInnerLoops(this ISwPlanarSheetBody body)
            => IterateLoops((SwFace)body.Faces.First()).Where(l => !l.Loop.IsOuter()).ToArray();

        private static IEnumerable<SwLoop> IterateLoops(SwFace face) {
            var loops = face.Face.GetLoops().ToSwArray<ILoop2>();

            foreach(var loop in loops) {
                yield return face.OwnerApplication.CreateObjectFromDispatch<SwLoop>(loop, face.OwnerDocument);
            }
        }
    }

    public interface ISwSolidBody : ISwBody {
    }

    internal class SwSolidBody : SwBody, ISwBody, ISwSolidBody {
        internal SwSolidBody(IBody2 body, SwDocument doc, SwApplication app) : base(body, doc, app) {
        }
    }


    public interface ISwWireBody : ISwBody, IXWireEntity {
        /// <summary>
        /// Content of the wire body
        /// </summary>
        IXSegment[] Segments { get; set; }
    }

    internal class SwWireBody : SwBody, ISwWireBody {
        internal SwWireBody(IBody2 body, SwDocument doc, SwApplication app) : base(body, doc, app) {
        }

        public virtual IXSegment[] Segments { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }

    internal class SwTempBody : SwBody {
        protected readonly ElementCreator<IBody2> m_Creator;

        public override bool IsCommitted => m_Creator.IsCreated;

        public override IBody2 Body => m_Creator.Element;

        //NOTE: keeping the pointer in this class only so it can be properly disposed
        internal SwTempBody(IBody2 body, SwApplication app) : base(null, null, app) {
            //see the comment in the constructor of why null is passed
            m_Creator = new ElementCreator<IBody2>(CreateBody, body, body != null);

            if(body != null && !body.IsTemporaryBody()) {
                throw new ArgumentException("Body is not temp");
            }

            m_Body = body;
        }

        public override void Commit(CancellationToken cancellationToken)
            => m_Creator.Create(cancellationToken);

        private IBody2 CreateBody(CancellationToken cancellationToken) {
            m_Body = CreateTempBody(cancellationToken);
            return m_Body;
        }

        protected virtual IBody2 CreateTempBody(CancellationToken cancellationToken)
            => throw new NotImplementedException();

        public override ISwBody CreateResilient()
            => throw new NotSupportedException("Only permanent bodies can be converter to resilient bodies");

        protected override void Dispose(bool disposing) {
            if(disposing) {
                if(m_Creator.IsCreated) {
                    Marshal.FinalReleaseComObject(m_Creator.Element);
                } else if(m_Body != null) {
                    Marshal.FinalReleaseComObject(m_Body);
                }
            }

            m_Body = null;
        }
    }

    internal class SwTempSolidBody : SwTempBody, ISwSolidBody {
        internal SwTempSolidBody(IBody2 body, SwApplication app) : base(body, app) {
        }
    }

    internal class SwTempSheetBody : SwTempBody, ISwSheetBody {
        internal SwTempSheetBody(IBody2 body, SwApplication app) : base(body, app) {
        }
    }

    internal class SwTempPlanarSheetBody : SwTempBody, ISwPlanarSheetBody {
        ISwLoop ISwRegion.OuterLoop { get => OuterLoop; set => throw new NotSupportedException(); }
        ISwLoop[] ISwRegion.InnerLoops { get => InnerLoops; set => throw new NotSupportedException(); }

        internal SwTempPlanarSheetBody(IBody2 body, SwApplication app) : base(body, app) {
        }

        public Plane Plane => this.GetPlane();

        public ISwPlanarSheetBody PlanarSheetBody => this;

        public ISwLoop OuterLoop { get => this.GetOuterLoop(); set => throw new NotSupportedException(); }
        public ISwLoop[] InnerLoops { get => this.GetInnerLoops(); set => throw new NotSupportedException(); }
    }

    internal class SwTempWireBody : SwTempBody, ISwWireBody {
        public IXSegment[] Segments {
            get {
                if(IsCommitted) {
                    return Edges.ToSwArray<IXSegment>();
                } else {
                    return m_Creator.CachedProperties.Get<IXSegment[]>();
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

        internal SwTempWireBody(IBody2 body, SwApplication app) : base(body, app) {
            if(body != null) {
                if(body.GetType() != (int)swBodyType_e.swWireBody) {
                    throw new Exception("Specified body is not a wire body");
                }
            }
        }

        protected override IBody2 CreateTempBody(CancellationToken cancellationToken) {
            var curves = Segments.SelectMany(s => {
                switch(s) {
                    case ISwCurve curve:
                        return curve.Curves;

                    case ISwEdge edge:
                        return edge.Definition.Curves;

                    default:
                        throw new NotSupportedException("Only edges and curves are supported for the segments");
                }
            }).ToArray();

            if(!curves.Any()) {
                throw new Exception("No curves found");
            }

            IBody2 wireBody;

            if(curves.Length == 1) {
                wireBody = curves.First().CreateWireBody();
            } else {
                wireBody = SwUtils.Modeler.CreateWireBody(curves, (int)swCreateWireBodyOptions_e.swCreateWireBodyByDefault);
            }

            if(wireBody == null) {
                throw new NullReferenceException($"Wire body cannot be created from the curves");
            }

            return wireBody;
        }
    }
}