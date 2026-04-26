using System;
using System.Runtime.InteropServices;
using SolidWorks.Interop.sldworks;
using XCad.Structures;

namespace XCad.Sw.Manipulator {
    [ComVisible(true)]
    public class Triad : ManipulatorBase<ITriadManipulator> {
        public Triad(IModelDoc2 model) : base(model) { }

        public Vec3d Origin {
            get => Mp.Origin.ToXa();
            set => Mp.Origin = value.ToSwPt();
        }

        public Vec3d XAxis {
            get => Mp.XAxis.ToXa();
            set => Mp.XAxis = value.ToSwVec();
        }

        public Vec3d YAxis {
            get => Mp.YAxis.ToXa();
            set => Mp.YAxis = value.ToSwVec();
        }

        public Vec3d ZAxis {
            get => Mp.ZAxis.ToXa();
            set => Mp.ZAxis = value.ToSwVec();
        }

        public Vec3d PreviousDragPoint => Mp.PreviousDragPoint.ToXa();

        public Transform Transform { get; private set; }

        public void UpdatePosition() => Mp.UpdatePosition();

        private Vec3d m_OldPt;

        private double m_Value;

        public override void OnHandleSelected(object _p, int handleIndex) {
            m_OldPt = PreviousDragPoint;
        }

        public override bool OnDoubleValueChanged(object _p, int handleIndex, ref double value) {
            m_Value = value;
            return true;
        }

        public event Action ResetRequested;

        public override void OnEndNoDrag(object _p, int handleIndex) {
            ResetRequested?.Invoke();
        }

        public override void OnEndDrag(object _p, int handleIndex) {
            var newPt = PreviousDragPoint;
            var delta = newPt - m_OldPt;
            var vecOld = m_OldPt - Origin;
            var vecNew = newPt - Origin;
            double dist;
            double angle;
            Vec3d move;
            switch(handleIndex) {
                case 1: dist = delta.Dot(XAxis); break;
                case 2: dist = delta.Dot(YAxis); break;
                case 3: dist = delta.Dot(ZAxis); break;
                case 4: move = delta.ProjectOntoPlane(ZAxis); break;
                case 5: move = delta.ProjectOntoPlane(XAxis); break;
                case 6: move = delta.ProjectOntoPlane(YAxis); break;
                case 7: angle = vecOld.CalculateAngleOnPlane(vecNew, ZAxis); break;
                case 8: angle = vecOld.CalculateAngleOnPlane(vecNew, XAxis); break;
                case 9: angle = vecOld.CalculateAngleOnPlane(vecNew, YAxis); break;
            }
        }

        public override void OnUpdateDrag(object _p, int handleIndex, object newPosMathPt) {
            var newPt = ((IMathPoint)newPosMathPt).ToXa();
            var oldPt = m_OldPt; // Wait, previous drag point? Or handle select point?
            // In original code it was using PreviousDragPoint which updates.

            UpdateTriad(handleIndex, oldPt, newPt);
        }

        public void UpdateTriad(int index, double value) {
            var o0 = Origin;
            var x0 = XAxis;
            var y0 = YAxis;
            var z0 = ZAxis;

            var o1 = o0;
            var x1 = x0;
            var y1 = y0;
            var z1 = z0;

            var angleRad = index >= 7 ? value * (Math.PI / 180.0) : 0;

            switch(index) {
                case 1: o1 = o0.Move(x0, value * 1e-3); break;
                case 2: o1 = o0.Move(y0, value * 1e-3); break;
                case 3: o1 = o0.Move(z0, value * 1e-3); break;

                case 7:
                    x1 = x0.RotateAroundAxis(z0, angleRad);
                    y1 = y0.RotateAroundAxis(z0, angleRad);
                    break;
                case 8:
                    y1 = y0.RotateAroundAxis(x0, angleRad);
                    z1 = z0.RotateAroundAxis(x0, angleRad);
                    break;
                case 9:
                    x1 = x0.RotateAroundAxis(y0, angleRad);
                    z1 = z0.RotateAroundAxis(y0, angleRad);
                    break;
            }

            if(index >= 7) {
                XAxis = x1;
                YAxis = y1;
                ZAxis = z1;
            } else {
                Origin = o1;
            }

            Transform = Transform.FromCoordinateSystems(x0, y0, o0, x1, y1, o1);
            UpdatePosition();
        }

        public void UpdateTriad(int index, Vec3d p0, Vec3d p1) {
            var o0 = Origin;
            var x0 = XAxis;
            var y0 = YAxis;
            var z0 = ZAxis;

            var o1 = o0;
            var x1 = x0;
            var y1 = y0;
            var z1 = z0;

            var delta = p1 - p0;
            var vecOld = p0 - Origin;
            var vecNew = p1 - Origin;

            switch(index) {
                case 1:
                    o1 = o0.Move(x0, delta.Dot(x0));
                    break;
                case 2:
                    o1 = o0.Move(y0, delta.Dot(y0));
                    break;
                case 3:
                    o1 = o0.Move(z0, delta.Dot(z0));
                    break;
                case 4:
                    o1 = o0 + delta.ProjectOntoPlane(z0);
                    break;
                case 5:
                    o1 = o0 + delta.ProjectOntoPlane(x0);
                    break;
                case 6:
                    o1 = o0 + delta.ProjectOntoPlane(y0);
                    break;
                case 7:
                    double angleZ = vecOld.CalculateAngleOnPlane(vecNew, z0);
                    x1 = x0.RotateAroundAxis(z0, angleZ);
                    y1 = y0.RotateAroundAxis(z0, angleZ);
                    break;
                case 8:
                    double angleX = vecOld.CalculateAngleOnPlane(vecNew, x0);
                    y1 = y0.RotateAroundAxis(x0, angleX);
                    z1 = z0.RotateAroundAxis(x0, angleX);
                    break;
                case 9:
                    double angleY = vecOld.CalculateAngleOnPlane(vecNew, y0);
                    x1 = x0.RotateAroundAxis(y0, angleY);
                    z1 = z0.RotateAroundAxis(y0, angleY);
                    break;
            }

            if(index >= 7) {
                XAxis = x1;
                YAxis = y1;
                ZAxis = z1;
            } else {
                Origin = o1;
            }

            Transform = Transform.FromCoordinateSystems(x0, y0, o0, x1, y1, o1);
            UpdatePosition();
        }
    }
}
