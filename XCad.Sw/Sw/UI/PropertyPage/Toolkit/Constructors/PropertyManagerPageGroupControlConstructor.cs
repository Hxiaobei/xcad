//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using XCad.kit.PageBuilder.Attributes;
using XCad.kit.PageBuilder.Base;
using XCad.kit.PageBuilder.Constructors;
using XCad.kit.PageBuilder.Core;
using XCad.kit.Services;
using XCad.Sw.UI.PropertyPage.Toolkit.Controls;
using XCad.UI.PropertyPage.Base;

namespace XCad.Sw.UI.PropertyPage.Toolkit.Constructors {
    [DefaultType(typeof(SpecialTypes.ComplexType))]
    internal class PropertyManagerPageGroupControlConstructor
        : GroupConstructor<PropertyManagerPageGroupControl, PropertyManagerPagePage>,
        IPropertyManagerPageElementConstructor {
        public Type ControlType => typeof(PropertyManagerPageGroupControl);

        private readonly SwApplication m_App;
        private readonly IIconsCreator m_IconsConv;

        internal PropertyManagerPageGroupControlConstructor(SwApplication app, IIconsCreator iconsConv) {
            m_App = app;
            m_IconsConv = iconsConv;
        }

        public void PostProcessControls(IEnumerable<IPropertyManagerPageControlEx> ctrls) {
            //TODO: not used
        }

        protected override PropertyManagerPageGroupControl Create(IGroup parentGroup, IAttributeSet atts, IMetadata[] metadata, ref int numberOfUsedIds)
            => new PropertyManagerPageGroupControl(m_App, parentGroup, atts, metadata, m_IconsConv, ref numberOfUsedIds);
    }
}