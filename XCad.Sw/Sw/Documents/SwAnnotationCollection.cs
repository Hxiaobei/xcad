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
using XCad.Sw.Annotations;
using XCad.Sw.Base;

namespace XCad.Sw.Documents {
    public interface ISwAnnotationCollection : IXRepository<ISwAnnotation> {
    }

    internal abstract class SwAnnotationCollection : ISwAnnotationCollection {
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        protected readonly SwDocument m_Doc;

        protected SwAnnotationCollection(SwDocument doc) {
            m_Doc = doc;
        }

        public ISwAnnotation this[string name] => RepositoryHelper.Get(this, name);

        public virtual int Count => IterateAllAnnotations().Count();

        public void AddRange(IEnumerable<ISwAnnotation> ents, CancellationToken cancellationToken)
            => RepositoryHelper.AddRange(ents, cancellationToken);

        public IEnumerator<ISwAnnotation> GetEnumerator() => IterateAllAnnotations().GetEnumerator();

        private IEnumerable<ISwAnnotation> IterateAllAnnotations() => IterateAnnotations(true, true, true);

        protected abstract IEnumerable<ISwAnnotation> IterateAnnotations(bool notes, bool dimensions, bool other);

        public void RemoveRange(IEnumerable<ISwAnnotation> ents, CancellationToken cancellationToken)
            => throw new NotImplementedException();

        public bool TryGet(string name, out ISwAnnotation ent)
            => throw new NotSupportedException();

        public T PreCreate<T>() where T : ISwAnnotation {
            throw new NotImplementedException();
        }

        public IEnumerable Filter(bool reverseOrder, params RepositoryFilterQuery[] filters) {
            throw new NotImplementedException();
        }
    }

    internal class SwDocument3DAnnotationCollection : SwAnnotationCollection {
        public SwDocument3DAnnotationCollection(SwDocument3D doc) : base(doc) {
        }

        public override int Count => m_Doc.Model.Extension.GetAnnotationCount();

        protected override IEnumerable<ISwAnnotation> IterateAnnotations(bool notes, bool dimensions, bool all) {
            var ann = m_Doc.Model.IGetFirstAnnotation2();

            while(ann != null) {
                yield return m_Doc.CreateObjectFromDispatch<ISwAnnotation>(ann);

                ann = ann.IGetNext2();
            }
        }
    }

    internal class SwDrawingAnnotationCollection : SwAnnotationCollection {
        private readonly SwDrawing m_Drw;

        public SwDrawingAnnotationCollection(SwDrawing drw) : base(drw) {
            m_Drw = drw;
        }

        protected override IEnumerable<ISwAnnotation> IterateAnnotations(bool notes, bool dimensions, bool other) {
            throw new NotImplementedException();
        }
    }
}
