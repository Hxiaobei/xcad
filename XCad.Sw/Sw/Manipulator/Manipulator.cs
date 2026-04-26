using SolidWorks.Interop.swpublished;

namespace XCad.Sw.Manipulator {
    /// <summary>
    /// Base class for SolidWorks manipulator handlers
    /// </summary>
    public abstract class Manipulator : ISwManipulatorHandler2 {
        public virtual bool OnDelete(object _p) => true;
        public virtual void OnDirectionFlipped(object _p) { }
        public virtual bool OnHandleLmbSelected(object _p) => true;
        public virtual void OnHandleRmbSelected(object _p, int handleIndex) { }
        public virtual void OnHandleSelected(object _p, int handleIndex) { }
        public virtual void OnEndDrag(object _p, int handleIndex) { }
        public virtual void OnEndNoDrag(object _p, int handleIndex) { }
        public virtual bool OnDoubleValueChanged(object _p, int handleIndex, ref double value) => false;
        public virtual bool OnStringValueChanged(object _p, int handleIndex, ref string value) => false;
        public virtual void OnItemSetFocus(object _p, int handleIndex) { }
        public virtual void OnUpdateDrag(object _p, int handleIndex, object newPosMathPt) { }
    }
}
