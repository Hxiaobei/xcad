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
using SolidWorks.Interop.swconst;
using XCad.Structures;
using XCad.Sw.Documents;
using XCad.Sw.Extensions;
using XCad.Sw.Features;

namespace XCad.Sw.Sketch {
    public interface ISwSketchBlockInstance : ISwFeature, ISwSketchEntity {
        ISketchBlockInstance SketchBlockInstance { get; }

        /// <summary>
        /// Definition of this sketch block instance
        /// </summary>
        ISwSketchBlockDefinition Definition { get; }

        /// <summary>
        /// Transformation of this sketch block instance regarding its defintion
        /// </summary>
        Transform Transform { get; }

        /// <summary>
        /// Entities of this sketch block definition
        /// </summary>
        ISwSketchEntityCollection Entities { get; }
    }

    internal class SwSketchBlockInstance : SwFeature, ISwSketchBlockInstance {
        public ISketchBlockInstance SketchBlockInstance { get; }
        public ISwSketchBlockDefinition Definition => OwnerDocument.CreateObjectFromDispatch<ISwSketchBlockDefinition>(SketchBlockInstance.Definition);
        public IXSketchBase OwnerSketch => OwnerDocument.CreateObjectFromDispatch<ISwSketchBase>(SketchBlockInstance.GetSketch());

        public ISwSketchBlockInstance OwnerBlock {
            get {
                if(AssignedOwnerBlock != null) {
                    return AssignedOwnerBlock;
                } else {
                    foreach(var node in this.IterateAllSketchBlockInstanceNodes()) {
                        var feat = (IFeature)node.Object;
                        var block = (ISketchBlockInstance)feat.GetSpecificFeature2();

                        if(OwnerApplication.Sw.IsSame(block, SketchBlockInstance) == (int)swObjectEquality.swObjectSame) {
                            var parentNode = node.GetParent();

                            if(parentNode.ObjectType == (int)swTreeControlItemType_e.swFeatureManagerItem_Feature) {
                                var parentFeat = (IFeature)parentNode.Object;

                                if(parentFeat.GetTypeName2() == "SketchBlockInst") {
                                    return OwnerDocument.CreateObjectFromDispatch<ISwSketchBlockInstance>(parentFeat);
                                } else {
                                    return null;
                                }
                            }
                        }
                    }

                    throw new Exception("Sketch block instance is not found in the tree. This may indicate that tree is hidden or not loaded");
                }
            }
        }

        internal SwSketchBlockInstance AssignedOwnerBlock { get; set; }

        public Transform Transform => SketchBlockInstance.BlockToSketchTransform.ToXa();

        public override bool IsAlive => this.CheckIsAlive(() => {
            var test = SketchBlockInstance.Name;

            //NOTE: the deleted block may still produce a valid pointer and all the methods can be executed successfully, checking if the definition still contains this block
            var instances = SketchBlockInstance.Definition.GetInstances().ToSwArray<ISketchBlockInstance>();

            if(instances?.Any(i => OwnerApplication.Sw.IsSame(i, SketchBlockInstance) == (int)swObjectEquality.swObjectSame) != true) {
                throw new Exception();
            }
        });

        public ISwSketchEntityCollection Entities { get; }

        internal SwSketchBlockInstance(IFeature feat, SwDocument doc, SwApplication app, bool created) : base(feat, doc, app, created) {
            SketchBlockInstance = (ISketchBlockInstance)feat.GetSpecificFeature2();
            Entities = new SwSketchBlockInstanceEntityCollection(this, doc.CreateObjectFromDispatch<SwSketchBase>(SketchBlockInstance.Definition.GetSketch()), doc, app);
        }

        public override bool Equals(ISwObject other) {
            if(base.Equals(other)) {
                //NOTE: sketch block instance pointers are from the definition and will be equal from different sketch block instances
                if(AssignedOwnerBlock != null && (other as SwSketchBlockInstance)?.AssignedOwnerBlock != null) {
                    return AssignedOwnerBlock.Equals(((SwSketchBlockInstance)other).AssignedOwnerBlock);
                } else {
                    return true;
                }
            } else {
                return false;
            }
        }
    }

    internal class SwSketchBlockInstanceEntityCollection : SwSketchEntityCollection {
        private readonly SwSketchBlockInstance m_SketchBlockInst;

        internal SwSketchBlockInstanceEntityCollection(SwSketchBlockInstance skBlockInst, SwSketchBase sketch, SwDocument doc, SwApplication app)
            : base(sketch, doc, app) {
            m_SketchBlockInst = skBlockInst;
        }

        protected override IEnumerable<ISwSketchEntity> IterateEntities() {
            foreach(var ent in base.IterateEntities()) {
                switch(ent) {
                    case SwSketchEntity skEnt:
                        skEnt.AssignedOwnerBlock = m_SketchBlockInst;
                        break;

                    case SwSketchBlockInstance skBlockInst:
                        skBlockInst.AssignedOwnerBlock = m_SketchBlockInst;
                        break;

                    default:
                        throw new NotSupportedException($"{ent?.GetType()} sketch block entity is not supported");
                }

                yield return ent;
            }
        }
    }
}
