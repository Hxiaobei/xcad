//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using XCad.kit.CustomFeature;
using XCad.Sw.Documents;
using XCad.Sw.UI.PropertyPage;
using XCad.UI.PropertyPage.Enums;

namespace XCad.Sw.Features.CustomFeature {
    internal class SwMacroFeatureEditor<TData, TPage> : BaseCustomFeatureEditor<TData, TPage>
        where TData : class
        where TPage : class {
        internal event Func<ISwDocument, ISwObject> ProvidePreviewContext;

        internal SwMacroFeatureEditor(ISwApplication app, Type defType,
            IServiceProvider svcProvider,
            SwPropertyManagerPage<TPage> page, CustomFeatureEditorBehavior_e behavior)
            : base(app, defType, svcProvider, page, behavior) {
        }

        protected override ISwObject CurrentPreviewContext => ProvidePreviewContext?.Invoke(CurrentDocument);

        protected override void CompleteFeature(PageCloseReasons_e reason) {
            base.CompleteFeature(reason);

            if(reason == PageCloseReasons_e.Okay || reason == PageCloseReasons_e.Apply) {
                if(m_CurrentFeature.IsCommitted) {
                    var curMacroFeat = (SwMacroFeature<TData>)m_CurrentFeature;

                    if(curMacroFeat.UseCachedParameters) {
                        curMacroFeat.ApplyParametersCache();
                        curMacroFeat.UseCachedParameters = false;
                    }
                }
            }
        }
    }
}