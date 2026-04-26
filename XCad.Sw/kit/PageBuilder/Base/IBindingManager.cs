//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Collections.Generic;
using XCad.Sw;
using XCad.UI.PropertyPage.Base;

namespace XCad.kit.PageBuilder.Base {
    public interface IBindingManager {
        IEnumerable<IBinding> Bindings { get; }
        IDependencyManager Dependency { get; }
        IMetadata[] Metadata { get; }

        void Load(ISwApplication app, IEnumerable<IBinding> bindings, IRawDependencyGroup dependencies, IMetadata[] metadata);
    }
}