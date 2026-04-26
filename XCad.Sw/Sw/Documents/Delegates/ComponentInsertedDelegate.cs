namespace XCad.Sw.Documents.Delegates {
    /// <summary>
    /// Delegate of <see cref="ISwAssembly.ComponentInserted"/> notification
    /// </summary>
    /// <param name="assembly">Assembly where component is inserted</param>
    /// <param name="component">Component inserted into the assembly</param>
    public delegate void ComponentInsertedDelegate(ISwAssembly assembly, ISwComponent component);
}
