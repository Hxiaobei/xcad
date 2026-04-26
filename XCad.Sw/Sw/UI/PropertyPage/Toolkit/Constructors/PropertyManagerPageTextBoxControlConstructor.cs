//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using XCad.kit.PageBuilder.Attributes;
using XCad.kit.PageBuilder.Base;
using XCad.kit.Services;
using XCad.Sw.UI.PropertyPage.Toolkit.Controls;
using XCad.UI.PropertyPage.Base;

namespace XCad.Sw.UI.PropertyPage.Toolkit.Constructors {
    [DefaultType(typeof(string))]
    internal class PropertyManagerPageTextBoxControlConstructor
        : PropertyManagerPageBaseControlConstructor<PropertyManagerPageTextBoxControl, IPropertyManagerPageTextbox> {
        public PropertyManagerPageTextBoxControlConstructor(SwApplication app, IIconsCreator iconsConv)
            : base(app, iconsConv) {
        }

        protected override PropertyManagerPageTextBoxControl Create(IGroup parentGroup, IAttributeSet atts, IMetadata[] metadata, ref int numberOfUsedIds)
            => new PropertyManagerPageTextBoxControl(m_App, parentGroup, m_IconConv, atts, metadata, ref numberOfUsedIds);
    }
}