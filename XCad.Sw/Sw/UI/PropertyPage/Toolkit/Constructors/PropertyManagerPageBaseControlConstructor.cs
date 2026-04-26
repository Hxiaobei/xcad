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

namespace XCad.Sw.UI.PropertyPage.Toolkit.Constructors {
    internal interface IPropertyManagerPageElementConstructor : IPageElementConstructor {
        Type ControlType { get; }
        void PostProcessControls(IEnumerable<IPropertyManagerPageControlEx> ctrls);
    }

    internal abstract class PropertyManagerPageBaseControlConstructor<TControl, TControlSw>
            : ControlConstructor<TControl, PropertyManagerPageGroupBase, PropertyManagerPagePage>,
            IPropertyManagerPageElementConstructor
            where TControl : IPropertyManagerPageControlEx
            where TControlSw : class {
        public Type ControlType => typeof(TControl);
        protected readonly IIconsCreator m_IconConv;
        protected readonly SwApplication m_App;

        protected PropertyManagerPageBaseControlConstructor(SwApplication app, IIconsCreator iconsConv) {
            m_App = app;
            m_IconConv = iconsConv;
        }

        public virtual void PostProcessControls(IEnumerable<IPropertyManagerPageControlEx> ctrls) {
        }
    }
}