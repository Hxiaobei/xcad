using XCad.Sw.Documents.Structures;

namespace XCad.Sw.Documents.Delegates {
    /// <summary>
    /// Delegate of <see cref="ISwAssembly.ComponentDeleting"/> notification
    /// </summary>
    /// <param name="assembly">Assembly where component is being deleted</param>
    /// <param name="component">Component being deleted from the assembly</param>
    /// <param name="args">Deleting arguments</param>
    public delegate void ComponentDeletingDelegate(ISwAssembly assembly, ISwComponent component, ItemDeleteArgs args);
}
