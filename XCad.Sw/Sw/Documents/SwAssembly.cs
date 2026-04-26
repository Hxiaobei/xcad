using System;
using System.Collections.Generic;
using System.Threading;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using XCad.Sw.Base;
using XCad.Sw.Documents.Delegates;
using XCad.Sw.Documents.Exceptions;
using XCad.Sw.Extensions;
using XCad.Sw.Utils;

namespace XCad.Sw.Documents {
    public interface ISwAssembly : ISwDocument3D {
        /// <summary>
        /// Raised when new component is inserted into the assembly
        /// </summary>
        event ComponentInsertedDelegate ComponentInserted;

        /// <summary>
        /// Raised when component is about to be deleted from the assembly
        /// </summary>
        event ComponentDeletingDelegate ComponentDeleting;

        /// <summary>
        /// Raised when component is deleted from the assembly
        /// </summary>
        event ComponentDeletedDelegate ComponentDeleted;

        /// <inheritdoc/>
        new ISwAssemblyConfigurationCollection Configurations { get; }

        /// <summary>
        /// Returns the component which is currently being editied in-context or null
        /// </summary>
        ISwComponent EditingComponent { get; }
        IAssemblyDoc Assembly { get; }
    }

    internal class SwAssembly : SwDocument3D, ISwAssembly {
        public event ComponentInsertedDelegate ComponentInserted {
            add => m_ComponentInsertedEventsHandler.Attach(value);
            remove => m_ComponentInsertedEventsHandler.Detach(value);
        }

        public event ComponentDeletingDelegate ComponentDeleting {
            add => m_ComponentDeletingEventsHandler.Attach(value);
            remove => m_ComponentDeletingEventsHandler.Detach(value);
        }

        public event ComponentDeletedDelegate ComponentDeleted {
            add => m_ComponentDeletedEventsHandler.Attach(value);
            remove => m_ComponentDeletedEventsHandler.Detach(value);
        }

        public IAssemblyDoc Assembly => Model as IAssemblyDoc;

        private readonly Lazy<SwAssemblyConfigurationCollection> m_LazyConfigurations;

        private readonly ComponentInsertedEventsHandler m_ComponentInsertedEventsHandler;
        private readonly ComponentDeletingEventsHandler m_ComponentDeletingEventsHandler;
        private readonly ComponentDeletedEventsHandler m_ComponentDeletedEventsHandler;

        internal SwAssembly(IAssemblyDoc assembly, SwApplication app, IXLogger logger, bool isCreated)
            : base((IModelDoc2)assembly, app, logger, isCreated) {
            m_ComponentInsertedEventsHandler = new ComponentInsertedEventsHandler(this, app);
            m_ComponentDeletingEventsHandler = new ComponentDeletingEventsHandler(this, app);
            m_ComponentDeletedEventsHandler = new ComponentDeletedEventsHandler(this, app, logger);

            m_LazyConfigurations = new Lazy<SwAssemblyConfigurationCollection>(() => new SwAssemblyConfigurationCollection(this, app));
        }

        ISwAssemblyConfigurationCollection ISwAssembly.Configurations => m_LazyConfigurations.Value;
        internal protected override swDocumentTypes_e? DocumentType => swDocumentTypes_e.swDocASSEMBLY;

        public ISwComponent EditingComponent {
            get {
                var comp = Assembly.GetEditTargetComponent();

                if(comp != null && !comp.IsRoot()) {
                    return this.CreateObjectFromDispatch<ISwComponent>(comp);
                } else {
                    return null;
                }
            }
        }

        protected override void CommitCache(IModelDoc2 model, CancellationToken cancellationToken) {
            base.CommitCache(model, cancellationToken);

            if(m_LazyConfigurations.IsValueCreated) {
                if(m_LazyConfigurations.Value.ActiveNonCommittedConfigurationLazy.IsValueCreated) {
                    ((SwComponentCollection)((SwAssemblyConfiguration)m_LazyConfigurations.Value.ActiveNonCommittedConfigurationLazy.Value).Components)
                        .CommitCache(cancellationToken);
                }
            }
        }

        protected override SwAnnotationCollection CreateAnnotations()
            => new SwDocument3DAnnotationCollection(this);

        protected override SwConfigurationCollection CreateConfigurations()
            => new SwAssemblyConfigurationCollection(this, OwnerApplication);

        protected override bool IsDocumentTypeCompatible(swDocumentTypes_e docType) => docType == swDocumentTypes_e.swDocASSEMBLY;
    }

    internal class SwAssemblyComponentCollection : SwComponentCollection {
        private readonly SwAssembly m_Assm;

        private readonly IConfiguration m_Conf;

        public SwAssemblyComponentCollection(SwAssembly assm, IConfiguration conf) : base(assm) {
            m_Assm = assm;
            m_Conf = conf;
        }

        protected bool IsActiveConfiguration => m_Assm.Model.GetActiveConfiguration() == m_Conf;

        protected override bool TryGetByName(string name, out ISwComponent ent) {
            var comp = RootAssembly.Assembly.GetComponentByName(name);

            if(comp != null) {
                if(!IsActiveConfiguration) {
                    var rootComp = m_Conf.GetRootComponent3(true);

                    var compId = comp.GetID();

                    comp = null;

                    //finding the correspodning configuration specific component

                    foreach(var corrComp in rootComp.GetChildren().ToSwArray<Component2>()) {
                        if(corrComp.GetID() == compId) {
                            comp = corrComp;
                            break;
                        }
                    }
                }
            }

            if(comp != null) {
                ent = RootAssembly.CreateObjectFromDispatch<SwComponent>(comp);
                return true;
            } else {
                ent = null;
                return false;
            }
        }

        protected override IEnumerable<IComponent2> IterateChildren() {
            ValidateSpeedPak();

            return new OrderedComponentsCollection(
                    () => m_Conf.GetRootComponent3(!IsActiveConfiguration).GetChildren().ToSwArray<IComponent2>(),
                    m_Assm.Model.IFirstFeature(),
                    m_Assm.OwnerApplication.Logger);
        }

        protected override int GetTotalChildrenCount() {
            ValidateSpeedPak();
            return m_Assm.Assembly.GetComponentCount(false);
        }

        protected override int GetChildrenCount() {
            ValidateSpeedPak();
            return m_Assm.Assembly.GetComponentCount(true);
        }

        private void ValidateSpeedPak() {
            if(m_Conf.IsSpeedPak()) {
                throw new SpeedPakConfigurationComponentsException();
            }
        }
    }
}