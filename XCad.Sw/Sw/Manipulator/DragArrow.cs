using System.Runtime.InteropServices;
using SolidWorks.Interop.sldworks;
using XCad.Structures;

namespace XCad.Sw.Manipulator {
    [ComVisible(true)]
    public class DragArrow : ManipulatorBase<IDragArrowManipulator> {
        public DragArrow(IModelDoc2 model, Vec3d direction) : base(model) {
            Selectable = true;
            Mp.AllowFlip = true;
            Mp.ShowRuler = false;

            Mp.FixedLength = true;
            Mp.Length = 1 * 1e-3;

            Mp.Direction = direction.ToSwVec();
            Show(model);
        }

        public void Update() => Mp.Update();

        public Vec3d Origin {
            get => Mp.Origin.ToXa();
            set => Mp.Origin = value.ToSwPt();
        }

        public Vec3d Direction => Mp.Direction.ToXa();
    }
}
