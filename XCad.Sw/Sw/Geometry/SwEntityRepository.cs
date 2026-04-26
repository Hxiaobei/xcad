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
using XCad.kit.Utils;
using XCad.Sw.Base;
using XCad.Sw.Extensions;

namespace XCad.Sw.Geometry {
    public interface ISwEntityRepository : IXRepository<ISwEntity> {
    }

    internal abstract class SwEntityRepository : ISwEntityRepository {
        ISwEntity IXRepository<ISwEntity>.this[string name] => this[name];
        void IXRepository<ISwEntity>.AddRange(IEnumerable<ISwEntity> ents, CancellationToken cancellationToken)
            => AddRange(ents.ToSwArray<ISwEntity>(), cancellationToken);
        void IXRepository<ISwEntity>.RemoveRange(IEnumerable<ISwEntity> ents, CancellationToken cancellationToken)
            => RemoveRange(ents.ToSwArray<ISwEntity>(), cancellationToken);

        bool IXRepository<ISwEntity>.TryGet(string name, out ISwEntity ent) {
            var res = TryGet(name, out var specEnt);
            ent = specEnt;
            return res;
        }
        T IXRepository<ISwEntity>.PreCreate<T>() => PreCreate<T>();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        IEnumerator<ISwEntity> IEnumerable<ISwEntity>.GetEnumerator() => GetEnumerator();

        public ISwEntity this[string name] => throw new NotSupportedException();
        public void AddRange(IEnumerable<ISwEntity> ents, CancellationToken cancellationToken)
            => throw new NotSupportedException();
        public void RemoveRange(IEnumerable<ISwEntity> ents, CancellationToken cancellationToken)
            => RepositoryHelper.RemoveAll(this, ents, cancellationToken);

        public bool TryGet(string name, out ISwEntity ent)
            => RepositoryHelper.TryFindByName(this, name, out ent);

        public T PreCreate<T>() where T : ISwEntity
            => throw new NotSupportedException();

        public virtual int Count => IterateAllEntities().Count();

        public IEnumerable Filter(bool reverseOrder, params RepositoryFilterQuery[] filters) {
            bool faces;
            bool edges;
            bool vertices;
            bool silhouetteEdges;

            if(filters?.Any() == true) {
                faces = false;
                edges = false;
                vertices = false;
                silhouetteEdges = false;

                foreach(var filter in filters) {
                    faces = filter.Type == null || typeof(ISwFace).IsAssignableFrom(filter.Type);
                    edges = filter.Type == null || typeof(ISwEdge).IsAssignableFrom(filter.Type);
                    vertices = filter.Type == null || typeof(ISwVertex).IsAssignableFrom(filter.Type);
                    silhouetteEdges = filter.Type == null || typeof(ISwSilhouetteEdge).IsAssignableFrom(filter.Type);
                }
            } else {
                faces = true;
                edges = true;
                vertices = true;
                silhouetteEdges = true;
            }

            foreach(var ent in RepositoryHelper.FilterDefault(IterateEntities(faces, edges, vertices, silhouetteEdges), filters, reverseOrder)) {
                yield return ent;
            }
        }

        public IEnumerator<ISwEntity> GetEnumerator() => IterateAllEntities().GetEnumerator();

        private IEnumerable<ISwEntity> IterateAllEntities() => IterateEntities(true, true, true, true);

        protected abstract IEnumerable<ISwEntity> IterateEntities(bool faces, bool edges, bool vertices, bool silhouetteEdges);
    }
}