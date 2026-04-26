
using System;
using System.Drawing;
using System.Threading;
using System.Windows.Media.Imaging;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using XCad.kit.Graphics;
using XCad.Structures;
using XCad.Sw.Base;
using XCad.Sw.Documents.Delegates;
using XCad.Sw.Documents.Enums;
using XCad.Sw.Utils;

namespace XCad.Sw.Documents {
    public interface ISwModelView : IXTransaction, ISwObject {

        IModelView View { get; }

        /// <summary>
        /// Display mode of the view
        /// </summary>
        ViewDisplayMode_e DisplayMode { get; set; }

        /// <summary>
        /// Fired when custom graphics can be drawn in the model
        /// </summary>
        event RenderCustomGraphicsDelegate RenderCustomGraphics;

        /// <summary>
        /// Freezes all view updates
        /// </summary>
        /// <param name="freeze">True to suppress all updates</param>
        /// <returns>Freeze object, when disposed - view is restored</returns>
        IDisposable Freeze(bool freeze);

        /// <summary>
        /// Transformation of this view related to the model origin
        /// </summary>
        Transform Transform { get; set; }

        /// <summary>
        /// Transformation of this view related to the screen coordinates
        /// </summary>
        Transform ScreenTransform { get; }

        /// <summary>
        /// View boundaries
        /// </summary>
        Rectangle ScreenRect { get; }

        /// <summary>
        /// Refreshes the view
        /// </summary>
        void Update();
    }

    internal class ModelViewFreezer : IDisposable {
        private readonly bool m_OrigIsGraphicsEnabled;
        private readonly IModelView m_View;

        internal ModelViewFreezer(SwModelView view, bool freeze) {
            m_View = view.View;
            m_OrigIsGraphicsEnabled = m_View.EnableGraphicsUpdate;

            m_View.EnableGraphicsUpdate = !freeze;
        }

        public void Dispose() {
            m_View.EnableGraphicsUpdate = m_OrigIsGraphicsEnabled;
            m_View.GraphicsRedraw(null);
        }
    }

    internal class SwModelView : SwObject, ISwModelView {

        public event RenderCustomGraphicsDelegate RenderCustomGraphics {
            add {
                if(m_RenderCustomGraphicsDelegate == null) {
                    m_GraphicsContext = new OglGraphicsContext();
                    ((ModelView)View).BufferSwapNotify += OnBufferSwapNotify;
                }

                m_RenderCustomGraphicsDelegate += value;
            }
            remove {
                m_RenderCustomGraphicsDelegate -= value;

                if(m_RenderCustomGraphicsDelegate == null) {
                    m_GraphicsContext?.Dispose();
                    m_GraphicsContext = null;
                    ((ModelView)View).BufferSwapNotify -= OnBufferSwapNotify;
                }
            }
        }

        private OglGraphicsContext m_GraphicsContext;
        private RenderCustomGraphicsDelegate m_RenderCustomGraphicsDelegate;

        internal IModelDoc2 Owner { get; }

        public virtual Rectangle ScreenRect {
            get {
                var box = View.GetVisibleBox() as int[];

                return new Rectangle(box[0], box[1], box[2] - box[0], box[3] - box[1]);
            }
        }

        public virtual Transform ScreenTransform => View.Transform.ToXa();

        public virtual Transform Transform {
            get {
                var origOr = View.Orientation3.ToXa();
                var origScale = View.Scale2;
                var origTrans = View.Translation3.ToXa();

                var rotation = origOr.Rotation;
                rotation.ScaleInPlace(origScale);

                return new Transform(rotation, origTrans);
            }
            set {
                var rotation = value.Rotation;
                var origScale = rotation.GetScale();

                View.Scale2 = rotation.GetScale();
                View.Translation3 = value.Trans.ToSwVec();

                rotation.ScaleInPlace(1.0d / origScale);
                var ts = new Transform(rotation, Vec3d.Zero);
                View.Orientation3 = ts.ToSw();
            }
        }

        public virtual IModelView View { get; }

        //TODO: implement creation of new views
        public bool IsCommitted => true;

        public override object Dispatch => View;

        public ViewDisplayMode_e DisplayMode {
            get {
                if(IsCommitted) {
                    switch((swViewDisplayMode_e)View.DisplayMode) {
                        case swViewDisplayMode_e.swViewDisplayMode_Wireframe:
                            return ViewDisplayMode_e.Wireframe;

                        case swViewDisplayMode_e.swViewDisplayMode_HiddenLinesGrayed:
                            return ViewDisplayMode_e.HiddenLinesVisible;

                        case swViewDisplayMode_e.swViewDisplayMode_HiddenLinesRemoved:
                            return ViewDisplayMode_e.HiddenLinesRemoved;

                        case swViewDisplayMode_e.swViewDisplayMode_ShadedWithEdges:
                            return ViewDisplayMode_e.ShadedWithEdges;

                        case swViewDisplayMode_e.swViewDisplayMode_Shaded:
                            return ViewDisplayMode_e.Shaded;

                        default:
                            throw new NotSupportedException();
                    }
                } else {
                    throw new NotSupportedException();
                }
            }
            set {
                if(IsCommitted) {
                    swViewDisplayMode_e dispMode;

                    switch(value) {
                        case ViewDisplayMode_e.Wireframe:
                            dispMode = swViewDisplayMode_e.swViewDisplayMode_Wireframe;
                            break;

                        case ViewDisplayMode_e.HiddenLinesVisible:
                            dispMode = swViewDisplayMode_e.swViewDisplayMode_HiddenLinesGrayed;
                            break;

                        case ViewDisplayMode_e.HiddenLinesRemoved:
                            dispMode = swViewDisplayMode_e.swViewDisplayMode_HiddenLinesRemoved;
                            break;

                        case ViewDisplayMode_e.ShadedWithEdges:
                            dispMode = swViewDisplayMode_e.swViewDisplayMode_ShadedWithEdges;
                            break;

                        case ViewDisplayMode_e.Shaded:
                            dispMode = swViewDisplayMode_e.swViewDisplayMode_Shaded;
                            break;

                        default:
                            throw new NotSupportedException();
                    }

                    View.DisplayMode = (int)dispMode;
                } else {
                    throw new NotSupportedException();
                }
            }
        }

        internal SwModelView(IModelView view, SwDocument doc, SwApplication app) : base(view, doc, app) {
            View = view;
            Owner = doc.Model;
        }

        public IDisposable Freeze(bool freeze) => new ModelViewFreezer(this, freeze);

        public void Update()
            => View.GraphicsRedraw(null);

        /// <inheritdoc/>
        public void Commit(CancellationToken cancellationToken) {
            throw new NotImplementedException();
        }

        private int OnBufferSwapNotify() {
            if(m_RenderCustomGraphicsDelegate?.Invoke(this, m_GraphicsContext) == true) {
                return HResult.S_OK;
            } else {
                return HResult.S_FALSE;
            }
        }

    }

    public interface ISwNamedView : ISwModelView {
        /// <summary>
        /// Name of the view
        /// </summary>
        string Name { get; }
    }

    internal class SwNamedView : SwModelView, ISwNamedView {
        //TODO: implement overrides for transforms

        public string Name { get; }

        internal SwNamedView(IModelView view, SwDocument doc, SwApplication app, string name)
            : base(view, doc, app) {
            Name = name;
        }
    }

    public interface ISwStandardView : ISwNamedView {
        /// <summary>
        /// Type of this standard view
        /// </summary>
        StandardViewType_e Type { get; }
    }

    internal class SwStandardView : SwNamedView, ISwStandardView {
        //TODO: implement overrides for transforms

        public StandardViewType_e Type { get; }

        internal swStandardViews_e SwViewType { get; }

        private static string GetStandardViewName(IModelDoc2 model, StandardViewType_e swViewType) {
            var viewNames = model.GetModelViewNames() as string[];

            switch(swViewType) {
                case StandardViewType_e.Front:
                    return viewNames[1];

                case StandardViewType_e.Back:
                    return viewNames[2];

                case StandardViewType_e.Left:
                    return viewNames[3];

                case StandardViewType_e.Right:
                    return viewNames[4];

                case StandardViewType_e.Top:
                    return viewNames[5];

                case StandardViewType_e.Bottom:
                    return viewNames[6];

                case StandardViewType_e.Isometric:
                    return viewNames[7];

                case StandardViewType_e.Trimetric:
                    return viewNames[8];

                case StandardViewType_e.Dimetric:
                    return viewNames[9];

                default:
                    throw new NotImplementedException($"{swViewType} is not supported");
            }
        }

        internal SwStandardView(IModelView view, SwDocument doc, SwApplication app, StandardViewType_e type)
            : this(view, doc, app, type, GetStandardViewName(doc.Model, type)) {
        }

        internal SwStandardView(IModelView view, SwDocument doc, SwApplication app, StandardViewType_e type, string name)
            : base(view, doc, app, name) {
            Type = type;

            switch(Type) {
                case StandardViewType_e.Back:
                    SwViewType = swStandardViews_e.swBackView;
                    break;

                case StandardViewType_e.Bottom:
                    SwViewType = swStandardViews_e.swBottomView;
                    break;

                case StandardViewType_e.Dimetric:
                    SwViewType = swStandardViews_e.swDimetricView;
                    break;

                case StandardViewType_e.Front:
                    SwViewType = swStandardViews_e.swFrontView;
                    break;

                case StandardViewType_e.Isometric:
                    SwViewType = swStandardViews_e.swIsometricView;
                    break;

                case StandardViewType_e.Left:
                    SwViewType = swStandardViews_e.swLeftView;
                    break;

                case StandardViewType_e.Right:
                    SwViewType = swStandardViews_e.swRightView;
                    break;

                case StandardViewType_e.Top:
                    SwViewType = swStandardViews_e.swTopView;
                    break;

                case StandardViewType_e.Trimetric:
                    SwViewType = swStandardViews_e.swTrimetricView;
                    break;
            }
        }
    }

    /// <summary>
    /// Standard 3D views of the model
    /// </summary>
    public enum StandardViewType_e {
        /// <summary>
        /// Front view
        /// </summary>
        Front,

        /// <summary>
        /// Back view
        /// </summary>
        Back,

        /// <summary>
        /// Left view
        /// </summary>
        Left,

        /// <summary>
        /// Right view
        /// </summary>
        Right,

        /// <summary>
        /// Top view
        /// </summary>
        Top,

        /// <summary>
        /// Bottom view
        /// </summary>
        Bottom,

        /// <summary>
        /// Isometric view
        /// </summary>
        Isometric,

        /// <summary>
        /// Trimetric view
        /// </summary>
        Trimetric,

        /// <summary>
        /// Dimetric view
        /// </summary>
        Dimetric
    }
}