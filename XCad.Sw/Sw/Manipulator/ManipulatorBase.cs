using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace XCad.Sw.Manipulator {
    public abstract class ManipulatorBase<T> : Manipulator, IDisposable where T : class {
        private readonly IManipulator _manipulator;
        private readonly T _mp;
        public T Mp => _mp;

        private bool m_IsDisposed;

        private static readonly Dictionary<Type, swManipulatorType_e> TypeMap = new Dictionary<Type, swManipulatorType_e>
        {
            { typeof(ITriadManipulator), swManipulatorType_e.swTriadManipulator },
            { typeof(IDragArrowManipulator), swManipulatorType_e.swDragArrowManipulator },
            { typeof(IPlaneManipulator), swManipulatorType_e.swPlaneManipulator }
        };

        protected ManipulatorBase(IModelDoc2 model) {
            if(!TypeMap.TryGetValue(typeof(T), out var swMpType))
                throw new NotImplementedException($"Unsupported manipulator type: {typeof(T).Name}");

            _manipulator = model.ModelViewManager.CreateManipulator((int)swMpType, this) ?? throw new InvalidOperationException("Failed to create manipulator");
            _mp = _manipulator.GetSpecificManipulator() as T ?? throw new InvalidCastException($"Expected {typeof(T).Name}");
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

        public void Show(IModelDoc2 model) => _manipulator.Show(model);

        public virtual void Dispose() {
            if(m_IsDisposed) return;

            try {
                _manipulator?.Remove();
            } catch { }

            try {
                if(_manipulator != null && Marshal.IsComObject(_manipulator))
                    Marshal.ReleaseComObject(_manipulator);
            } catch { }

            try {
                if(_mp != null && Marshal.IsComObject(_mp))
                    Marshal.ReleaseComObject(_mp);
            } catch { }

            m_IsDisposed = true;
            GC.SuppressFinalize(this);
        }

        ~ManipulatorBase() {
            if(!m_IsDisposed) {
                Dispose();
            }
        }
    }
}
