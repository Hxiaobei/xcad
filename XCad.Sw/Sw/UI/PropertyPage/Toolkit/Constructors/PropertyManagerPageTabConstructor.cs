//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using XCad.kit.PageBuilder.Base;
using XCad.kit.PageBuilder.Constructors;
using XCad.kit.Services;
using XCad.Sw.UI.PropertyPage.Toolkit.Controls;
using XCad.UI.PropertyPage.Attributes;
using XCad.UI.PropertyPage.Base;

namespace XCad.Sw.UI.PropertyPage.Toolkit.Constructors {
    internal class PropertyManagerPageTabConstructor
        : GroupConstructor<PropertyManagerPageTabControl, PropertyManagerPagePage>,
        IPropertyManagerPageElementConstructor, ITabConstructor {
        public Type ControlType => typeof(PropertyManagerPageTabControl);

        private readonly SwApplication m_App;
        private readonly IIconsCreator m_IconsConv;

        public PropertyManagerPageTabConstructor(SwApplication app, IIconsCreator iconsConv) {
            m_App = app;
            m_IconsConv = iconsConv;
        }

        public void PostProcessControls(IEnumerable<IPropertyManagerPageControlEx> ctrls) {
            //TODO: not used
        }

        protected override PropertyManagerPageTabControl Create(IGroup parentGroup, IAttributeSet atts, IMetadata[] metadata, ref int numberOfUsedIds)
            => new PropertyManagerPageTabControl(m_App, parentGroup, atts, metadata, m_IconsConv, ref numberOfUsedIds);
    }
}