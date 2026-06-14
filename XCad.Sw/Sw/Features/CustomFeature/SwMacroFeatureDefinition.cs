
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using SolidWorks.Interop.swpublished;
using XCad.kit;
using XCad.kit.CustomFeature;
using XCad.kit.Diagnostics;
using XCad.kit.Services;
using XCad.Structures;
using XCad.Sw;
using XCad.Sw.Annotations;
using XCad.Sw.Base;
using XCad.Sw.Base.Attributes;
using XCad.Sw.Base.Enums;
using XCad.Sw.Delegates;
using XCad.Sw.Documents;
using XCad.Sw.Enums;
using XCad.Sw.Exceptions;
using XCad.Sw.Extensions;
using XCad.Sw.Features.CustomFeature.Attributes;
using XCad.Sw.Features.CustomFeature.Delegates;
using XCad.Sw.Features.CustomFeature.Enums;
using XCad.Sw.Features.CustomFeature.Structures;
using XCad.Sw.Features.CustomFeature.Toolkit;
using XCad.Sw.Features.CustomFeature.Toolkit.Icons;
using XCad.Sw.Geometry;
using XCad.Sw.Services;
using XCad.Sw.UI.PropertyPage;
using XCad.Sw.Utils;
using XCad.UI.PropertyPage.Base;
using XCad.UI.PropertyPage.Enums;

namespace XCad.Sw.Features.CustomFeature {

    #region CustomFeature
    /// <summary>
    /// 表示自定义特征的定义，其中定义了业务逻辑
    /// </summary>
    public interface ISwMacroFeatureDefinition {
        /// <summary>
        /// 当从特征管理器树中单击“编辑特征”菜单时调用
        /// </summary>
        /// <param name="app">应用程序指针</param>
        /// <param name="model">特征所在当前模型的指针</param>
        /// <param name="feature">正在编辑的特征指针</param>
        /// <returns>编辑结果</returns>
        /// <remarks>使用此处理程序显示属性管理器页面或任何其他用户界面来编辑特征。
        /// </remarks>
        bool OnEditDefinition(ISwApplication app, ISwDocument model, ISwMacroFeature feature);

        /// <summary>
        /// 当宏特征重建时调用
        /// </summary>
        /// <param name="app">SOLIDWORKS 应用程序指针</param>
        /// <param name="model">宏特征所在文档的指针</param>
        /// <param name="feature">特征指针</param>
        /// <returns>操作结果。使用 <see cref="Structures.RebuildResult"/> 类的静态方法生成结果</returns>
        RebuildResult OnRebuild(ISwApplication app, ISwDocument model, ISwMacroFeature feature);

        /// <summary>
        /// 当特征状态改变时调用（例如特征被选中、移动、更新等）
        /// 使用此方法为特征提供额外的动态安全选项（例如不允许拖拽、编辑等）
        /// </summary>
        /// <param name="app">应用程序指针</param>
        /// <param name="model">特征所在模型的指针</param>
        /// <param name="feature">需要更新状态的特征指针</param>
        /// <returns>特征状态</returns>
        CustomFeatureState_e OnUpdateState(ISwApplication app, ISwDocument model, ISwMacroFeature feature);
    }

    /// <summary>
    /// 表示绑定到参数数据模型的自定义特征定义
    /// </summary>
    /// <typeparam name="TParams">参数类型</typeparam>
    public interface ISwMacroFeatureDefinition<TParams> : ISwMacroFeatureDefinition
        where TParams : class {
        /// <inheritdoc cref="ISwMacroFeatureDefinition.OnRebuild(ISwApplication, ISwDocument, ISwMacroFeature)"/>
        /// <returns>重建结果</returns>
        RebuildResult OnRebuild(ISwApplication app, ISwDocument model, ISwMacroFeature<TParams> feature);

        /// <summary>
        /// 为宏特征尺寸赋值的委托方法
        /// </summary>
        /// <param name="dim">尺寸指针</param>
        /// <param name="parameterName">参数名称</param>
        /// <param name="data">特征参数数据</param>
        void AssignDimension(ISwDimension dim, string parameterName, TParams data);

        /// <summary>
        /// 对齐宏特征尺寸的辅助函数
        /// </summary>
        /// <param name="dim">尺寸指针</param>
        /// <param name="pts">尺寸的点</param>
        /// <param name="dir">尺寸方向</param>
        /// <param name="extDir">尺寸延伸线方向</param>
        /// <remarks>使用 <see cref="XCustomFeatureDefinitionExtension"/> 扩展方法获取更多对齐特定类型尺寸的辅助函数</remarks>
        void AlignDimension(ISwDimension dim, Vec3d[] pts, Vec3d dir, Vec3d extDir);
    }

    /// <summary>
    /// 表示带有内置页面编辑器的自定义特征
    /// </summary>
    /// <typeparam name="TParams">自定义特征的参数类型</typeparam>
    /// <typeparam name="TPage">自定义特征的页面编辑器类型</typeparam>
    public interface ISwMacroFeatureDefinition<TParams, TPage> : ISwMacroFeatureDefinition<TParams>
        where TParams : class
        where TPage : class {
        /// <summary>
        /// 开始插入此自定义特征
        /// </summary>
        /// <param name="doc">要插入特征的文档</param>
        /// <param name="data">插入的数据</param>
        void Insert(ISwDocument doc, TParams data);

        /// <summary>
        /// 当需要重建此特征的几何体时调用
        /// </summary>
        /// <param name="app">应用程序</param>
        /// <param name="doc">当前文档</param>
        /// <param name="feat">自定义特征</param>
        /// <returns>此宏特征的几何体</returns>
        /// <remarks>通过 <see cref="ISwMacroFeature{TParams}.Parameters"/> 从特征中提取当前参数</remarks>
        ISwBody[] CreateGeometry(ISwApplication app, ISwDocument doc, ISwMacroFeature<TParams> feat);

        /// <summary>
        /// 为自定义特征创建预览几何体
        /// </summary>
        /// <param name="app">应用程序</param>
        /// <param name="model">当前文档</param>
        /// <param name="feat">当前特征</param>
        /// <param name="page">当前页面</param>
        /// <returns>预览主体</returns>
        /// <remarks>通过 <see cref="ISwMacroFeature{TParams}.Parameters"/> 从特征中提取当前参数</remarks>
        ISwBody[] CreatePreviewGeometry(ISwApplication app, ISwDocument model, ISwMacroFeature<TParams> feat, TPage page);

        /// <summary>
        /// 控制在预览时是否隐藏特定主体的委托方法
        /// </summary>
        /// <param name="body">正在预览的原始编辑主体</param>
        /// <param name="data">特征参数</param>
        /// <param name="page">页面数据</param>
        bool ShouldHidePreviewEditBody(ISwBody body, TParams data, TPage page);

        /// <summary>
        /// 为预览主体指定自定义颜色的委托方法
        /// </summary>
        /// <param name="body">正在预览的主体</param>
        /// <param name="color">指定的颜色 (使用 Color.Empty 采用默认颜色)</param>
        void AssignPreviewBodyColor(ISwBody body, out System.Drawing.Color color);

        /// <summary>
        /// 自定义特征参数与页面编辑器之间的转换器
        /// </summary>
        /// <param name="app">应用程序</param>
        /// <param name="doc">当前文档</param>
        /// <param name="par">自定义特征参数</param>
        /// <returns>对应的页面</returns>
        /// <remarks>在开始编辑特征或插入新特征时调用此方法一次</remarks>
        TPage ConvertParamsToPage(ISwApplication app, ISwDocument doc, TParams par);

        /// <summary>
        /// 从页面转换为自定义特征参数
        /// </summary>
        /// <param name="app">应用程序</param>
        /// <param name="doc">当前文档</param>
        /// <param name="page">要转换为自定义特征参数的页面数据</param>
        /// <param name="curParams">当前参数</param>
        /// <returns>对应的自定义特征参数</returns>
        /// <remarks>每次页面数据更改时调用此方法</remarks>
        TParams ConvertPageToParams(ISwApplication app, ISwDocument doc, TPage page, TParams curParams);
    }
    #endregion

    /// <inheritdoc/>
    public abstract class SwMacroFeatureDefinition : ISwMacroFeatureDefinition, ISwComFeature, IXServiceConsumer {
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        protected class MacroFeatureRegenerateData {
            internal ISwApplication Application { get; set; }
            internal ISwDocument Document { get; set; }
            internal ISwMacroFeature Feature { get; set; }
        }

        public event ConfigureServicesDelegate ConfigureServices;

        public event PostRebuildMacroFeatureDelegate PostRebuild {
            add {
                m_PostRebuild += value;
                m_HandlePostRebuild = m_PostRebuild != null;
            }
            remove {
                m_PostRebuild -= value;
                m_HandlePostRebuild = m_PostRebuild != null;
            }
        }

        private PostRebuildMacroFeatureDelegate m_PostRebuild;

        #region Initiation

        private readonly string m_Provider;
        protected readonly IXLogger m_Logger;

        public IXLogger Logger => m_Logger;

        protected readonly IServiceProvider m_SvcProvider;

        /// <summary>
        /// 集中访问应用程序实例，替代散布在各处的 SwUtils.App 直接调用 (优化1)
        /// </summary>
        /// <remarks>
        /// 不能在构造时缓存，因为 SwUtils.App 在 AddIn 生命周期中会被重新设置为完整配置的实例。
        /// 使用属性保持动态访问，同时提供统一的访问点。
        /// </remarks>
        protected ISwApplication App => SwUtils.App;

        protected readonly List<MacroFeatureRegenerateData> m_RebuildFeaturesQueue;

        private bool m_IsSubscribedToIdle;

        private readonly Func<SwMacroFeatureDefinition, IFeature, SwDocument, SwApplication, SwMacroFeature> m_MacroFeatInstFact;

        internal SwMacroFeatureDefinition(Func<SwMacroFeatureDefinition, IFeature, SwDocument, SwApplication, SwMacroFeature> macroFeatInstFact) {
            m_MacroFeatInstFact = macroFeatInstFact;
            m_Provider = GetType().TryGetAttribute<MissingDefinitionErrorMessage>()?.Message ?? "";
            m_RebuildFeaturesQueue = new List<MacroFeatureRegenerateData>();
            m_IsSubscribedToIdle = false;

            var svcColl = App.CustomServices.Clone();

            svcColl.Add<IXLogger>(() => new TraceLogger($"xCad.MacroFeature.{GetType().FullName}"), ServiceLifetimeScope_e.Singleton, false);
            svcColl.Add<IIconsCreator, BaseIconsCreator>(ServiceLifetimeScope_e.Singleton, false);

            OnConfigureServices(svcColl);

            m_SvcProvider = svcColl.CreateProvider();
            m_Logger = m_SvcProvider.GetService<IXLogger>();

            CustomFeatureDefinitionInstanceCache.RegisterInstance(this);

            CreateIcons(m_SvcProvider.GetService<IIconsCreator>(), MacroFeatureIconInfo.GetLocation(GetType()));
        }

        /// <summary>
        /// 创建宏特征的各类图标（普通、高亮、抑制、高分辨率等）
        /// </summary>
        /// <param name="ic">图标创建器</param>
        /// <param name="folder">图标输出目录</param>
        /// <remarks>子类可重写以自定义图标创建逻辑或跳过图标创建</remarks>
        protected virtual void CreateIcons(IIconsCreator ic, string folder) {
            if(ic == null) return;

            var icon = GetType().TryGetAttribute<IconAttribute>()?.Icon ?? Defaults.Icon;

            try {
                ic.ConvertIcon(new MacroFeatureIcon(icon, MacroFeatureIconInfo.RegularName), folder);
                ic.ConvertIcon(new MacroFeatureIcon(icon, MacroFeatureIconInfo.HighlightedName), folder);
                ic.ConvertIcon(new MacroFeatureSuppressedIcon(icon, MacroFeatureIconInfo.SuppressedName), folder);
                ic.ConvertIcon(new MacroFeatureHighResIcon(icon, MacroFeatureIconInfo.RegularName), folder);
                ic.ConvertIcon(new MacroFeatureHighResIcon(icon, MacroFeatureIconInfo.HighlightedName), folder);
                ic.ConvertIcon(new MacroFeatureSuppressedHighResIcon(icon, MacroFeatureIconInfo.SuppressedName), folder);
            } catch(Exception ex) {
                Logger.Log(ex);
            }
        }

        public SwMacroFeatureDefinition() : this((sender, feat, doc, app) => new SwMacroFeature(feat, doc, app, true)) { }

        #endregion Initiation

        #region Overrides

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public object Edit(object app, object modelDoc, object feature) {
            var swApp = app as ISldWorks; var swDoc = modelDoc as IModelDoc2; var swFeat = feature as IFeature;
            try {
                LogOperation("Editing feature", swApp, swDoc, swFeat);
                var doc = (SwDocument)App.Documents[swDoc];
                return OnEditDefinition(App, doc, m_MacroFeatInstFact(this, swFeat, doc, (SwApplication)App));
            } catch(Exception ex) {
                m_Logger.Log(ex);
                return HandleEditException(ex);
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public object Regenerate(object app, object modelDoc, object feature) {
            var swApp = app as ISldWorks; var swDoc = modelDoc as IModelDoc2; var swFeat = feature as IFeature;
            try {
                LogOperation("Regenerating feature", swApp, swDoc, swFeat);

                if(!string.IsNullOrEmpty(m_Provider)
                    && swApp.IsVersionNewerOrEqual(SwVersion_e.Sw2016)
                    && swFeat.GetDefinition() is IMacroFeatureData featData
                    && featData?.Provider != m_Provider) {

                    featData.Provider = m_Provider;
                }

                var doc = (SwDocument)App.Documents[swDoc];

                var comp = (swFeat as IEntity).IGetComponent2();
                var contextDoc = (SwDocument)App.Documents[comp?.GetParent()?.IGetModelDoc()] ?? doc;

                var macroFeatInst = m_MacroFeatInstFact(this, swFeat, contextDoc, (SwApplication)App);
                var res = OnRebuild(App, doc, macroFeatInst);

                if(m_HandlePostRebuild) {
                    AddDataToRebuildQueue(App, doc, macroFeatInst);
                    if(!m_IsSubscribedToIdle) {
                        m_IsSubscribedToIdle = true;
                        ((SldWorks)App.Sw).OnIdleNotify += OnIdleNotify;
                    }
                }

                return ParseMacroFeatureResult(res, macroFeatInst.FeatureData, swApp, swDoc);

            } catch(Exception ex) {
                m_Logger.Log(ex);
                return ex is IUserException ? ex.Message : $"Regeneration error: {ex.GetType().Name}";
            }
        }

        private int OnIdleNotify() {
            m_IsSubscribedToIdle = false;
            ((SldWorks)App.Sw).OnIdleNotify -= OnIdleNotify;
            try {
                m_RebuildFeaturesQueue.ForEach(DispatchPostBuildData);
            } finally {
                m_RebuildFeaturesQueue.Clear();
            }
            return HResult.S_OK;
        }

        private object ParseMacroFeatureResult(RebuildResult res, IMacroFeatureData featData, ISldWorks swApp, IModelDoc2 swDoc) {
            if(res == null) return null;

            if(!(res is BodyRebuildResult bodyRes))
                return res.Result ? (object)true : res.ErrorMessage;

            var bodies = (bodyRes.Bodies?
                .Select(body => (body ?? throw new InvalidCastException($"Only bodies of type '{nameof(ISwBody)}' are supported")).Body)
                ?? Enumerable.Empty<IBody2>()).ToArray();

            if(bodies.Length == 0) return true;

            if(featData == null) throw new ArgumentNullException(nameof(featData));

            if(CompatibilityUtils.IsVersionNewerOrEqual(swApp, SwVersion_e.Sw2013, 5))
                featData.EnableMultiBodyConsume = true;

            int faceIdOffset = 0;
            int edgeIdOffset = 0;
            foreach(var body in bodies) {
                featData.GetEntitiesNeedUserId(body, out var facesObj, out var edgesObj);
                var faces = facesObj.ToSwArray<Face2>();
                var edges = edgesObj.ToSwArray<Edge>();
                AssignFaceIds(swApp, swDoc, faces, featData, faceIdOffset);
                AssignEdgeIds(swApp, swDoc, edges, featData, edgeIdOffset);
                faceIdOffset += faces.Length;
                edgeIdOffset += edges.Length;
            }

            var result = bodies.ToArray();
            return result.Length == 1 ? (object)result[0] : result;
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public object Security(object app, object modelDoc, object feature) {
            var swDoc = modelDoc as IModelDoc2; var swFeat = feature as IFeature;
            try {
                var doc = (SwDocument)App.Documents[swDoc];
                return (int)OnUpdateState(App, doc, m_MacroFeatInstFact(this, swFeat, doc, (SwApplication)App));
            } catch(Exception ex) {
                m_Logger.Log(ex);
                return HandleStateException(ex);
            }
        }

        protected virtual object HandleEditException(Exception ex) => throw ex;
        protected virtual object HandleStateException(Exception ex) => throw ex;

        protected virtual void AddDataToRebuildQueue(ISwApplication app, ISwDocument doc, ISwMacroFeature macroFeatInst)
            => m_RebuildFeaturesQueue.Add(new MacroFeatureRegenerateData { Application = app, Document = doc, Feature = macroFeatInst });

        private void LogOperation(string operName, ISldWorks app, IModelDoc2 modelDoc, IFeature feature)
            => Logger.Log($"{operName}: {feature?.Name} in {modelDoc?.GetTitle()} of SOLIDWORKS session: {app?.GetProcessID()}", LoggerMessageSeverity_e.Debug);

        #endregion Overrides

        bool ISwMacroFeatureDefinition.OnEditDefinition(ISwApplication app, ISwDocument model, ISwMacroFeature feature)
            => OnEditDefinition(app, model, (SwMacroFeature)feature);

        RebuildResult ISwMacroFeatureDefinition.OnRebuild(ISwApplication app, ISwDocument model, ISwMacroFeature feature)
            => OnRebuild(app, model, feature);

        CustomFeatureState_e ISwMacroFeatureDefinition.OnUpdateState(ISwApplication app, ISwDocument model, ISwMacroFeature feature)
            => OnUpdateState(app, model, (SwMacroFeature)feature);

        public virtual bool OnEditDefinition(ISwApplication app, ISwDocument model, ISwMacroFeature feature) => true;
        public virtual RebuildResult OnRebuild(ISwApplication app, ISwDocument model, ISwMacroFeature feature) => null;

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        protected virtual void DispatchPostBuildData(MacroFeatureRegenerateData data)
            => m_PostRebuild?.Invoke(data.Application, data.Document, data.Feature);

        public virtual CustomFeatureState_e OnUpdateState(ISwApplication app, ISwDocument model, ISwMacroFeature feature)
            => CustomFeatureState_e.Default;


        protected bool m_HandlePostRebuild;

        protected virtual void AssignFaceIds(ISldWorks app, IModelDoc2 model, Face2[] faces, IMacroFeatureData featData, int startId = 0) {
            for(int i = 0; i < faces.Length; i++) {
                featData.SetFaceUserId(faces[i], startId + i, 0);
            }
        }

        protected virtual void AssignEdgeIds(ISldWorks app, IModelDoc2 model, Edge[] edges, IMacroFeatureData featData, int startId = 0) {
            for(int i = 0; i < edges.Length; i++) {
                featData.SetEdgeUserId(edges[i], startId + i, 0);
            }
        }

        protected virtual void OnConfigureServices(IXServiceCollection svcColl)
            => ConfigureServices?.Invoke(this, svcColl);
    }

    /// <inheritdoc/>
    public abstract class SwMacroFeatureDefinition<TParams> : SwMacroFeatureDefinition, ISwMacroFeatureDefinition<TParams>
        where TParams : class {
        private static SwMacroFeature CreateMacroFeatureInstance(SwMacroFeatureDefinition sender, IFeature feat, SwDocument doc, SwApplication app)
            => new SwMacroFeature<TParams>(feat, doc, app, true);

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        protected class MacroFeatureParametersRegenerateData : MacroFeatureRegenerateData {
            internal TParams Parameters { get; set; }
        }

        // 优化5: 使用独立字段替代 new event 隐藏，通过 override DispatchPostBuildData 实现分发
        private PostRebuildMacroFeatureDelegate<TParams> m_TypedPostRebuild;

        public new event PostRebuildMacroFeatureDelegate<TParams> PostRebuild {
            add {
                m_TypedPostRebuild += value;
                m_HandlePostRebuild = m_TypedPostRebuild != null;
            }
            remove {
                m_TypedPostRebuild -= value;
                m_HandlePostRebuild = m_TypedPostRebuild != null;
            }
        }

        RebuildResult ISwMacroFeatureDefinition<TParams>.OnRebuild(ISwApplication app, ISwDocument doc, ISwMacroFeature<TParams> feature)
            => OnRebuild(app, doc, feature);

        /// <inheritdoc/>
        public virtual void AssignDimension(ISwDimension dim, string parameterName, TParams data) { }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override bool OnEditDefinition(ISwApplication app, ISwDocument doc, ISwMacroFeature feature)
            => OnEditDefinition(app, doc, (ISwMacroFeature<TParams>)feature);

        public SwMacroFeatureDefinition() : base(CreateMacroFeatureInstance) { }

        public void AlignDimension(ISwDimension dim, Vec3d[] pts, Vec3d dir, Vec3d extDir) {
            if(pts != null && pts.Length == 2 && dim?.Dimension != null) {

                if(!dir.IsNaN())
                    dim.Dimension.DimensionLineDirection = dir.ToSwVec();
                if(!extDir.IsNaN())
                    dim.Dimension.ExtensionLineDirection = extDir.ToSwVec();

                dim.Dimension.ReferencePoints = pts.Select(p => p.ToSwPt()).ToArray();

                var swDispDim = dim.DisplayDimension;
                if(swDispDim.Type2 == (int)swDimensionType_e.swAngularDimension) {
                    swDispDim.IGetAnnotation().SetPosition2(
                        (pts[1].X + pts[0].X) / 2,
                        (pts[1].Y + pts[0].Y) / 2,
                        (pts[1].Z + pts[0].Z) / 2);
                }
            }
        }

        /// <inheritdoc/>
        public abstract RebuildResult OnRebuild(ISwApplication app, ISwDocument doc, ISwMacroFeature<TParams> feature);

        /// <inheritdoc/>
        public override RebuildResult OnRebuild(ISwApplication app, ISwDocument doc, ISwMacroFeature feature) {
            var paramsFeat = (SwMacroFeature<TParams>)feature;
            paramsFeat.UseCachedParameters = true;

            paramsFeat.ReadParameters(out var dims, out var dimParamNames, out _, out _, out _);

            var res = OnRebuild(app, doc, paramsFeat);

            if(dims?.Any() == true) {
                for(int i = 0; i < dims.Length; i++) {
                    if(res?.AlignDimension != null) {
                        res.AlignDimension.Invoke(dimParamNames[i], dims[i]);
                    } else {
                        AssignDimension(dims[i], dimParamNames[i], paramsFeat.Parameters);
                    }
                    dims[i].Dispose();
                }
            }

            if(m_HandlePostRebuild) {
                AddDataToRebuildQueue(app, doc, paramsFeat, paramsFeat.Parameters);
            }

            return res;
        }

        /// <inheritdoc/>
        public virtual bool OnEditDefinition(ISwApplication app, ISwDocument doc, ISwMacroFeature<TParams> feature) => true;

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        protected override void AddDataToRebuildQueue(ISwApplication app, ISwDocument doc, ISwMacroFeature macroFeatInst)
            => AddDataToRebuildQueue(app, doc, (ISwMacroFeature<TParams>)macroFeatInst, ((SwMacroFeature<TParams>)macroFeatInst).Parameters);

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        protected virtual void AddDataToRebuildQueue(ISwApplication app, ISwDocument doc, ISwMacroFeature<TParams> macroFeatInst, TParams parameters) {
            m_RebuildFeaturesQueue.Add(new MacroFeatureParametersRegenerateData {
                Application = app,
                Document = doc,
                Feature = macroFeatInst,
                Parameters = parameters
            });
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        protected override void DispatchPostBuildData(MacroFeatureRegenerateData data) {
            var paramData = (MacroFeatureParametersRegenerateData)data;
            m_TypedPostRebuild?.Invoke(paramData.Application, paramData.Document, (ISwMacroFeature<TParams>)paramData.Feature, paramData.Parameters);
        }
    }

    /// <inheritdoc/>
    public abstract class SwMacroFeatureDefinition<TParams, TPage> : SwMacroFeatureDefinition<TParams>, ISwMacroFeatureDefinition<TParams, TPage>
        where TParams : class
        where TPage : class {
        ISwBody[] ISwMacroFeatureDefinition<TParams, TPage>.CreateGeometry(
            ISwApplication app, ISwDocument doc, ISwMacroFeature<TParams> feat)
            => CreateGeometry(app, doc, feat).ToSwArray<SwBody>();

        private readonly Lazy<SwMacroFeatureEditor<TParams, TPage>> m_Editor;

        /// <summary>
        /// Default constructor
        /// </summary>
        public SwMacroFeatureDefinition() {
            m_Editor = new Lazy<SwMacroFeatureEditor<TParams, TPage>>(() => {
                var page = new SwPropertyManagerPage<TPage>((SwApplication)App, m_SvcProvider, CreatePageHandler(), CreateDynamicControls);

                var editor = new SwMacroFeatureEditor<TParams, TPage>(
                    App, this.GetType(),
                    m_SvcProvider, page, EditorBehavior);

                editor.EditingStarted += OnEditingStarted;
                editor.EditingCompleting += OnEditingCompleting;
                editor.EditingCompleted += OnEditingCompleted;
                editor.FeatureInserting += OnFeatureInserting;
                editor.PreviewUpdated += OnPreviewUpdated;
                editor.ShouldUpdatePreview += ShouldUpdatePreview;
                editor.ProvidePreviewContext += ProvidePreviewContext;
                editor.HandleEditingException += HandleEditingException;
                return editor;
            });
        }

        /// <summary>
        /// Behavior of macro feature editor
        /// </summary>
        protected virtual CustomFeatureEditorBehavior_e EditorBehavior => CustomFeatureEditorBehavior_e.Default;

        /// <summary>
        /// Override this method to handle the exception reading the macro feature parameters on editing of the macro feature
        /// </summary>
        /// <param name="feat">Feature being edited</param>
        /// <param name="ex">Exception</param>
        /// <returns>Parameters to use for feature editing</returns>
        protected virtual TParams HandleEditingException(ISwMacroFeature<TParams> feat, Exception ex) => throw ex;

        /// <summary>
        /// Checks if the preview should be updated
        /// </summary>
        /// <param name="oldData">Old parameters</param>
        /// <param name="newData">New parameters</param>
        /// <param name="page">Current page data</param>
        /// <param name="dataChanged">Indicates if the parameters of the data have changed</param>
        /// <remarks>This method is called everytime property manager page data is changed, however this is not always require preview update</remarks>
        public virtual bool ShouldUpdatePreview(TParams oldData, TParams newData, TPage page, bool dataChanged) => true;

        /// <summary>
        /// Create custom page handler
        /// </summary>
        /// <returns>Page handler</returns>
        public virtual SwPropertyManagerPageHandler CreatePageHandler()
            => m_SvcProvider.GetService<IPropertyPageHandlerProvider>().CreateHandler(App, typeof(TPage));

        /// <inheritdoc/>
        public virtual TParams ConvertPageToParams(ISwApplication app, ISwDocument doc, TPage page, TParams curParams) {
            if(typeof(TParams) == typeof(TPage)) {
                return (TParams)(object)page;
            } else {
                throw new Exception($"Override {nameof(ConvertPageToParams)} to provide the converter from TPage to TParams");
            }
        }

        /// <inheritdoc/>
        public virtual TPage ConvertParamsToPage(ISwApplication app, ISwDocument doc, TParams par) {
            if(typeof(TParams) == typeof(TPage)) {
                return (TPage)(object)par;
            } else {
                throw new Exception($"Override {nameof(ConvertParamsToPage)} to provide the converter from TParams to TPage");
            }
        }

        /// <inheritdoc/>
        public virtual ISwBody[] CreateGeometry(ISwApplication app, ISwDocument doc, ISwMacroFeature<TParams> feat) => [];

        /// <inheritdoc/>
        public virtual ISwBody[] CreatePreviewGeometry(ISwApplication app, ISwDocument doc, ISwMacroFeature<TParams> feat, TPage page)
            => CreateGeometry(app, doc, feat).ToSwArray<ISwBody>();

        /// <inheritdoc/>
        public virtual bool ShouldHidePreviewEditBody(ISwBody body, TParams data, TPage page) => false;

        /// <inheritdoc/>
        public virtual void AssignPreviewBodyColor(ISwBody body, out System.Drawing.Color color) => color = System.Drawing.Color.Empty;

        /// <inheritdoc/>
        public void Insert(ISwDocument doc, TParams data) => m_Editor.Value.Insert(doc, data);

        /// <inheritdoc/>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override bool OnEditDefinition(ISwApplication app, ISwDocument doc, ISwMacroFeature<TParams> feature) {
            ((SwMacroFeature<TParams>)feature).UseCachedParameters = true;
            m_Editor.Value.Edit(doc, feature);
            return true;
        }

        /// <inheritdoc/>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override RebuildResult OnRebuild(ISwApplication app, ISwDocument doc, ISwMacroFeature<TParams> feature)
            => new BodyRebuildResult() { Bodies = CreateGeometry(app, doc, feature) };

        /// <summary>
        /// Called when macro feature is about to be edited before Property Manager Page is opened
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="doc">Document</param>
        /// <param name="feat">Feature being edited (null if feature is being inserted)</param>
        /// <param name="page">Page data</param>
        public virtual void OnEditingStarted(ISwApplication app, ISwDocument doc, ISwMacroFeature<TParams> feat, TPage page) {
        }

        /// <summary>
        /// Called when macro feature is finishing editing and Property Manager Page is about to be closed
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="doc">Document</param>
        /// <param name="feat">Feature being edited</param>
        /// <param name="page">Page data</param>
        /// <param name="reason">Closing reason</param>
        public virtual void OnEditingCompleting(ISwApplication app, ISwDocument doc, ISwMacroFeature<TParams> feat, TPage page, PageCloseReasons_e reason) {
        }

        /// <summary>
        /// Called when macro feature is finished editing and Property Manager Page is closed
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="doc">Document</param>
        /// <param name="feat">Feature being edited</param>
        /// <param name="page">Page data</param>
        /// <param name="reason">Closing reason</param>
        public virtual void OnEditingCompleted(ISwApplication app, ISwDocument doc, ISwMacroFeature<TParams> feat, TPage page, PageCloseReasons_e reason) {
        }

        /// <summary>
        /// Called when macro feature is being created
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="doc">Document</param>
        /// <param name="feat">Feature which is being created (this feature is in not-committed state)</param>
        /// <param name="page">Page data</param>
        /// <remarks>Call <see cref="IXTransaction.Commit(System.Threading.CancellationToken)"/> on the feature to insert it into the tree</remarks>
        public virtual void OnFeatureInserting(ISwApplication app, ISwDocument doc, ISwMacroFeature<TParams> feat, TPage page)
           => feat.Commit();


        /// <summary>
        /// Called when the preview of the macro feature updated
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="doc">Document</param>
        /// <param name="feat">Feature being edited</param>
        /// <param name="page">Current page data</param>
        /// <remarks>Use <see cref="ShouldUpdatePreview(TParams, TParams, TPage, bool)"/> to control if preview needs to be updated</remarks>
        public virtual void OnPreviewUpdated(ISwApplication app, ISwDocument doc, ISwMacroFeature<TParams> feat, TPage page) {
        }

        /// <inheritdoc/>
        public virtual IControlDescriptor[] CreateDynamicControls(object tag) => null;

        /// <summary>
        /// Context for the preview of this document
        /// </summary>
        /// <param name="doc">Current document</param>
        /// <returns>Either <see cref="IXPart"/> or <see cref="ISwComponent"/></returns>
        protected virtual ISwObject ProvidePreviewContext(ISwDocument doc) {
            return doc switch {
                ISwPart part => part,
                ISwAssembly assm => assm.EditingComponent,
                _ => throw new NotSupportedException("Not supported preview context"),
            };
        }
    }

}