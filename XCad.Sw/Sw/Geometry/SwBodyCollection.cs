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
using SolidWorks.Interop.swconst;
using XCad.kit.Utils;
using XCad.Sw.Base;
using XCad.Sw.Documents;

namespace XCad.Sw.Geometry {
    public interface ISwBodyCollection : IXRepository<ISwBody> {
    }

    internal abstract class SwBodyCollection : ISwBodyCollection {
        private readonly ISwDocument m_RootDoc;

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        internal SwBodyCollection(ISwDocument rootDoc) {
            m_RootDoc = rootDoc;
        }

        public ISwBody this[string name] => RepositoryHelper.Get(this, name);

        public int Count => SelectAllBodies().Count();

        public void AddRange(IEnumerable<ISwBody> ents, CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public T PreCreate<T>() where T : ISwBody => throw new NotSupportedException();

        public IEnumerator<ISwBody> GetEnumerator() => SelectAllBodies().GetEnumerator();

        public void RemoveRange(IEnumerable<ISwBody> ents, CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public bool TryGet(string name, out ISwBody ent) {
            ent = SelectAllBodies().FirstOrDefault(b => string.Equals(b.Name, name, StringComparison.CurrentCultureIgnoreCase));

            return ent != null;
        }

        public IEnumerable Filter(bool reverseOrder, params RepositoryFilterQuery[] filters) {
            bool solid;
            bool surface;
            bool wire;

            if(filters?.Any() == true) {
                solid = false;
                surface = false;
                wire = false;

                foreach(var filter in filters) {
                    solid = filter.Type == null || typeof(ISwSolidBody).IsAssignableFrom(filter.Type);
                    surface = filter.Type == null || typeof(ISwSheetBody).IsAssignableFrom(filter.Type);
                    wire = filter.Type == null || typeof(ISwWireBody).IsAssignableFrom(filter.Type);
                }
            } else {
                solid = true;
                surface = true;
                wire = true;
            }

            foreach(var ent in RepositoryHelper.FilterDefault(TrySelectSpecificBodies(solid, surface, wire), filters, reverseOrder)) {
                yield return ent;
            }
        }

        private IEnumerable<SwBody> SelectAllBodies() => TrySelectSpecificBodies(true, true, true);

        private IEnumerable<SwBody> TrySelectSpecificBodies(bool solid, bool surface, bool wire) {
            swBodyType_e bodyType;

            if(solid && !surface & !wire) {
                bodyType = swBodyType_e.swSolidBody;
            } else if(surface && !solid & !wire) {
                bodyType = swBodyType_e.swSheetBody;
            } else if(wire && !solid & !surface) {
                bodyType = swBodyType_e.swWireBody;
            } else {
                bodyType = swBodyType_e.swAllBodies;
            }

            foreach(var swBody in SelectSwBodies(bodyType) ?? Enumerable.Empty<IBody2>()) {
                yield return m_RootDoc.CreateObjectFromDispatch<SwBody>(swBody);
            }
        }

        protected abstract IEnumerable<IBody2> SelectSwBodies(swBodyType_e bodyType);
    }
}
