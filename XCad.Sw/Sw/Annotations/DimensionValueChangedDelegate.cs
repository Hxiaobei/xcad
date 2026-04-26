namespace XCad.Sw.Annotations {
    /// <summary>
    /// Delegate of <see cref="ISwDimension.ValueChanged"/> event
    /// </summary>
    /// <param name="dim">Sender</param>
    /// <param name="newVal">New value of the dimension</param>
    public delegate void DimensionValueChangedDelegate(ISwDimension dim, double newVal);
}
