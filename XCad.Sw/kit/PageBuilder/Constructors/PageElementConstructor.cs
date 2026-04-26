//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using XCad.kit.PageBuilder.Base;
using XCad.UI.PropertyPage.Base;

namespace XCad.kit.PageBuilder.Constructors {
    public abstract class PageElementConstructor<TElem, TGroup, TPage> : IPageElementConstructor
            where TGroup : IGroup
            where TPage : IPage
            where TElem : IControl {
        IControl IPageElementConstructor.Create(IGroup parentGroup, IAttributeSet atts, IMetadata[] metadata, ref int idRange) => Create(parentGroup, atts, metadata, ref idRange);

        protected abstract TElem Create(IGroup parentGroup, IAttributeSet atts, IMetadata[] metadata, ref int numberOfUsedIds);
    }
}