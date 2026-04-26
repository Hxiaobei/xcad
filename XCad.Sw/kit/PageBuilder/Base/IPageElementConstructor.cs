//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using XCad.UI.PropertyPage.Base;

namespace XCad.kit.PageBuilder.Base {
    public interface IPageElementConstructor {
        IControl Create(IGroup parentGroup, IAttributeSet atts, IMetadata[] metadata, ref int idRange);
    }
}