using System.Linq;
using XCad.Sw;
using XCad.UI.PropertyPage.Base;
using XCad.UI.PropertyPage.Structures;

namespace XCad.UI.PropertyPage.Services {
    internal class CustomItemsAttributeDependencyHandler : IDependencyHandler {
        private readonly ICustomItemsProvider m_ItemsProvider;
        private readonly string m_DisplayMemberMemberPath;

        internal CustomItemsAttributeDependencyHandler(ICustomItemsProvider itemsProvider, string displayMemberMemberPath) {
            m_ItemsProvider = itemsProvider;
            m_DisplayMemberMemberPath = displayMemberMemberPath;
        }

        public void UpdateState(ISwApplication app, IControl source, IControl[] dependencies) {
            var itemsCtrl = (IItemsControl)source;

            itemsCtrl.Items = m_ItemsProvider.ProvideItems(app, dependencies)
                ?.Select(i => new ItemsControlItem(i, m_DisplayMemberMemberPath)).ToArray();
        }
    }
}
