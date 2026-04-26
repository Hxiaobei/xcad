//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using SolidWorks.Interop.sldworks;
using XCad.Structures;
using XCad.Sw.Documents;

namespace XCad.Sw.Geometry {
    public interface ISwEntity : ISwSelObject, ISupportsResilience<ISwEntity> {
        IEntity Entity { get; }
        ISwComponent Component { get; }
        ISwEntityRepository AdjacentEntities { get; }
        ISwBody Body { get; }
        Vec3d FindClosestPoint(Vec3d point);
    }

    internal abstract class SwEntity : SwSelObject, ISwEntity {
        ISwObject ISupportsResilience.CreateResilient() => CreateResilient();

        public virtual IEntity Entity { get; }

        public override object Dispatch => Entity;

        public abstract ISwBody Body { get; }

        public abstract ISwEntityRepository AdjacentEntities { get; }

        public virtual ISwComponent Component {
            get {
                var comp = Entity.IGetComponent2();
                return comp == null ? null : OwnerDocument.CreateObjectFromDispatch<ISwComponent>(comp);

            }
        }

        public override bool IsAlive => this.CheckIsAlive(() => { var test = Entity.IsSafe; });

        public bool IsResilient => Entity.IsSafe;

        internal SwEntity(IEntity entity, SwDocument doc, SwApplication app) : base(entity, doc, app) {
            Entity = entity;
        }

        internal override void Select(bool append, ISelectData selData) {
            if(!Entity.Select4(append, (SelectData)selData)) {
                throw new Exception("Failed to select entity");
            }
        }

        public abstract Vec3d FindClosestPoint(Vec3d point);

        public ISwEntity CreateResilient() {
            var safeEnt = Entity.GetSafeEntity() ?? throw new NullReferenceException("Failed to get safe entity");
            return OwnerApplication.CreateObjectFromDispatch<SwEntity>(safeEnt, OwnerDocument);
        }
    }
}