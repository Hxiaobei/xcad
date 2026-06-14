using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace XCad.Sw.Manipulator;

/// <summary>
/// 如需创建创建操纵器，必须调用Show方法才能显示它。
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class ManipulatorBase<T> : Manipulator, IDisposable where T : class {
    private readonly IManipulator _manipulator;
    private readonly T _mp;
    private readonly IModelDoc2 _model;
    public T Mp => _mp;

    private bool _isDisposed;

    private static readonly Dictionary<Type, swManipulatorType_e> TypeMap = new() {
            { typeof(ITriadManipulator), swManipulatorType_e.swTriadManipulator },
            { typeof(IDragArrowManipulator), swManipulatorType_e.swDragArrowManipulator },
            { typeof(IPlaneManipulator), swManipulatorType_e.swPlaneManipulator }
        };

    protected ManipulatorBase(IModelDoc2 model) {
        if(!TypeMap.TryGetValue(typeof(T), out var swMpType))
            throw new NotImplementedException($"Unsupported manipulator type: {typeof(T).Name}");

        _manipulator = model.ModelViewManager.CreateManipulator((int)swMpType, this) ?? throw new InvalidOperationException("Failed to create manipulator");
        _mp = _manipulator.GetSpecificManipulator() as T ?? throw new InvalidCastException($"Expected {typeof(T).Name}");
        _model = model;
    }

    public virtual string Name {
        get => _manipulator.Name;
        set => _manipulator.Name = value;
    }

    public virtual bool Visible {
        get => _manipulator.Visible;
        set => _manipulator.Visible = value;
    }

    public bool Selectable {
        get => _manipulator.Selectable;
        set => _manipulator.Selectable = value;
    }

    public bool Select(bool append, SelectData data) => _manipulator.Select(append, data);

    /// <summary>
    /// 创建操纵器后，必须调用此方法才能显示它。
    /// </summary>
    public void Show() => _manipulator.Show(_model);

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing) {
        if(_isDisposed) return;

        if(disposing) {
            try {
                _manipulator?.Remove();
            } catch { }

            try {
                if(_mp != null && Marshal.IsComObject(_mp))
                    Marshal.ReleaseComObject(_mp);
            } catch { }

            try {
                if(_manipulator != null && Marshal.IsComObject(_manipulator))
                    Marshal.ReleaseComObject(_manipulator);
            } catch { }
        }

        _isDisposed = true;
    }

    ~ManipulatorBase() {
        Dispose(false);
    }
}
