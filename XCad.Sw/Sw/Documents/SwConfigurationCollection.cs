//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SolidWorks.Interop.sldworks;
using XCad.kit.Utils;
using XCad.Sw.Base;
using XCad.Sw.Documents.Delegates;
using XCad.Sw.Documents.EventHandlers;

namespace XCad.Sw.Documents {
    public interface ISwConfigurationCollection : IXRepository<ISwConfiguration>, IDisposable {
        event ConfigurationActivatedDelegate ConfigurationActivated;
        ISwConfiguration PreCreate();
        ISwConfiguration Active { get; set; }
    }

    internal class SwConfigurationCollection : ISwConfigurationCollection {
        public event ConfigurationActivatedDelegate ConfigurationActivated {
            add {
                m_ConfigurationActivatedEventsHandler.Attach(value);
            }
            remove {
                m_ConfigurationActivatedEventsHandler.Detach(value);
            }
        }

        ISwConfiguration ISwConfigurationCollection.Active {
            get => Active;
            set => Active = value;
        }

        protected readonly SwApplication m_App;
        private readonly SwDocument3D m_Doc;

        private readonly ConfigurationActivatedEventsHandler m_ConfigurationActivatedEventsHandler;

        internal Lazy<ISwConfiguration> ActiveNonCommittedConfigurationLazy { get; }

        internal SwConfigurationCollection(SwDocument3D doc, SwApplication app) {
            m_App = app;
            m_Doc = doc;
            m_ConfigurationActivatedEventsHandler = new ConfigurationActivatedEventsHandler(doc, app);

            ActiveNonCommittedConfigurationLazy = new Lazy<ISwConfiguration>(() => {
                var activeConfName = m_App.Sw.GetActiveConfigurationName(m_Doc.Path);

                var conf = PreCreate();
                conf.Name = activeConfName;
                return conf;
            });
        }

        public ISwConfiguration this[string name] => RepositoryHelper.Get(this, name);

        public bool TryGet(string name, out ISwConfiguration ent) {
            if(m_Doc.IsCommitted) {
                var conf = m_Doc.Model.GetConfigurationByName(name);

                if(conf != null) {
                    ent = m_Doc.CreateObjectFromDispatch<SwConfiguration>((IConfiguration)conf);
                    return true;
                } else {
                    if(m_Doc.Model.IsOpenedViewOnly()) {
                        ent = CreateViewOnlyConfiguration(name);
                        return true;
                    } else {
                        ent = null;
                        return false;
                    }
                }
            } else {
                ent = this.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
                return ent != null;
            }
        }

        protected virtual ISwConfiguration CreateViewOnlyConfiguration(string name)
            => new SwViewOnlyUnloadedConfiguration(name, m_Doc, m_App);

        public int Count {
            get {
                if(m_Doc.IsCommitted) {
                    return m_Doc.Model.GetConfigurationCount();
                } else {
                    return this.Count();
                }
            }
        }

        public ISwConfiguration Active {
            get {
                if(m_Doc.IsCommitted) {
                    return m_Doc.CreateObjectFromDispatch<SwConfiguration>(m_Doc.Model.ConfigurationManager.ActiveConfiguration);
                } else {
                    return ActiveNonCommittedConfigurationLazy.Value;
                }
            }
            set {
                if(m_Doc.Model.ConfigurationManager.ActiveConfiguration != value.Configuration) {
                    if(!m_Doc.Model.ShowConfiguration2(value.Name)) {
                        throw new Exception($"Failed to activate configuration '{value.Name}'");
                    }
                }
            }
        }

        public virtual ISwConfiguration PreCreate() => new SwConfiguration(null, m_Doc, m_App, false);

        public void AddRange(IEnumerable<ISwConfiguration> ents, CancellationToken cancellationToken) => RepositoryHelper.AddRange(ents, cancellationToken);

        public void Dispose() {
        }

        public virtual IEnumerator<ISwConfiguration> GetEnumerator() => new SwConfigurationEnumerator(m_App, m_Doc);

        public IEnumerable Filter(bool reverseOrder, params RepositoryFilterQuery[] filters) => RepositoryHelper.FilterDefault(this, filters, reverseOrder);

        public void RemoveRange(IEnumerable<ISwConfiguration> ents, CancellationToken cancellationToken) {
            foreach(var conf in ents) {
                if(conf.IsCommitted) {
                    if(Count == 1) {
                        throw new Exception("Cannot delete the last configuration");
                    }

                    if(string.Equals(Active.Name, conf.Name)) {
                        Active = this.First(c => !string.Equals(c.Name, conf.Name, StringComparison.CurrentCultureIgnoreCase));
                    }

                    if(!m_Doc.Model.DeleteConfiguration2(conf.Name)) {
                        throw new Exception($"Failed to delete configuration '{conf.Name}'");
                    }
                } else {
                    throw new Exception($"Cannot delete uncommited configuration '{conf.Name}'");
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public T PreCreate<T>() where T : ISwConfiguration => (T)PreCreate();
    }

    internal class SwConfigurationEnumerator : SwConfigurationEnumeratorBase<SwConfiguration> {
        public SwConfigurationEnumerator(SwApplication app, SwDocument3D doc) : base(app, doc) {
        }

        protected override SwConfiguration CreateViewOnlyConfiguration(string confName)
            => new SwViewOnlyUnloadedConfiguration(confName, m_Doc, m_App);

        protected override SwConfiguration PreCreateNewConfiguration()
            => new SwConfiguration(null, m_Doc, m_App, false);
    }

    internal abstract class SwConfigurationEnumeratorBase<TConf> : IEnumerator<TConf>
        where TConf : ISwConfiguration {
        public TConf Current {
            get {
                var confName = m_ConfNames[m_CurConfIndex];

                if(m_Doc.IsCommitted) {
                    var conf = (IConfiguration)m_Doc.Model.GetConfigurationByName(confName);

                    if(conf == null) {
                        if(m_Doc.Model.IsOpenedViewOnly()) {
                            return CreateViewOnlyConfiguration(confName);
                        } else {
                            throw new Exception("Failed to get the configuration by name");

                        }
                    } else {
                        return m_Doc.CreateObjectFromDispatch<TConf>(conf);
                    }
                } else {
                    var conf = PreCreateNewConfiguration();
                    conf.Name = confName;
                    return conf;
                }
            }
        }

        object IEnumerator.Current => Current;

        private int m_CurConfIndex;

        protected readonly SwApplication m_App;
        protected readonly SwDocument3D m_Doc;

        private string[] m_ConfNames;

        internal SwConfigurationEnumeratorBase(SwApplication app, SwDocument3D doc) {
            m_App = app;
            m_Doc = doc;

            m_CurConfIndex = -1;

            if(doc.IsCommitted) {
                m_ConfNames = (string[])m_Doc.Model.GetConfigurationNames();
            } else {
                m_ConfNames = (string[])m_App.Sw.GetConfigurationNames(m_Doc.Path);
            }
        }

        public bool MoveNext() {
            m_CurConfIndex++;
            return m_CurConfIndex < m_ConfNames.Length;
        }

        public void Reset() {
            m_CurConfIndex = -1;
        }

        protected abstract TConf PreCreateNewConfiguration();
        protected abstract TConf CreateViewOnlyConfiguration(string confName);

        public void Dispose() {
        }
    }

    public interface ISwAssemblyConfigurationCollection : ISwConfigurationCollection {
        new ISwAssemblyConfiguration this[string name] { get; }
        new ISwAssemblyConfiguration PreCreate();
        new ISwAssemblyConfiguration Active { get; set; }
    }

    internal class SwAssemblyConfigurationCollection : SwConfigurationCollection, ISwAssemblyConfigurationCollection {
        private readonly SwAssembly m_Assm;

        internal SwAssemblyConfigurationCollection(SwAssembly assm, SwApplication app) : base(assm, app) {
            m_Assm = assm;
        }

        ISwAssemblyConfiguration ISwAssemblyConfigurationCollection.this[string name] => (ISwAssemblyConfiguration)base[name];

        ISwAssemblyConfiguration ISwAssemblyConfigurationCollection.Active {
            get => (ISwAssemblyConfiguration)base.Active;
            set => base.Active = value;
        }

        public void AddRange(IEnumerable<ISwAssemblyConfiguration> ents, CancellationToken cancellationToken)
            => base.AddRange(ents, cancellationToken);

        public void RemoveRange(IEnumerable<ISwAssemblyConfiguration> ents, CancellationToken cancellationToken)
            => base.RemoveRange(ents, cancellationToken);

        public bool TryGet(string name, out ISwAssemblyConfigurationCollection ent) {
            var res = base.TryGet(name, out ISwConfiguration conf);
            ent = (ISwAssemblyConfigurationCollection)conf;
            return res;
        }

        protected override ISwConfiguration CreateViewOnlyConfiguration(string name)
            => new SwLdrAssemblyUnloadedConfiguration(m_Assm, m_App, name);

        ISwAssemblyConfiguration ISwAssemblyConfigurationCollection.PreCreate()
            => new SwAssemblyConfiguration(null, m_Assm, m_App, false);

        public override ISwConfiguration PreCreate() => (this as ISwAssemblyConfigurationCollection).PreCreate();

        public override IEnumerator<ISwConfiguration> GetEnumerator() => new SwAssemblyConfigurationEnumerator(m_App, m_Assm);
    }

    internal class SwAssemblyConfigurationEnumerator : SwConfigurationEnumeratorBase<SwAssemblyConfiguration> {
        private readonly SwAssembly m_Assm;

        public SwAssemblyConfigurationEnumerator(SwApplication app, SwAssembly assm) : base(app, assm) {
            m_Assm = assm;
        }

        protected override SwAssemblyConfiguration CreateViewOnlyConfiguration(string confName)
            => new SwLdrAssemblyUnloadedConfiguration(m_Assm, m_App, confName);

        protected override SwAssemblyConfiguration PreCreateNewConfiguration()
            => new SwAssemblyConfiguration(null, m_Assm, m_App, false);
    }

    public interface ISwPartConfigurationCollection : ISwConfigurationCollection {
        new ISwPartConfiguration this[string name] { get; }
        new ISwPartConfiguration PreCreate();
        new ISwPartConfiguration Active { get; set; }
    }

    internal class SwPartConfigurationCollection : SwConfigurationCollection, ISwPartConfigurationCollection {
        private readonly SwPart m_Part;

        internal SwPartConfigurationCollection(SwPart part, SwApplication app) : base(part, app) {
            m_Part = part;
        }

        ISwPartConfiguration ISwPartConfigurationCollection.this[string name] => (ISwPartConfiguration)base[name];

        ISwPartConfiguration ISwPartConfigurationCollection.Active {
            get => (ISwPartConfiguration)base.Active;
            set => base.Active = value;
        }

        public void AddRange(IEnumerable<ISwPartConfiguration> ents, CancellationToken cancellationToken)
            => base.AddRange(ents, cancellationToken);

        public void RemoveRange(IEnumerable<ISwPartConfiguration> ents, CancellationToken cancellationToken)
            => base.RemoveRange(ents, cancellationToken);

        public bool TryGet(string name, out ISwPartConfiguration ent) {
            var res = base.TryGet(name, out ISwConfiguration conf);
            ent = (ISwPartConfiguration)conf;
            return res;
        }

        ISwPartConfiguration ISwPartConfigurationCollection.PreCreate()
            => new SwPartConfiguration(null, m_Part, m_App, false);

        public override ISwConfiguration PreCreate()
            => (this as ISwPartConfigurationCollection).PreCreate();

        public override IEnumerator<ISwConfiguration> GetEnumerator() => new SwPartConfigurationEnumerator(m_App, m_Part);
    }

    internal class SwPartConfigurationEnumerator : SwConfigurationEnumeratorBase<SwPartConfiguration> {
        private readonly SwPart m_Part;

        public SwPartConfigurationEnumerator(SwApplication app, SwPart part) : base(app, part) {
            m_Part = part;
        }

        protected override SwPartConfiguration CreateViewOnlyConfiguration(string confName)
            => new SwLdrPartUnloadedConfiguration(m_Part, m_App, confName);

        protected override SwPartConfiguration PreCreateNewConfiguration()
            => new SwPartConfiguration(null, m_Part, m_App, false);
    }
}
