//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Threading;
using SolidWorks.Interop.sldworks;
using XCad.Structures;
using XCad.Sw.Documents;
using XCad.Sw.Geometry.Wires;

namespace XCad.Sw.Geometry.Curves {
    public interface ISwPoint : ISwObject, IXPoint {
    }

    internal class SwPoint : SwObject, ISwPoint {
        internal SwPoint(object disp, SwDocument doc, SwApplication app) : base(disp, doc, app) {
        }

        public Vec3d Coordinate { get; set; }

        public bool IsCommitted => true;

        public void Commit(CancellationToken cancellationToken) {
        }
    }

    internal class SwMathPoint : SwObject, ISwPoint {
        internal IMathPoint MathPoint { get; }

        internal SwMathPoint(IMathPoint mathPt, SwDocument doc, SwApplication app) : base(mathPt, doc, app) {
            MathPoint = mathPt;
        }

        public bool IsCommitted => true;

        public Vec3d Coordinate {
            get => new Vec3d((double[])MathPoint.ArrayData);
            set => MathPoint.ArrayData = value.ToArray();
        }

        public void Commit(CancellationToken cancellationToken) {
        }
    }
}
