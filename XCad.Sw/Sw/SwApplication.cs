using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using XCad.kit;
using XCad.kit.Data;
using XCad.kit.Diagnostics;
using XCad.kit.Services;
using XCad.Sw;
using XCad.Sw.Base;
using XCad.Sw.Base.Attributes;
using XCad.Sw.Base.Enums;
using XCad.Sw.Delegates;
using XCad.Sw.Documents;
using XCad.Sw.Enums;
using XCad.Sw.Services;
using XCad.Sw.UI;
using XCad.Sw.Utils;
using XCad.UI;

namespace XCad.Sw {
    public interface ISwApplication : IXTransaction, IDisposable {
        ISldWorks Sw { get; }

        IXServiceCollection CustomServices { get; set; }

        ISwDocumentCollection Documents { get; }

        TObj CreateObjectFromDispatch<TObj>(object disp, ISwDocument doc)
            where TObj : ISwObject;

        /// <summary>
        /// Fires when application is starting
        /// </summary>
        event ApplicationStartingDelegate Starting;

        /// <summary>
        /// Fires when no activity detected in the application
        /// </summary>
        event ApplicationIdleDelegate Idle;

        /// <summary>
        /// State of the application
        /// </summary>
        ApplicationState_e State { get; set; }

        /// <summary>
        /// Checks if this application is alive
        /// </summary>
        bool IsAlive { get; }

        /// <summary>
        /// Returns the rectangle of the application window
        /// </summary>
        Rectangle WindowRectangle { get; }

        /// <summary>
        /// Window handle of the application main window
        /// </summary>
        IntPtr WindowHandle { get; }

        /// <summary>
        /// Application process
        /// </summary>
        Process Process { get; }

        /// <summary>
        /// Displays the message box
        /// </summary>
        /// <param name="msg">Message to display</param>
        /// <param name="icon">Message box icon</param>
        /// <param name="buttons">Message box buttons</param>
        /// <returns>Button clicked by the user</returns>
        MessageBoxResult_e ShowMessageBox(string msg, MessageBoxIcon_e icon = MessageBoxIcon_e.Info, MessageBoxButtons_e buttons = MessageBoxButtons_e.Ok);

        /// <summary>
        /// Displays the modeless tooltip
        /// </summary>
        /// <param name="spec">Specification of the tooltip</param>
        void ShowTooltip(ITooltipSpec spec);

        /// <summary>
        /// Creates an object tracker to track objects across operations
        /// </summary>
        /// <param name="name">Name of the tracker</param>
        /// <returns>Tracker</returns>
        SwObjectTracker CreateObjectTracker(string name);

        /// <summary>
        /// Close current instance of the application
        /// </summary>
        void Close();
    }

    /// <inheritdoc/>
    internal class SwApplication : ISwApplication, IXServiceConsumer {
        #region WinApi
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        #endregion

        ISwDocumentCollection ISwApplication.Documents => Documents;

        public event ApplicationStartingDelegate Starting;
        public event ConfigureServicesDelegate ConfigureServices;

        public event ApplicationIdleDelegate Idle {
            add {
                if(m_IdleDelegate == null) {
                    ((SldWorks)Sw).OnIdleNotify += OnIdleNotify;
                }

                m_IdleDelegate += value;
            }
            remove {
                m_IdleDelegate -= value;

                if(m_IdleDelegate == null) {
                    ((SldWorks)Sw).OnIdleNotify -= OnIdleNotify;
                }
            }
        }

        private int OnIdleNotify() {
            m_IdleDelegate?.Invoke(this);

            return HResult.S_OK;
        }

        private IXServiceCollection m_CustomServices;

        public ISldWorks Sw => m_Creator.Element;

        public SwVersion Version {
            get {
                if(IsCommitted) {
                    var major = Sw.GetVersion(out var sp, out var spRev);
                    var minor = sp > 0 ? sp : 0;//pre-release versiosn will have a negative SP
                    var build = spRev > 0 ? spRev : 0;
                    return new SwVersion(new Version(major, minor, build), sp, spRev);
                } else {
                    return m_Creator.CachedProperties.Get<SwVersion>();
                }
            }
            set {
                if(IsCommitted) {
                    throw new Exception("Version cannot be changed after the application is committed");
                } else {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        private SwDocumentCollection m_Documents;

        public ISwDocumentCollection Documents => m_Documents;

        public IntPtr WindowHandle => new IntPtr(Sw.IFrameObject().GetHWndx64());

        public Process Process => Process.GetProcessById(Sw.GetProcessID());

        public Rectangle WindowRectangle => new Rectangle(Sw.FrameLeft, Sw.FrameTop, Sw.FrameWidth, Sw.FrameHeight);

        public bool IsCommitted => m_Creator.IsCreated;

        public ApplicationState_e State {
            get {
                if(IsCommitted) {
                    return GetApplicationState();
                } else {
                    return m_Creator.CachedProperties.Get<ApplicationState_e>();
                }
            }
            set {
                if(IsCommitted) {
                    var curState = GetApplicationState();

                    if(curState == value) {
                        //do nothing
                    } else if(((int)curState - (int)value) == (int)ApplicationState_e.Hidden) {
                        Sw.Visible = true;
                    } else if((int)value - ((int)curState) == (int)ApplicationState_e.Hidden) {
                        Sw.Visible = false;
                    } else {
                        throw new Exception("Only visibility can changed after the application is started");
                    }
                } else {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        public IXServiceCollection CustomServices {
            get => m_CustomServices;
            set {
                if(!IsCommitted) {
                    m_CustomServices = value;
                } else {
                    throw new Exception("Services can only be set before committing");
                }
            }
        }

        internal IXLogger Logger { get; private set; }

        internal IServiceProvider Services { get; private set; }

        public bool IsAlive {
            get {
                try {
                    if(Process == null || Process.HasExited || !Process.Responding) {
                        return false;
                    } else {
                        var testCall = Sw.RevisionNumber();
                        return true;
                    }
                } catch {
                    return false;
                }
            }
        }

        private bool m_IsDisposed;
        private bool m_IsClosed;

        private bool m_IsInitialized;

        private bool m_HideOnStartup;

        private bool m_IsStartupNotified;

        private readonly IElementCreator<ISldWorks> m_Creator;

        private ApplicationIdleDelegate m_IdleDelegate;

        private readonly Action<SwApplication> m_StartupCompletedCallback;

        internal GlobalTagsRegistry TagsRegistry { get; }

        internal SwApplication(ISldWorks app, IXServiceCollection customServices)
            : this(app, default(Action<SwApplication>)) {
            customServices = customServices ?? new ServiceCollection();

            LoadServices(customServices);
            Init(customServices.CreateProvider());
        }

        /// <summary>
        /// Only to be used within SwAddInEx
        /// </summary>
        internal SwApplication(ISldWorks app, Action<SwApplication> startupCompletedCallback) {
            m_IsStartupNotified = false;
            m_StartupCompletedCallback = startupCompletedCallback;

            TagsRegistry = new GlobalTagsRegistry();

            m_Creator = new ElementCreator<ISldWorks>(CreateInstance, app, true);
            WatchStartupCompleted((SldWorks)app);
        }

        /// <Remarks>
        /// Used for <see cref="SwApplicationFactory.PreCreate"/>
        /// </Remarks>
        internal SwApplication() {
            m_IsStartupNotified = false;

            TagsRegistry = new GlobalTagsRegistry();

            m_Creator = new ElementCreator<ISldWorks>(CreateInstance, null, false);

            m_Creator.CachedProperties.Set(new ServiceCollection(), nameof(CustomServices));
        }

        internal void LoadServices(IXServiceCollection customServices) {
            if(!m_IsInitialized) {
                m_CustomServices = customServices;

                customServices.Add<IFilePathResolver>(() => new SwFilePathResolverNoSearchFolders(this), ServiceLifetimeScope_e.Singleton, false);//TODO: there is some issue with recursive search of folders in search locations - do a test to validate
                customServices.Add<IIconsCreator, BaseIconsCreator>(ServiceLifetimeScope_e.Singleton, false);

                ConfigureServices?.Invoke(this, customServices);
            } else {
                Debug.Assert(false, "App has been already initialized. Must be only once");
            }
        }

        internal void Init(IServiceProvider svcProvider) {
            if(!m_IsInitialized) {
                m_IsInitialized = true;

                Services = svcProvider;
                Logger = Services.GetService<IXLogger>();

                m_Documents = new SwDocumentCollection(this, Logger);
            } else {
                Debug.Assert(false, "App has been already initialized. Must be only once");
            }
        }

        public MessageBoxResult_e ShowMessageBox(string msg, MessageBoxIcon_e icon = MessageBoxIcon_e.Info, MessageBoxButtons_e buttons = MessageBoxButtons_e.Ok) {
            swMessageBoxBtn_e swBtn = 0;
            swMessageBoxIcon_e swIcon = 0;

            switch(icon) {
                case MessageBoxIcon_e.Info:
                    swIcon = swMessageBoxIcon_e.swMbInformation;
                    break;

                case MessageBoxIcon_e.Question:
                    swIcon = swMessageBoxIcon_e.swMbQuestion;
                    break;

                case MessageBoxIcon_e.Error:
                    swIcon = swMessageBoxIcon_e.swMbStop;
                    break;

                case MessageBoxIcon_e.Warning:
                    swIcon = swMessageBoxIcon_e.swMbWarning;
                    break;
            }

            switch(buttons) {
                case MessageBoxButtons_e.Ok:
                    swBtn = swMessageBoxBtn_e.swMbOk;
                    break;

                case MessageBoxButtons_e.YesNo:
                    swBtn = swMessageBoxBtn_e.swMbYesNo;
                    break;

                case MessageBoxButtons_e.OkCancel:
                    swBtn = swMessageBoxBtn_e.swMbOkCancel;
                    break;

                case MessageBoxButtons_e.YesNoCancel:
                    swBtn = swMessageBoxBtn_e.swMbYesNoCancel;
                    break;
            }

            var swRes = (swMessageBoxResult_e)Sw.SendMsgToUser2(msg, (int)swIcon, (int)swBtn);

            switch(swRes) {
                case swMessageBoxResult_e.swMbHitOk:
                    return MessageBoxResult_e.Ok;

                case swMessageBoxResult_e.swMbHitCancel:
                    return MessageBoxResult_e.Cancel;

                case swMessageBoxResult_e.swMbHitYes:
                    return MessageBoxResult_e.Yes;

                case swMessageBoxResult_e.swMbHitNo:
                    return MessageBoxResult_e.No;

                default:
                    return 0;
            }
        }

        public void Commit(CancellationToken cancellationToken) {
            m_Creator.Create(cancellationToken);

            var customServices = CustomServices ?? new ServiceCollection();
            LoadServices(customServices);
            Init(customServices.CreateProvider());
        }

        private ISldWorks CreateInstance(CancellationToken cancellationToken) {
            m_HideOnStartup = State.HasFlag(ApplicationState_e.Hidden);

            using(var appStarter = new SwApplicationStarter(State, Version)) {
                var logger = Logger ?? new TraceLogger("xCAD.SwApplication");

                var app = appStarter.Start(p => Starting?.Invoke(this, p), logger, cancellationToken);
                WatchStartupCompleted((SldWorks)app);
                return app;
            }
        }

        private void WatchStartupCompleted(SldWorks sw) {
            sw.OnIdleNotify += OnLoadFirstIdleNotify;
        }

        private int OnLoadFirstIdleNotify() {
            Debug.Assert(!m_IsStartupNotified, "This event shoud only be fired once");

            if(!m_IsStartupNotified) {
                if(Sw?.StartupProcessCompleted == true) {
                    if(m_HideOnStartup) {
                        const int HIDE = 0;
                        ShowWindow(new IntPtr(Sw.IFrameObject().GetHWnd()), HIDE);

                        Sw.Visible = false;
                    }

                    m_IsStartupNotified = true;

                    m_StartupCompletedCallback?.Invoke(this);

                    if(Sw != null) {
                        (Sw as SldWorks).OnIdleNotify -= OnLoadFirstIdleNotify;
                    }
                }
            } else {
                (Sw as SldWorks).OnIdleNotify -= OnLoadFirstIdleNotify;
            }

            return HResult.S_OK;
        }

        private ApplicationState_e GetApplicationState() {
            //TODO: find the state
            return ApplicationState_e.Default;
        }

        public void ShowTooltip(ITooltipSpec spec) {
            IXImage icon = spec.GetType().TryGetAttribute<IconAttribute>()?.Icon;

            var bmpType = icon != null ? swBitMaps.swBitMapUserDefined : swBitMaps.swBitMapNone;

            using(var bmp = CreateTooltipIcon(icon)) {
                Sw.HideBubbleTooltip();

                Sw.ShowBubbleTooltipAt2(spec.Position.X, spec.Position.Y, (int)spec.ArrowPosition,
                            spec.Title, spec.Message, (int)bmpType,
                            bmp?.FilePaths.First(), "", 0, (int)swLinkString.swLinkStringNone, "", "");
            }
        }

        private IImageCollection CreateTooltipIcon(IXImage icon) {
            if(icon != null) {
                var iconsCreator = Services.GetService<IIconsCreator>();

                return iconsCreator.ConvertIcon(new TooltipIcon(icon));
            } else {
                return null;
            }
        }

        public TObj CreateObjectFromDispatch<TObj>(object disp, ISwDocument doc)
            where TObj : ISwObject
            => SwObjectFactory.FromDispatch<TObj>(disp, (SwDocument)doc, this);

        public SwObjectTracker CreateObjectTracker(string name)
            => new SwObjectTracker(this, name);

        internal void Release(bool close) {
            if(!m_IsDisposed) {
                m_IsDisposed = true;

                if(Services is IDisposable disposable) {
                    disposable.Dispose();
                }

                try {
                    m_Documents.Dispose();
                } catch(Exception ex) {
                    Logger.Log(ex);
                }

                TagsRegistry.Dispose();

                if(close) {
                    if(!m_IsClosed) {
                        Close();
                    }
                }

                if(Sw != null) {
                    if(Marshal.IsComObject(Sw)) {
                        Marshal.ReleaseComObject(Sw);
                    }
                }
            }
        }

        public void Dispose() => Release(true);

        public void Close() {
            if(!m_IsClosed) {
                m_IsClosed = true;
                Sw.ExitApp();
                Dispose();
            }
        }
    }

    /// <summary>
    /// Additional methods of <see cref="ISwApplication"/>
    /// </summary>
    public static class SwApplicationExtension {
        /// <summary>
        /// Checks if the current version of the SOLIDWORKS applicating equals or newver than the specified version
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="version">Major version</param>
        /// <param name="servicePack">Service pack</param>
        /// <param name="servicePackRev">Revision</param>
        /// <returns>True if current version is newer or equal</returns>
        /// <remarks>Use this method for forward compatibility</remarks>
        public static bool IsVersionNewerOrEqual(this ISwApplication app, SwVersion_e version,
            int? servicePack = null, int? servicePackRev = null) {
            return app.Sw.IsVersionNewerOrEqual(version, servicePack, servicePackRev);
        }

        /// <summary>
        /// Checks if currently running application is in-process application
        /// </summary>
        /// <param name="app">Application</param>
        /// <returns>True if in process</returns>
        /// <remarks>This method also checks the UI thread</remarks>
        public static bool IsInProcess(this ISwApplication app) {
            if(Process.GetCurrentProcess().Id == app.Process.Id) {
                return Thread.CurrentThread.ManagedThreadId == 1;
            } else {
                return false;
            }
        }
    }
}