//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************


//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using XCad.kit.PageBuilder.Base;

namespace XCad.kit.PageBuilder.Constructors {
    public abstract class PageConstructor<TPage> : IPageConstructor<TPage>
        where TPage : IPage {
        TPage IPageConstructor<TPage>.Create(IAttributeSet atts) => Create(atts);

        protected abstract TPage Create(IAttributeSet atts);
    }
}