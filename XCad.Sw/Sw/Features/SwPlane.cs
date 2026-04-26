//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Threading;
using SolidWorks.Interop.sldworks;
using XCad.Structures;
using XCad.Sw.Documents;
using XCad.Sw.Exceptions;
using XCad.Sw.Geometry;

namespace XCad.Sw.Features {
    public interface ISwPlane : ISwFeature, ISwPlanarRegion {
        /// <summary>
        /// Pointer to the referenced plane feature
        /// </summary>
        IRefPlane RefPlane { get; }
        new Plane Plane { get; set; }
    }

    internal class SwPlane : SwFeature, ISwPlane {
        internal const string TypeName = "RefPlane";

        public IRefPlane RefPlane { get; private set; }

        internal SwPlane(IFeature feat, SwDocument doc, SwApplication app, bool created) : base(feat, doc, app, created) {
            if(feat != null) {
                RefPlane = feat.GetSpecificFeature2() as IRefPlane;
            }
        }

        public override object Dispatch => RefPlane;

        public Plane Plane {
            get {
                if(IsCommitted) {

                    var transform = RefPlane.Transform;

                    var x = Vec3d.UnitX.ToSwVec().IMultiplyTransform(transform);
                    var z = Vec3d.UnitZ.ToSwVec().IMultiplyTransform(transform);
                    var origin = Vec3d.Zero.ToSwVec().IMultiplyTransform(transform);

                    return new Plane(origin.ToXa(), z.ToXa(), x.ToXa());
                } else {
                    return m_Creator.CachedProperties.Get<Plane>();
                }
            }
            set {
                if(!IsCommitted) {
                    m_Creator.CachedProperties.Set(value);
                } else {
                    throw new CommitedElementReadOnlyParameterException();
                }
            }
        }

        public override bool IsUserFeature {
            get {
                const int MAX_STANDARD_PLANES_COUNT = 3;

                var nextFeat = Feature;

                for(int i = 0; i < MAX_STANDARD_PLANES_COUNT; i++) {
                    nextFeat = nextFeat.IGetNextFeature();

                    if(nextFeat == null) {
                        break;
                    }

                    var nextFeatTypeName = nextFeat.GetTypeName2();

                    if(nextFeatTypeName == SwOrigin.TypeName)//this feature is standard plane
                    {
                        return false;
                    } else if(nextFeatTypeName != TypeName) //this feature is not a standard plane
                      {
                        break;
                    }
                }

                return true;
            }
        }

        public ISwLoop OuterLoop { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ISwLoop[] InnerLoops { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public ISwPlanarSheetBody PlanarSheetBody => throw new NotImplementedException();

        protected override IFeature InsertFeature(CancellationToken cancellationToken) {
            if(Plane == null) {
                throw new NullReferenceException("Plane is not specified");
            }

            var pt1 = Plane.Point;
            var pt2 = Plane.Point.Move(Plane.Direction, 0.1);
            var pt3 = Plane.Point.Move(Plane.Reference, 0.1);

            RefPlane = (IRefPlane)OwnerDocument.Model.CreatePlaneFixed2(pt1.ToArray(), pt2.ToArray(), pt3.ToArray(), false);

            var feat = (IFeature)RefPlane;

            return feat;
        }
    }
}
