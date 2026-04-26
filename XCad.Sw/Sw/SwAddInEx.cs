using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using SolidWorks.Interop.swpublished;
using XCad.kit;
using XCad.kit.Diagnostics;
using XCad.kit.Reflection;
using XCad.kit.Services;
using XCad.Sw.Attributes;
using XCad.Sw.Base;
using XCad.Sw.Base.Enums;
using XCad.Sw.Delegates;
using XCad.Sw.Documents;
using XCad.Sw.Enums;
using XCad.Sw.Extensions.Attributes;
using XCad.Sw.Extensions.Delegates;
using XCad.Sw.Services;
using XCad.Sw.UI;
using XCad.Sw.UI.Commands;
using XCad.Sw.UI.Commands.Toolkit.Enums;
using XCad.Sw.UI.PropertyPage;
using XCad.Sw.UI.Toolkit;
using XCad.Sw.Utils;
using XCad.UI.PropertyPage.Delegates;
using XCad.UI.TaskPane;

namespace XCad.Sw {
    /// <summary>
    /// Represents the SolidWorks specific extensibility interface (add-in)
    /// </summary>
    public interface ISwAddInEx {
        event ExtensionConnectDelegate Connect;
        event ExtensionDisconnectDelegate Disconnect;
        event ExtensionStartupCompletedDelegate StartupCompleted;

        void OnConnect();
        void OnDisconnect();

        ISwApplication Application { get; }
        ISwCommandManager CommandManager { get; }
        IXLogger Logger { get; }

        ISwPropertyManagerPage<TData> CreatePage<TData>(CreateDynamicControlsDelegate createDynCtrlHandler = null);
        ISwPropertyManagerPage<TData> CreatePage<TData, THandler>(CreateDynamicControlsDelegate createDynCtrlHandler = null)
            where THandler : SwPropertyManagerPageHandler, new();

        ISwModelViewTab<TControl> CreateDocumentTab<TControl>(ISwDocument doc);
        ISwPopupWindow<TWindow> CreatePopupWindow<TWindow>(TWindow window);
        ISwTaskPane<TControl> CreateTaskPane<TControl>(TaskPaneSpec spec = null);
        ISwFeatureMgrTab<TControl> CreateFeatureManagerTab<TControl>(ISwDocument doc);
    }

    /// <inheritdoc/>
    [ComVisible(true)]
    public abstract class SwAddInEx : ISwAddInEx, ISwAddin, IXServiceConsumer, IDisposable {
        #region Registration

        private static RegistrationHelper m_RegHelper;

        [ComRegisterFunction]
        public static void RegisterFunction(Type t) {
            if(t.TryGetAttribute<SkipRegistrationAttribute>()?.Skip != true) {
                GetRegistrationHelper(t).Register(t);
            }
        }

        [ComUnregisterFunction]
        public static void UnregisterFunction(Type t) {
            if(t.TryGetAttribute<SkipRegistrationAttribute>()?.Skip != true) {
                GetRegistrationHelper(t).Unregister(t);
            }
        }

        private static RegistrationHelper GetRegistrationHelper(Type moduleType) {
            return m_RegHelper ?? (m_RegHelper = new RegistrationHelper(new TraceLogger(moduleType.FullName)));
        }

        #endregion Registration

        public event ExtensionConnectDelegate Connect;
        public event ExtensionDisconnectDelegate Disconnect;
        public event ConfigureServicesDelegate ConfigureServices;
        public event ExtensionStartupCompletedDelegate StartupCompleted;

        public ISwApplication Application => m_Application;
        private SwApplication m_Application;

        public ISwCommandManager CommandManager => m_CommandManager;
        private SwCommandManager m_CommandManager;

        protected int AddInId { get; private set; }
        public IXLogger Logger { get; private set; }

        private readonly List<IDisposable> m_Disposables;
        protected IServiceProvider m_SvcProvider;
        private bool m_IsDisposed;

        public SwAddInEx() {
            m_Disposables = new List<IDisposable>();
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ConnectToSW(object ThisSW, int cookie) {
            m_IsDisposed = false;

            try {
                Validate();

                var app = ThisSW as ISldWorks;
                AddInId = cookie;

                if(app.IsVersionNewerOrEqual(SwVersion_e.Sw2015)) {
                    app.SetAddinCallbackInfo2(0, this, AddInId);
                } else {
                    app.SetAddinCallbackInfo(0, this, AddInId);
                }

                m_Application = new SwApplication(app, OnStartupCompleted);
                var svcCollection = GetServiceCollection(m_Application);

                OnConfigureServices(svcCollection);
                m_SvcProvider = svcCollection.CreateProvider();
                m_Application.Init(m_SvcProvider);

                Logger = m_SvcProvider.GetService<IXLogger>();
                Logger.Log("Loading add-in", LoggerMessageSeverity_e.Debug);

                SwUtils.App = m_Application;
                m_CommandManager = new SwCommandManager(Application, AddInId, m_SvcProvider);

                OnConnect();
                m_CommandManager.TryBuildCommandTabs();

                return true;
            } catch(Exception ex) {
                HandleException(ex);
                return false;
            }
        }

        protected virtual void Validate() {
            if(this.GetType().TryGetAttribute<PartnerProductAttribute>(out _)) {
                throw new Exception($"'{nameof(PartnerProductAttribute)}' must be used with {nameof(SwPartnerAddInEx)}");
            }
        }

        protected virtual void HandleException(Exception ex) {
            var logger = Logger ?? CreateDefaultLogger();
            logger.Log(ex);
        }

        private void OnStartupCompleted(SwApplication app) {
            try {
                StartupCompleted?.Invoke(this);
            } catch(Exception ex) {
                Logger.Log(ex);
            }
        }

        private IXServiceCollection GetServiceCollection(SwApplication app) {
            var svcCollection = CreateServiceCollection();
            app.LoadServices(svcCollection);

            svcCollection.Add<IXLogger>(CreateDefaultLogger, ServiceLifetimeScope_e.Singleton);
            svcCollection.Add<IIconsCreator, BaseIconsCreator>(ServiceLifetimeScope_e.Singleton);
            svcCollection.Add<IPropertyPageHandlerProvider, DataModelPropertyPageHandlerProvider>(ServiceLifetimeScope_e.Singleton);
            svcCollection.Add<IFeatureManagerTabControlProvider, FeatureManagerTabControlProvider>(ServiceLifetimeScope_e.Singleton);
            svcCollection.Add<ITaskPaneControlProvider, TaskPaneControlProvider>(ServiceLifetimeScope_e.Singleton);
            svcCollection.Add<IModelViewControlProvider, ModelViewControlProvider>(ServiceLifetimeScope_e.Singleton);
            svcCollection.Add<ICommandGroupTabConfigurer, DefaultCommandGroupTabConfigurer>(ServiceLifetimeScope_e.Singleton);

            return svcCollection;
        }

        protected IXLogger CreateDefaultLogger() {
            var addInType = this.GetType();
            var title = GetRegistrationHelper(addInType).GetTitle(addInType);
            return new TraceLogger($"XCad.AddIn.{title}");
        }

        protected virtual IXServiceCollection CreateServiceCollection() => new ServiceCollection();

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool DisconnectFromSW() {
            Logger.Log("Unloading add-in", LoggerMessageSeverity_e.Debug);

            try {
                OnDisconnect();
                Dispose();
                return true;
            } catch(Exception ex) {
                Logger.Log(ex);
                return false;
            }
        }

        public void Dispose() {
            if(!m_IsDisposed) {
                Dispose(true);
                m_IsDisposed = true;
            }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void OnCommandClick(string cmdId) {
            try {
                m_CommandManager.HandleCommandClick(cmdId);
            } catch(Exception ex) {
                HandleException(ex);
            }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public int OnCommandEnable(string cmdId) {
            try {
                return m_CommandManager.HandleCommandEnable(cmdId);
            } catch(Exception ex) {
                HandleException(ex);
                return (int)CommandItemEnableState_e.DeselectDisable;
            }
        }

        public virtual void OnConnect() => Connect?.Invoke(this);

        public virtual void OnDisconnect() => Disconnect?.Invoke(this);

        protected virtual void Dispose(bool disposing) {
            if(disposing) {
                foreach(var dispCtrl in m_Disposables.ToArray()) {
                    try {
                        dispCtrl.Dispose();
                    } catch(Exception ex) {
                        Logger.Log(ex);
                    }
                }

                try { CommandManager?.Dispose(); } catch(Exception ex) { Logger.Log(ex); }
                try { m_Application?.Release(false); } catch(Exception ex) { Logger.Log(ex); }
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public ISwPropertyManagerPage<TData> CreatePage<TData>(CreateDynamicControlsDelegate createDynCtrlHandler = null)
            => CreatePropertyManagerPage<TData>(typeof(TData), createDynCtrlHandler);

        public ISwPropertyManagerPage<TData> CreatePage<TData, THandler>(CreateDynamicControlsDelegate createDynCtrlHandler = null)
            where THandler : SwPropertyManagerPageHandler, new()
            => CreatePropertyManagerPage<TData>(typeof(THandler), createDynCtrlHandler);

        private ISwPropertyManagerPage<TData> CreatePropertyManagerPage<TData>(Type handlerType, CreateDynamicControlsDelegate createDynCtrlHandler) {
            var handler = m_SvcProvider.GetService<IPropertyPageHandlerProvider>().CreateHandler(Application, handlerType);
            var page = new SwPropertyManagerPage<TData>(m_Application, m_SvcProvider, handler, createDynCtrlHandler);
            page.Disposed += OnItemDisposed;
            m_Disposables.Add(page);
            return page;
        }

        public ISwModelViewTab<TControl> CreateDocumentTab<TControl>(ISwDocument doc) {
            var tab = new SwModelViewTab<TControl>(
                new ModelViewTabCreator<TControl>(doc.Model.ModelViewManager, m_SvcProvider),
                doc.Model.ModelViewManager, (SwDocument)doc, Application, Logger);

            tab.InitControl();
            tab.Disposed += OnItemDisposed;
            m_Disposables.Add(tab);

            return tab;
        }

        public ISwPopupWindow<TWindow> CreatePopupWindow<TWindow>(TWindow window) {
            var parent = (IntPtr)Application.Sw.IFrameObject().GetHWnd();

            if(typeof(System.Windows.Window).IsAssignableFrom(typeof(TWindow))) {
                return new SwPopupWpfWindow<TWindow>(window, parent);
            } else if(typeof(Form).IsAssignableFrom(typeof(TWindow))) {
                return new SwPopupWinForm<TWindow>(window, parent);
            } else {
                throw new NotSupportedException($"Only {typeof(Form).FullName} or {typeof(System.Windows.Window).FullName} are supported");
            }
        }

        public ISwTaskPane<TControl> CreateTaskPane<TControl>(TaskPaneSpec spec = null) {
            var taskPane = new SwTaskPane<TControl>(
                new TaskPaneTabCreator<TControl>(Application, m_SvcProvider, spec ?? new TaskPaneSpec()), Logger);

            taskPane.Disposed += OnItemDisposed;
            m_Disposables.Add(taskPane);

            return taskPane;
        }

        public ISwFeatureMgrTab<TControl> CreateFeatureManagerTab<TControl>(ISwDocument doc) {
            var tab = new SwFeatureMgrTab<TControl>(
                new FeatureManagerTabCreator<TControl>(doc.Model.ModelViewManager, m_SvcProvider),
                (SwDocument)doc, Application, Logger);

            tab.InitControl();
            tab.Disposed += OnItemDisposed;
            m_Disposables.Add(tab);

            return tab;
        }

        protected virtual void OnConfigureServices(IXServiceCollection svcCollection) {
            ConfigureServices?.Invoke(this, svcCollection);
        }

        private void OnItemDisposed(IAutoDisposable item) {
            item.Disposed -= OnItemDisposed;
            if(m_Disposables.Contains(item)) {
                m_Disposables.Remove(item);
            } else {
                System.Diagnostics.Debug.Assert(false, "Disposable is not registered");
            }
        }
    }

    /// <inheritdoc/>
    [ComVisible(true)]
    public abstract class SwPartnerAddInEx : SwAddInEx, ISwPEManager {
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public void IdentifyToSW(object classFactory) {
            if(this.GetType().TryGetAttribute<PartnerProductAttribute>(out var att)) {
                try {
                    var res = (swPartnerEntitlementStatus_e)((ISwPEClassFactory)classFactory).SetPartnerKey(att.PartnerKey, out _);
                    if(res != swPartnerEntitlementStatus_e.swPESuccess) {
                        throw new Exception($"Failed to register partner product: {res}");
                    }
                } catch(Exception ex) {
                    var logger = Logger ?? CreateDefaultLogger();
                    logger.Log(ex);
                }
            } else {
                throw new Exception($"Decorate the add-in class with '{typeof(PartnerProductAttribute).FullName}' to specify partner key");
            }
        }

        protected override void Validate() { }
    }

    public static class SwAddInExExtension {
        public static ISwModelViewTab<TControl> CreateDocumentTabWinForm<TControl>(this ISwAddInEx addIn, ISwDocument doc)
            where TControl : Control => addIn.CreateDocumentTab<TControl>(doc);

        public static ISwModelViewTab<TControl> CreateDocumentTabWpf<TControl>(this ISwAddInEx addIn, ISwDocument doc)
            where TControl : System.Windows.UIElement => addIn.CreateDocumentTab<TControl>(doc);

        public static ISwPopupWindow<TWindow> CreatePopupWpfWindow<TWindow>(this ISwAddInEx addIn)
            where TWindow : System.Windows.Window => (SwPopupWpfWindow<TWindow>)addIn.CreatePopupWindow((TWindow)Activator.CreateInstance(typeof(TWindow)));

        public static ISwPopupWindow<TWindow> CreatePopupWinForm<TWindow>(this ISwAddInEx addIn)
            where TWindow : Form => (SwPopupWinForm<TWindow>)addIn.CreatePopupWindow((TWindow)Activator.CreateInstance(typeof(TWindow)));

        public static ISwTaskPane<TControl> CreateTaskPaneWinForm<TControl>(this ISwAddInEx addIn, TaskPaneSpec spec = null)
            where TControl : Control => addIn.CreateTaskPane<TControl>(spec);

        public static ISwTaskPane<TControl> CreateTaskPaneWpf<TControl>(this ISwAddInEx addIn, TaskPaneSpec spec = null)
            where TControl : System.Windows.UIElement => addIn.CreateTaskPane<TControl>(spec);

        public static ISwFeatureMgrTab<TControl> CreateFeatureManagerTabWpf<TControl>(this ISwAddInEx addIn, ISwDocument doc)
            where TControl : System.Windows.UIElement => addIn.CreateFeatureManagerTab<TControl>(doc);

        public static ISwFeatureMgrTab<TControl> CreateFeatureManagerTabWinForm<TControl>(this ISwAddInEx addIn, ISwDocument doc)
            where TControl : Control => addIn.CreateFeatureManagerTab<TControl>(doc);

        // Note: Assuming IXEnumTaskPane logic is either moved/adapted or dropped, if it relies strictly on IXEx base types.
        // If you still have IXEnumTaskPane, you can update its generic constraint internally to accept ISwAddInEx.
    }
}