//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using SolidWorks.Interop.sldworks;
using XCad.kit.PageBuilder.Attributes;
using XCad.kit.PageBuilder.Base;
using XCad.kit.PageBuilder.Core;
using XCad.kit.Services;
using XCad.Sw.UI.PropertyPage.Toolkit.Controls;
using XCad.UI.PropertyPage.Attributes;
using XCad.UI.PropertyPage.Base;

namespace XCad.Sw.UI.PropertyPage.Toolkit.Constructors {
    internal abstract class PropertyManagerPageComboBoxControlConstructorBase<TVal>
        : PropertyManagerPageBaseControlConstructor<PropertyManagerPageComboBoxControl<TVal>, IPropertyManagerPageCombobox> {
        public PropertyManagerPageComboBoxControlConstructorBase(SwApplication app, IIconsCreator iconsConv)
            : base(app, iconsConv) {
        }

        protected override PropertyManagerPageComboBoxControl<TVal> Create(IGroup parentGroup, IAttributeSet atts, IMetadata[] metadata, ref int numberOfUsedIds)
            => new PropertyManagerPageComboBoxControl<TVal>(m_App, parentGroup, m_IconConv, atts, metadata, ref numberOfUsedIds);
    }

    [DefaultType(typeof(SpecialTypes.EnumType))]
    internal class PropertyManagerPageEnumComboBoxControlConstructor
        : PropertyManagerPageComboBoxControlConstructorBase<Enum> {
        public PropertyManagerPageEnumComboBoxControlConstructor(SwApplication app, IIconsCreator iconsConv)
            : base(app, iconsConv) {
        }
    }

    internal class PropertyManagerPageCustomItemsComboBoxControlConstructor
        : PropertyManagerPageComboBoxControlConstructorBase<object>, ICustomItemsComboBoxControlConstructor {
        public PropertyManagerPageCustomItemsComboBoxControlConstructor(SwApplication app, IIconsCreator iconsConv)
            : base(app, iconsConv) {
        }
    }
}