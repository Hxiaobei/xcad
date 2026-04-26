//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using XCad.UI.PropertyPage.Base;
using XCad.UI.PropertyPage.Delegates;

namespace XCad.kit.PageBuilder.Base {
    public delegate IControl CreateBindingControlDelegate(Type dataType, IAttributeSet atts,
        IGroup parent, IMetadata[] metadata, out int numberOfUsedIds);

    public delegate IPage CreateBindingPageDelegate(IAttributeSet atts);

    public interface IDataModelBinder {
        void Bind<TDataModel>(CreateBindingPageDelegate pageCreator,
            CreateBindingControlDelegate ctrlCreator, CreateDynamicControlsDelegate dynCtrlDescCreator, IContextProvider modelSetter,
            out IEnumerable<IBinding> bindings, out IRawDependencyGroup dependencies, out IMetadata[] metadata);
    }
}