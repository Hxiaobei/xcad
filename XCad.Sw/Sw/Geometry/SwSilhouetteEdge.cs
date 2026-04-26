//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using SolidWorks.Interop.sldworks;
using XCad.Structures;
using XCad.Sw.Documents;
using XCad.Sw.Enums;
using XCad.Sw.Geometry.Curves;
using XCad.Sw.Geometry.Wires;

namespace XCad.Sw.Geometry {
    public interface ISwSilhouetteEdge : ISwEntity, IXSegment {
        ISilhouetteEdge SilhouetteEdge { get; }
    }

    internal class SwSilhouetteEdge : SwEntity, ISwSilhouetteEdge {
        public ISilhouetteEdge SilhouetteEdge { get; }

        //NOTE: ISilhouetteEdge is not an IEntity
        internal SwSilhouetteEdge(ISilhouetteEdge silhouetteEdge, SwDocument doc, SwApplication app) : base(silhouetteEdge as IEntity, doc, app) {
            SilhouetteEdge = silhouetteEdge;
        }

        public override IEntity Entity => throw new NotSupportedException($"{nameof(ISilhouetteEdge)} is not an {nameof(IEntity)}");

        public ISwFace Face => OwnerDocument.CreateObjectFromDispatch<ISwFace>(SilhouetteEdge.GetFace());

        public ISwCurve Definition => OwnerApplication.CreateObjectFromDispatch<SwCurve>(SilhouetteEdge.GetCurve(), OwnerDocument);

        public IXPoint StartPoint => new SwMathPoint(SilhouetteEdge.GetStartPoint(), OwnerDocument, OwnerApplication);

        public IXPoint EndPoint => new SwMathPoint(SilhouetteEdge.GetStartPoint(), OwnerDocument, OwnerApplication);

        public double Length => Definition.Length;

        public override ISwBody Body => Face.Body;

        public override ISwEntityRepository AdjacentEntities => new EmptySwEntityRepository();

        public override object Dispatch => SilhouetteEdge;

        public override ISwComponent Component {
            get {
                var comp = (IComponent2)((IEntity)SilhouetteEdge.GetFace()).GetComponent();

                if(comp != null) {
                    return OwnerDocument.CreateObjectFromDispatch<ISwComponent>(comp);
                } else {
                    return null;
                }
            }
        }

        internal override void Select(bool append, ISelectData selData) {
            if(OwnerApplication.IsVersionNewerOrEqual(SwVersion_e.Sw2014)) {
                SilhouetteEdge.Select2(append, (SelectData)selData);
            } else {
                SilhouetteEdge.Select(append, (SelectData)selData);
            }
        }

        public override Vec3d FindClosestPoint(Vec3d point)
            => new Vec3d(((double[])SilhouetteEdge.GetCurve().GetClosestPointOn(point.X, point.Y, point.Z)).Take(3).ToArray());
    }

    internal class EmptySwEntityRepository : SwEntityRepository {
        protected override IEnumerable<ISwEntity> IterateEntities(bool faces, bool edges, bool vertices, bool silhouetteEdges) {
            yield break;
        }
    }
}