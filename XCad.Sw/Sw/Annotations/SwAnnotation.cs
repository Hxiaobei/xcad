//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Threading;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using XCad.kit.Services;
using XCad.kit.Utils;
using XCad.Structures;
using XCad.Sw.Documents;
using XCad.Sw.Enums;
using XCad.Sw.Utils;

namespace XCad.Sw.Annotations {
    /// <summary>
    /// 注解基础接口（合并自 ISwAnnotation）
    /// </summary>
    public interface ISwAnnotation : IHasColor, ISwSelObject {
        IAnnotation Annotation { get; }
        /// <summary>
        /// Position of this annotation
        /// </summary>
        Vec3d Position { get; set; }

        /// <summary>
        /// 
        /// Font of this annotation
        /// </summary>
        IFont Font { get; set; }
    }

    internal class SwAnnotation : SwSelObject, ISwAnnotation {
        public virtual IAnnotation Annotation => m_Creator.Element;

        public override bool IsCommitted => m_Creator.IsCreated;

        protected readonly ElementCreator<IAnnotation> m_Creator;

        internal SwAnnotation(IAnnotation ann, SwDocument doc, SwApplication app) : base(ann, doc, app) {
            m_Creator = new ElementCreator<IAnnotation>(CreateAnnotation, ann, ann != null);
        }

        public Vec3d Position {
            get {
                if(IsCommitted) {
                    return new Vec3d((double[])Annotation.GetPosition());
                } else {
                    return m_Creator.CachedProperties.Get<Vec3d>();
                }
            }
            set {
                if(IsCommitted) {
                    SetPosition(Annotation, value);
                } else {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        public System.Drawing.Color? Color {
            get {
                if(IsCommitted) {
                    var layerOverride = (swLayerOverride_e)Annotation.LayerOverride;

                    if(layerOverride.HasFlag(swLayerOverride_e.swLayerOverrideColor)) {
                        return ColorUtils.FromColorRef(Annotation.Color);
                    } else {
                        return null;
                    }
                } else {
                    return m_Creator.CachedProperties.Get<System.Drawing.Color?>();
                }
            }
            set {
                if(IsCommitted) {
                    SetColor(Annotation, value);
                } else {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        public IFont Font {
            get {
                if(IsCommitted) {
                    return SwFontHelper.FromTextFormat((ITextFormat)Annotation.GetTextFormat(0));
                } else {
                    return m_Creator.CachedProperties.Get<IFont>();
                }
            }
            set {
                if(IsCommitted) {
                    var textFormat = (ITextFormat)Annotation.GetTextFormat(0);

                    if(value != null) {
                        SwFontHelper.FillTextFormat(value, textFormat);
                    }

                    Annotation.SetTextFormat(0, value == null, textFormat);
                } else {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        public override void Commit(CancellationToken cancellationToken)
            => m_Creator.Create(cancellationToken);

        protected virtual IAnnotation CreateAnnotation(CancellationToken arg)
            => throw new NotSupportedException("Creating of this annotation is not supported");

        protected void SetPosition(IAnnotation ann, Vec3d value) {
            if(OwnerApplication.IsVersionNewerOrEqual(SwVersion_e.Sw2014, 3)) {
                if(!ann.SetPosition2(value.X, value.Y, value.Z)) {
                    throw new Exception("Failed to set the position of the dimension");
                }
            } else {
                if(!ann.SetPosition(value.X, value.Y, value.Z)) {
                    throw new Exception("Failed to set the position of the dimension");
                }
            }
        }

        protected void SetColor(IAnnotation ann, System.Drawing.Color? value) {
            if(value.HasValue) {
                ann.Color = ColorUtils.ToColorRef(value.Value);
            } else {
                var layerOverride = (swLayerOverride_e)ann.LayerOverride;

                if(layerOverride.HasFlag(swLayerOverride_e.swLayerOverrideColor)) {
                    layerOverride -= swLayerOverride_e.swLayerOverrideColor;
                }

                ann.LayerOverride = (int)layerOverride;
            }
        }

        protected void Refresh(IAnnotation ann) {
            var origVisible = ann.Visible;

            if(origVisible != (int)swAnnotationVisibilityState_e.swAnnotationHidden) {
                ann.Visible = (int)swAnnotationVisibilityState_e.swAnnotationHidden;

                ann.Visible = origVisible;
            }
        }
    }
}
