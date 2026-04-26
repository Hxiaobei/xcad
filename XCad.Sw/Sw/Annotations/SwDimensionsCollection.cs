
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SolidWorks.Interop.sldworks;
using XCad.kit.Utils;
using XCad.Sw.Base;
using XCad.Sw.Documents;
using XCad.Sw.Features;
using XCad.Sw.Utils;

namespace XCad.Sw.Annotations {
    public interface ISwDimensionsCollection : IXRepository<ISwDimension>, IDisposable {
        new ISwDimension this[string name] { get; }
    }

    internal abstract class SwDimensionsCollection : ISwDimensionsCollection {
        ISwDimension IXRepository<ISwDimension>.this[string name] => this[name];

        public ISwDimension this[string name] => RepositoryHelper.Get(this, name);

        public abstract bool TryGet(string name, out ISwDimension ent);

        public int Count => throw new NotImplementedException();

        protected readonly Context m_Context;

        protected SwDimensionsCollection(Context context) {
            m_Context = context;
        }

        public void AddRange(IEnumerable<ISwDimension> ents, CancellationToken cancellationToken)
            => throw new NotImplementedException();

        public abstract IEnumerator<ISwDimension> GetEnumerator();

        public void RemoveRange(IEnumerable<ISwDimension> ents, CancellationToken cancellationToken)
            => throw new NotImplementedException();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerable Filter(bool reverseOrder, params RepositoryFilterQuery[] filters) => RepositoryHelper.FilterDefault(this, filters, reverseOrder);

        public void Dispose() {
        }

        public T PreCreate<T>() where T : ISwDimension => throw new NotImplementedException();
    }

    internal class SwFeatureManagerDimensionsCollection : SwDimensionsCollection {
        private readonly ISwFeatureManager m_FeatMgr;

        internal SwFeatureManagerDimensionsCollection(ISwFeatureManager featMgr, Context context) : base(context) {
            m_FeatMgr = featMgr;
        }

        public override IEnumerator<ISwDimension> GetEnumerator()
            => m_FeatMgr.SelectMany(f => f.Dimensions).GetEnumerator();

        public override bool TryGet(string name, out ISwDimension ent) {
            var dimNameParts = name.Split('@');

            if(dimNameParts.Length != 2) {
                throw new Exception("Invalid dimension name. Name must be specified in the following format: DimName@FeatureName");
            }

            var dimName = dimNameParts[0];
            var featName = dimNameParts[1];

            ISwDimension dim = null;

            if(m_FeatMgr.TryGet(featName, out ISwFeature feat)) {
                dim = feat.Dimensions.FirstOrDefault(
                    d => string.Equals(d.Name, $"{dimName}@{featName}",
                    StringComparison.CurrentCultureIgnoreCase));
            }

            if(dim != null) {
                ent = dim;
                return true;
            } else {
                ent = null;
                return false;
            }
        }
    }

    internal class SwFeatureDimensionsCollection : SwDimensionsCollection {
        private readonly ISwDocument m_Doc;
        private readonly SwFeature m_Feat;

        internal SwFeatureDimensionsCollection(SwFeature feat, ISwDocument doc, Context context) : base(context) {
            m_Feat = feat;
            m_Doc = doc;
        }

        public override bool TryGet(string name, out ISwDimension ent) {
            var dimNameParts = name.Split('@');

            var dimName = dimNameParts[0];
            var featName = "";

            if(dimNameParts.Length == 2) {
                featName = dimNameParts[0];

                if(!string.Equals(featName, m_Feat.Name, StringComparison.CurrentCultureIgnoreCase)) {
                    throw new Exception("Specified dimension does not belong to this feature");
                }
            }

            var dim = this.FirstOrDefault(
                d => string.Equals(d.Name, dimName,
                StringComparison.CurrentCultureIgnoreCase));

            if(dim != null) {
                ent = dim;
                return true;
            } else {
                ent = null;
                return false;
            }
        }

        public override IEnumerator<ISwDimension> GetEnumerator()
            => new SwFeatureDimensionsEnumerator(m_Feat.Feature, m_Doc, m_Context);
    }

    internal class SwFeatureDimensionsEnumerator : IEnumerator<ISwDimension> {
        public ISwDimension Current {
            get {
                var dim = m_Doc.CreateObjectFromDispatch<SwDimension>(m_CurDispDim);
                dim.SetContext(m_Context);
                return dim;
            }
        }

        object IEnumerator.Current => Current;

        private readonly ISwDocument m_Doc;
        private readonly IFeature m_Feat;
        private readonly Context m_Context;

        private IDisplayDimension m_CurDispDim;

        private bool m_IsStart;

        internal SwFeatureDimensionsEnumerator(IFeature feat, ISwDocument doc, Context context) {
            m_Doc = doc;
            m_Feat = feat;
            m_Context = context;

            m_IsStart = true;
        }

        public bool MoveNext() {
            if(m_IsStart) {
                m_IsStart = false;
                m_CurDispDim = m_Feat.GetFirstDisplayDimension() as IDisplayDimension;
            } else {
                m_CurDispDim = m_Feat.GetNextDisplayDimension(m_CurDispDim) as IDisplayDimension;
            }

            if(m_CurDispDim != null) {
                //NOTE: parent feature, such as extrude will also return all dimensions from child features, such as sketch
                var featName = m_CurDispDim.GetDimension2(0).FullName.Split('@')[1];

                if(!string.Equals(featName, m_Feat.Name, StringComparison.CurrentCultureIgnoreCase)) {
                    return MoveNext();
                }
            }

            return m_CurDispDim != null;
        }

        public void Reset() {
            m_IsStart = true;
        }

        public void Dispose() {
        }
    }
}
