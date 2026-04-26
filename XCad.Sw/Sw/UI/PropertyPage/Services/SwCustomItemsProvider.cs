//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Collections.Generic;
using System.Linq;
using XCad.Sw.Extensions;
using XCad.UI.PropertyPage.Base;
using XCad.UI.PropertyPage.Services;

namespace XCad.Sw.UI.PropertyPage.Services {
    public abstract class SwCustomItemsProvider<TItem> : ICustomItemsProvider {
        IEnumerable<object> ICustomItemsProvider.ProvideItems(ISwApplication app, IControl[] dependencies)
            => ProvideItems(app, dependencies).ToSwArray<object>();

        public virtual IEnumerable<TItem> ProvideItems(ISwApplication app, IControl[] dependencies)
            => ProvideItems(app);

        public virtual IEnumerable<TItem> ProvideItems(ISwApplication app)
            => Enumerable.Empty<TItem>();
    }
}
