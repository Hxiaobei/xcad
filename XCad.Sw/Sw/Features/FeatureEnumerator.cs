//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Collections;
using System.Collections.Generic;
using SolidWorks.Interop.sldworks;
using XCad.Sw.Documents;
using XCad.Sw.Utils;

namespace XCad.Sw.Features {
    internal abstract class FeatureEnumerator : IEnumerator<ISwFeature> {
        internal static IEnumerable<IFeature> IterateFeatures(IFeature firstFeature, bool recursive) {
            var processedFeats = new HashSet<IFeature>();

            var nextFeat = firstFeature;

            while(nextFeat != null) {
                if(processedFeats.Add(nextFeat)) {
                    yield return nextFeat;

                    if(recursive && nextFeat.GetTypeName2() != "HistoryFolder") {
                        foreach(var subFeat in IterateSubFeatures(nextFeat, processedFeats, recursive)) {
                            yield return subFeat;
                        }
                    }
                }

                nextFeat = nextFeat.IGetNextFeature();
            }
        }

        internal static IEnumerable<IFeature> IterateSubFeatures(IFeature parent, bool recursive)
            => IterateSubFeatures(parent, new HashSet<IFeature>(), recursive);

        private static IEnumerable<IFeature> IterateSubFeatures(IFeature parent, HashSet<IFeature> processedFeats, bool recursive) {
            var nextSubFeat = parent.IGetFirstSubFeature();

            while(nextSubFeat != null) {
                if(processedFeats.Add(nextSubFeat)) {
                    yield return nextSubFeat;

                    if(recursive) {
                        foreach(var subSubFeat in IterateSubFeatures(nextSubFeat, processedFeats, recursive)) {
                            yield return subSubFeat;
                        }
                    }
                }

                nextSubFeat = nextSubFeat.IGetNextSubFeature();
            }
        }

        public ISwFeature Current {
            get {
                var feat = m_RootDoc.CreateObjectFromDispatch<SwFeature>(m_Features.Current);
                feat.SetContext(m_Context);
                return feat;
            }
        }

        object IEnumerator.Current => Current;

        private readonly ISwDocument m_RootDoc;
        private readonly IFeature m_FirstFeat;
        private readonly Context m_Context;
        private IEnumerator<IFeature> m_Features;

        internal FeatureEnumerator(ISwDocument rootDoc, IFeature firstFeat, Context context) {
            m_RootDoc = rootDoc;
            m_FirstFeat = firstFeat;
            m_Context = context;
            Reset();
        }

        public bool MoveNext() => m_Features.MoveNext();

        public void Reset() {
            m_Features?.Dispose();
            m_Features = IterateFeatures(m_FirstFeat, true).GetEnumerator();
        }

        public void Dispose() {
            m_Features?.Dispose();
        }
    }
}