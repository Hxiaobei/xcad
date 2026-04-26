using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using SolidWorks.Interop.sldworks;
using XCad.kit.Services;
using XCad.kit.Utils;
using XCad.Sw.Base;
using XCad.Sw.Documents.Enums;

namespace XCad.Sw.Documents {
    public interface ISwComponentCollection : IXRepository<ISwComponent> {
        new ISwComponent this[string name] { get; }
        int TotalCount { get; }
    }

    internal abstract class SwComponentCollection : ISwComponentCollection {

        ISwComponent IXRepository<ISwComponent>.this[string name] => this[name];

        public ISwComponent this[string name] => (SwComponent)RepositoryHelper.Get(this, name);

        public bool TryGet(string name, out ISwComponent ent) {
            if(RootAssembly.IsCommitted) {
                return TryGetByName(name, out ent);
            } else {
                return m_Cache.TryGet(name, out ent);
            }
        }

        protected abstract bool TryGetByName(string name, out ISwComponent ent);

        public int Count {
            get {
                if(RootAssembly.IsCommitted) {
                    if(RootAssembly.Model.IsOpenedViewOnly()) {
                        throw new Exception("Components count is inaccurate in Large Design Review assembly");
                    }

                    return GetChildrenCount();
                } else {
                    return m_Cache.Count;
                }
            }
        }

        public int TotalCount {
            get {
                if(RootAssembly.IsCommitted) {
                    if(RootAssembly.Model.IsOpenedViewOnly()) {
                        throw new Exception("Total components count is inaccurate in Large Design Review assembly");
                    }

                    return GetTotalChildrenCount();
                } else {
                    throw new Exception("Assembly is not committed");
                }
            }
        }

        internal SwAssembly RootAssembly { get; }

        private readonly EntityCache<ISwComponent> m_Cache;

        internal SwComponentCollection(SwAssembly assm) {
            RootAssembly = assm;
            m_Cache = new EntityCache<ISwComponent>(assm, this, c => c.Name);
        }

        public void AddRange(IEnumerable<ISwComponent> ents, CancellationToken cancellationToken) {
            if(!RootAssembly.IsCommitted) {
                m_Cache.AddRange(ents, cancellationToken);
            }
        }

        internal void CommitCache(CancellationToken cancellationToken) => m_Cache.Commit(cancellationToken);

        protected abstract IEnumerable<IComponent2> IterateChildren();

        protected abstract int GetChildrenCount();
        protected abstract int GetTotalChildrenCount();

        public IEnumerator<ISwComponent> GetEnumerator() {
            if(RootAssembly.IsCommitted) {
                if(RootAssembly.Model.IsOpenedViewOnly()) {
                    throw new Exception("Components cannot be extracted for the Large Design Review assembly");
                }

                return (IterateChildren() ?? new IComponent2[0])
                    .Select(c => RootAssembly.CreateObjectFromDispatch<SwComponent>(c)).GetEnumerator();
            } else {
                return m_Cache.GetEnumerator();
            }
        }

        public IEnumerable Filter(bool reverseOrder, params RepositoryFilterQuery[] filters) => RepositoryHelper.FilterDefault(this, filters, reverseOrder);

        public void RemoveRange(IEnumerable<ISwComponent> ents, CancellationToken cancellationToken) {
            if(RootAssembly.IsCommitted) {
                RepositoryHelper.RemoveAll(this, ents, cancellationToken);
            } else {
                m_Cache.RemoveRange(ents, cancellationToken);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        protected string GetRelativeName(IComponent2 comp) {
            var parentComp = comp.GetParent();

            if(parentComp == null) {
                return comp.Name2;
            } else {
                if(comp.Name2.StartsWith(parentComp.Name2, StringComparison.CurrentCultureIgnoreCase)) {
                    return comp.Name2.Substring(parentComp.Name2.Length + 1);
                } else {
                    throw new Exception("Invalid component name");
                }
            }
        }

        public T PreCreate<T>() where T : ISwComponent
            => RepositoryHelper.PreCreate<ISwComponent, T>(this,
                () => new SwPartComponent(null, RootAssembly, RootAssembly.OwnerApplication),
                () => new SwAssemblyComponent(null, RootAssembly, RootAssembly.OwnerApplication));
    }

    public static class SwComponentCollectionExtension {
        /// <summary>
        /// Pre creates new component from path
        /// </summary>
        /// <param name="docsColl">Documents collection</param>
        /// <param name="path"></param>
        /// <returns>Pre-created document</returns>
        public static ISwComponent PreCreateFromPath(this ISwComponentCollection compsColl, string path) {
            var ext = Path.GetExtension(path);

            ISwComponent comp;

            switch(ext.ToLower()) {
                case ".sldprt":
                    comp = compsColl.PreCreate<ISwPartComponent>();

                    break;

                case ".sldasm":
                    comp = compsColl.PreCreate<ISwAssemblyComponent>();
                    break;

                default:
                    throw new NotSupportedException("Only parts and assemblies are supported");
            }

            var app = ((SwComponentCollection)compsColl).RootAssembly.OwnerApplication;

            comp.ReferencedDocument = (ISwDocument3D)app.Documents.PreCreateFromPath(path);

            return comp;
        }
    }

    /// <summary>
    /// Additonal methods of <see cref="ISwComponentCollection"/>
    /// </summary>
    public static class XComponentRepositoryExtension {
        /// <summary>
        /// Returns all components, including children
        /// </summary>
        /// <param name="repo">Components repository</param>
        /// <returns>All components</returns>
        public static IEnumerable<ISwComponent> TryFlatten(this ISwComponentCollection repo) {
            IEnumerator<ISwComponent> enumer;

            try {
                enumer = repo.GetEnumerator();
            } catch {
                yield break;
            }

            while(true) {
                ISwComponent comp;

                try {
                    if(!enumer.MoveNext()) {
                        break;
                    }

                    comp = enumer.Current;
                } catch {
                    break;
                }

                yield return comp;

                ISwComponentCollection children;

                var state = comp.State;

                if(!state.HasFlag(ComponentState_e.Suppressed) && !state.HasFlag(ComponentState_e.SuppressedIdMismatch)) {
                    try {
                        children = comp.Children;
                    } catch {
                        children = null;
                    }
                } else {
                    children = null;
                }

                if(children != null) {
                    foreach(var subComp in TryFlatten(children)) {
                        yield return subComp;
                    }
                }
            }
        }

        /// <summary>
        /// Creates a template for part component
        /// </summary>
        /// <returns>Part component template</returns>
        public static ISwPartComponent PreCreatePartComponent(this ISwComponentCollection repo) => repo.PreCreate<ISwPartComponent>();

        /// <summary>
        /// Creates a template for assembly component
        /// </summary>
        /// <returns>Assembly component template</returns>
        public static ISwAssemblyComponent PreCreateAssemblyComponent(this ISwComponentCollection repo) => repo.PreCreate<ISwAssemblyComponent>();
    }
}
