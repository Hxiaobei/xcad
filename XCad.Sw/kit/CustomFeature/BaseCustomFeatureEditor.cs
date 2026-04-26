using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using XCad.kit.Services;
using XCad.Sw;
using XCad.Sw.Base;
using XCad.Sw.Documents;
using XCad.Sw.Exceptions;
using XCad.Sw.Extensions;
using XCad.Sw.Features;
using XCad.Sw.Features.CustomFeature;
using XCad.Sw.Geometry;
using XCad.UI.PropertyPage;
using XCad.UI.PropertyPage.Enums;
using XCad.UI.PropertyPage.Structures;

namespace XCad.kit.CustomFeature {
    // 委托定义保持不变...
    public delegate void CustomFeatureStateChangedDelegate<TData, TPage>(ISwApplication app, ISwDocument doc, ISwMacroFeature<TData> feat, TPage page)
        where TData : class
        where TPage : class;

    public delegate void CustomFeaturePageParametersChangedDelegate<TData, TPage>(ISwApplication app, ISwDocument doc, ISwMacroFeature<TData> feat, TPage page)
        where TData : class
        where TPage : class;

    public delegate void CustomFeatureInsertedDelegate<TData, TPage>(ISwApplication app, ISwDocument doc, ISwMacroFeature<TData> feat, TPage page)
        where TData : class
        where TPage : class;

    public delegate void CustomFeatureEditingCompletedDelegate<TData, TPage>(ISwApplication app, ISwDocument doc, ISwMacroFeature<TData> feat, TPage page, PageCloseReasons_e reason)
        where TData : class
        where TPage : class;

    public delegate bool ShouldUpdatePreviewDelegate<TData, TPage>(TData oldData, TData newData, TPage page, bool dataChanged)
        where TData : class
        where TPage : class;

    public delegate TData HandleEditingExceptionDelegate<TData>(ISwMacroFeature<TData> feat, Exception ex)
        where TData : class;

    [Flags]
    public enum CustomFeatureEditorBehavior_e {
        Default = 0,
        ReopenOnApply = 1
    }

    public abstract class BaseCustomFeatureEditor<TData, TPage>
        where TData : class
        where TPage : class {
        public event CustomFeatureStateChangedDelegate<TData, TPage> EditingStarted;
        public event CustomFeatureEditingCompletedDelegate<TData, TPage> EditingCompleting;
        public event CustomFeatureEditingCompletedDelegate<TData, TPage> EditingCompleted;
        public event CustomFeatureInsertedDelegate<TData, TPage> FeatureInserting;
        public event CustomFeaturePageParametersChangedDelegate<TData, TPage> PreviewUpdated;
        public event ShouldUpdatePreviewDelegate<TData, TPage> ShouldUpdatePreview;
        public event HandleEditingExceptionDelegate<TData> HandleEditingException;

        protected readonly ISwApplication m_App;
        protected readonly IServiceProvider m_SvcProvider;
        protected readonly IXLogger m_Logger;

        private readonly XObjectEqualityComparer<ISwBody> m_BodiesComparer;
        private readonly CustomFeatureParametersParser m_ParamsParser;
        private readonly Type m_DefType;

        private readonly IXPropertyPage<TPage> m_PmPage;
        private readonly Lazy<ISwMacroFeatureDefinition<TData, TPage>> m_DefinitionLazy;

        private TPage m_CurPageData;
        private ISwBody[] m_HiddenEditBodies;
        protected ISwMacroFeature<TData> m_CurrentFeature;
        private Exception m_LastError;
        private ISwBody[] m_PreviewBodies;

        protected ISwDocument CurrentDocument { get; private set; }

        private bool m_IsPageActive;
        private bool m_IsApplying;

        private readonly CustomFeatureEditorBehavior_e m_Behavior;

        protected BaseCustomFeatureEditor(ISwApplication app,
            Type featDefType,
            IServiceProvider svcProvider,
            IXPropertyPage<TPage> page,
            CustomFeatureEditorBehavior_e behavior) {
            m_App = app;
            m_SvcProvider = svcProvider;
            m_Logger = svcProvider.GetService<IXLogger>();
            m_DefType = featDefType;
            m_BodiesComparer = new XObjectEqualityComparer<ISwBody>();
            m_ParamsParser = CustomFeatureParametersParser.Instance;
            m_Behavior = behavior;

            m_DefinitionLazy = new Lazy<ISwMacroFeatureDefinition<TData, TPage>>(
                () => (ISwMacroFeatureDefinition<TData, TPage>)CustomFeatureDefinitionInstanceCache.GetInstance(m_DefType));

            m_PmPage = page;

            m_PmPage.Closing += OnPageClosing;
            m_PmPage.DataChanged += OnDataChanged;
            m_PmPage.Closed += OnPageClosed;
        }

        private ISwMacroFeatureDefinition<TData, TPage> Definition => m_DefinitionLazy.Value;
        private IEditor<ISwFeature> m_CurEditor;

        public void Edit(ISwDocument model, ISwMacroFeature<TData> feature) {
            m_IsPageActive = true;
            CurrentDocument = model ?? throw new ArgumentNullException(nameof(model));
            m_CurrentFeature = feature ?? throw new ArgumentNullException(nameof(feature));
            m_CurEditor = m_CurrentFeature.Edit();

            try {
                TData featData;
                try {
                    featData = m_CurrentFeature.Parameters;
                } catch(Exception ex) {
                    var handler = HandleEditingException;
                    if(handler == null) throw;
                    featData = handler(m_CurrentFeature, ex);
                    m_CurrentFeature.Parameters = featData;
                }

                m_CurPageData = Definition.ConvertParamsToPage(m_App, model, featData);
                EditingStarted?.Invoke(m_App, model, feature, m_CurPageData);
                m_PmPage.Show(m_CurPageData);
                UpdatePreview();
            } catch(Exception ex) {
                m_Logger.Log(ex);
                m_CurEditor.Cancel = true;
                m_CurEditor.Dispose();
                m_IsPageActive = false;
                throw;
            }
        }

        public void Insert(ISwDocument doc, TData data) {
            m_IsPageActive = true;
            CurrentDocument = doc ?? throw new ArgumentNullException(nameof(doc));

            m_CurrentFeature = CurrentDocument.Features.PreCreate<ISwMacroFeature<TData>>();
            m_CurrentFeature.DefinitionType = m_DefType;
            m_CurrentFeature.Parameters = data ?? throw new ArgumentNullException(nameof(data));

            m_CurPageData = Definition.ConvertParamsToPage(m_App, doc, data);
            EditingStarted?.Invoke(m_App, doc, m_CurrentFeature, m_CurPageData);
            m_PmPage.Show(m_CurPageData);
            UpdatePreview();
        }

        protected virtual ISwObject CurrentPreviewContext => CurrentDocument;

        private void DisplayPreview(ISwBody[] bodies) {
            if(bodies?.Any() != true) return;

            var previewContext = CurrentPreviewContext ?? throw new Exception("Preview context is not specified");
            foreach(var body in bodies) {
                Definition.AssignPreviewBodyColor(body, out Color color);
                if(color.IsEmpty) {
                    DefaultAssignPreviewBodyColor(out color);
                }
                body.Preview(previewContext, color);
            }
        }

        private void HideEditBodies() {
            var (_, _, _, _, editBodies) = ParseParameters(m_CurrentFeature.Parameters); // 5个元素，对应 atts, sels, extraData, dimVals, editBodies
            var bodiesToShow = m_HiddenEditBodies.ToSwArray<ISwBody>().Except(editBodies.ToSwArray<ISwBody>(), m_BodiesComparer);
            foreach(var body in bodiesToShow)
                body.Visible = true;

            var doNotHideBodies = new List<ISwBody>();
            var bodiesToHide = editBodies.ToSwArray<ISwBody>().Except(m_HiddenEditBodies.ToSwArray<ISwBody>(), m_BodiesComparer);

            foreach(var body in bodiesToHide) {
                bool hide = body.Visible && Definition.ShouldHidePreviewEditBody(body, m_CurrentFeature.Parameters, m_CurPageData);
                if(hide)
                    body.Visible = false;
                else
                    doNotHideBodies.Add(body);
            }

            m_HiddenEditBodies = editBodies?.Except(doNotHideBodies).ToArray();
        }

        private void HidePreviewBodies() {
            if(m_PreviewBodies == null) return;

            foreach(var body in m_PreviewBodies) {
                try {
                    body.Visible = false;
                    body.Dispose();
                } catch(Exception ex) {
                    m_Logger.Log(ex);
                }
            }
            m_PreviewBodies = null;
        }

        private void OnDataChanged() {
            if(!m_IsPageActive) return;

            var oldParams = m_CurrentFeature.Parameters;
            var newParams = Definition.ConvertPageToParams(m_App, CurrentDocument, m_CurPageData, oldParams);
            bool dataChanged = !AreParametersEqual(oldParams, newParams);

            bool needUpdatePreview = ShouldUpdatePreview?.Invoke(oldParams, newParams, m_CurPageData, dataChanged) ?? dataChanged;

            m_CurrentFeature.Parameters = newParams;
            if(needUpdatePreview) {
                UpdatePreview();
                PreviewUpdated?.Invoke(m_App, CurrentDocument, m_CurrentFeature, m_CurPageData);
            }
        }

        private bool AreParametersEqual(TData oldParams, TData newParams) {
            if(ReferenceEquals(oldParams, newParams)) return true;
            if(oldParams == null || newParams == null) return false;

            var (oldAtts, oldSels, _, oldDimVals, oldEditBodies) = ParseParameters(oldParams);
            var (newAtts, newSels, _, newDimVals, newEditBodies) = ParseParameters(newParams);

            return AreArraysEqual(oldAtts, newAtts, (o, n) => string.Equals(o.Name, n.Name) && Equals(o.Value, n.Value) && Type.Equals(o.Type, n.Type))
                && AreArraysEqual(oldSels, newSels, (o, n) => o.Equals(n))
                && AreArraysEqual(oldDimVals, newDimVals, (o, n) => double.Equals(o, n))
                && AreArraysEqual(oldEditBodies, newEditBodies, (o, n) => m_BodiesComparer.Equals(o, n));
        }

        private static bool AreArraysEqual<T>(T[] a, T[] b, Func<T, T, bool> comparer) {
            if(ReferenceEquals(a, b)) return true;
            if(a == null || b == null) return false;
            if(a.Length != b.Length) return false;
            for(int i = 0; i < a.Length; i++)
                if(!comparer(a[i], b[i]))
                    return false;
            return true;
        }

        // 修正：Parse 方法只有 5 个 out 参数，因此返回 5 元组
        private (CustomFeatureAttribute[] atts, ISwSelObject[] sels, object extraData, double[] dimVals, ISwBody[] editBodies) ParseParameters(TData data) {
            m_ParamsParser.Parse(data, out var atts, out var sels, out var extraData, out var dimVals, out var editBodies);
            return (atts, sels, extraData, dimVals, editBodies);
        }

        private void OnPageClosed(PageCloseReasons_e reason) {
            if(m_IsApplying) reason = PageCloseReasons_e.Apply;

            var cachedParams = m_CurrentFeature.Parameters;
            m_IsPageActive = false;

            try {
                CompleteFeature(reason);

                TData reusableParams = default;
                if(m_IsApplying) {
                    reusableParams = Definition.ConvertPageToParams(m_App, CurrentDocument, m_CurPageData, cachedParams);
                }

                m_CurEditor?.Dispose();
                ResetState();

                if(!m_IsApplying) {
                    CurrentDocument = null;
                } else {
                    m_IsApplying = false;
                    Insert(CurrentDocument, reusableParams);
                    m_PmPage.IsPinned = true;
                }
            } catch(Exception ex) {
                m_Logger.Log(ex);
                m_CurEditor?.Dispose();
                ResetState();
                CurrentDocument = null;
            }
        }

        private void ShowEditBodies() {
            if(m_HiddenEditBodies == null) return;
            foreach(var body in m_HiddenEditBodies.ToSwArray<ISwBody>())
                body.Visible = true;
            m_HiddenEditBodies = null;
        }

        private void OnPageClosing(PageCloseReasons_e reason, PageClosingArg arg) {
            if(m_IsApplying) return;

            if(EditingCompleting != null) {
                try {
                    EditingCompleting.Invoke(m_App, CurrentDocument, m_CurrentFeature, m_CurPageData, reason);
                } catch(Exception ex) {
                    m_Logger.Log(ex);
                    m_LastError = ex;
                }
            }

            if(m_LastError != null) {
                arg.ErrorMessage = m_LastError is IUserException ? m_LastError.Message : "Unknown error. Please see log for more details";
                arg.Cancel = true;
                return;
            }

            if(reason == PageCloseReasons_e.Apply) {
                if(m_Behavior.HasFlag(CustomFeatureEditorBehavior_e.ReopenOnApply)) {
                    m_IsApplying = true;
                    m_PmPage.Close(true);
                } else {
                    CompleteFeature(reason);
                    m_CurrentFeature.Parameters = Definition.ConvertPageToParams(m_App, CurrentDocument, m_CurPageData, m_CurrentFeature.Parameters);
                    UpdatePreview();
                }
            }
        }

        private void DefaultAssignPreviewBodyColor(out Color color) => color = Color.FromArgb(100, Color.Yellow);

        private void UpdatePreview() {
            if(!m_IsPageActive) return;

            using(CurrentDocument.ModelViews.Active.Freeze(true)) {
                try {
                    m_LastError = null;
                    HidePreviewBodies();
                    m_PreviewBodies = Definition.CreatePreviewGeometry(m_App, CurrentDocument, m_CurrentFeature, m_CurPageData);
                    HideEditBodies();
                    if(m_PreviewBodies?.Any() == true)
                        DisplayPreview(m_PreviewBodies);
                } catch(Exception ex) {
                    HidePreviewBodies();
                    ShowEditBodies();
                    m_Logger.Log(ex);
                    m_LastError = ex;
                }
            }
        }

        protected virtual void CompleteFeature(PageCloseReasons_e reason) {
            EditingCompleted?.Invoke(m_App, CurrentDocument, m_CurrentFeature, m_CurPageData, reason);

            ShowEditBodies();
            HidePreviewBodies();
            m_PreviewBodies = null;

            if(reason == PageCloseReasons_e.Okay || reason == PageCloseReasons_e.Apply) {
                if(!m_CurrentFeature.IsCommitted)
                    FeatureInserting?.Invoke(m_App, CurrentDocument, m_CurrentFeature, m_CurPageData);
            } else {
                if(m_CurrentFeature.IsCommitted)
                    m_CurEditor.Cancel = true;
            }
        }

        /// <summary>
        /// 集中清理所有可变状态，确保异常时也能完整 reset (优化6)
        /// </summary>
        private void ResetState() {
            m_CurPageData = null;
            m_HiddenEditBodies = null;
            m_CurrentFeature = null;
            m_LastError = null;
            m_PreviewBodies = null;
            m_CurEditor = null;
        }
    }
}