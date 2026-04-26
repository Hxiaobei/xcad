//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using XCad.kit.PageBuilder.Base;
using XCad.kit.Services;
using XCad.Sw.UI.PropertyPage.Toolkit.Controls;
using XCad.UI.PropertyPage.Attributes;
using XCad.UI.PropertyPage.Base;

namespace XCad.Sw.UI.PropertyPage.Toolkit.Constructors {
    internal class PropertyManagerPageCustomControlConstructor
        : PropertyManagerPageBaseControlConstructor<PropertyManagerPageCustomControl, IPropertyManagerPageWindowFromHandle>, ICustomControlConstructor {
        public PropertyManagerPageCustomControlConstructor(SwApplication app, IIconsCreator iconsConv)
            : base(app, iconsConv) {
        }

        protected override PropertyManagerPageCustomControl Create(IGroup parentGroup, IAttributeSet atts, IMetadata[] metadata, ref int numberOfUsedIds)
            => new PropertyManagerPageCustomControl(m_App, parentGroup, m_IconConv, atts, metadata, ref numberOfUsedIds);
    }
}