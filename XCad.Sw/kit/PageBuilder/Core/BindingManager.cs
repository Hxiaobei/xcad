//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Collections.Generic;
using XCad.kit.PageBuilder.Base;
using XCad.Sw;
using XCad.UI.PropertyPage.Base;

namespace XCad.kit.PageBuilder.Core {
    public class BindingManager : IBindingManager {
        public IEnumerable<IBinding> Bindings { get; set; }
        public IDependencyManager Dependency { get; set; }
        public IMetadata[] Metadata { get; set; }

        public void Load(ISwApplication app, IEnumerable<IBinding> bindings,
            IRawDependencyGroup dependencies, IMetadata[] metadata) {
            Bindings = bindings;
            Dependency = new DependencyManager();
            Metadata = metadata;

            Dependency.Init(app, dependencies);
        }
    }
}