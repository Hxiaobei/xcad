//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using SolidWorks.Interop.sldworks;
using XCad.Structures;
using XCad.Sw.Documents;
using XCad.Sw.Extensions;
using XCad.Sw.Features;

namespace XCad.Sw.Sketch {
    public interface ISwSketchBlockDefinition : ISwFeature {
        ISketchBlockDefinition SketchBlockDefinition { get; }

        /// <summary>
        /// Insertion point of the sketch block definition
        /// </summary>
        Vec3d InsertionPoint { get; }

        /// <summary>
        /// All instances of this sketch block defintion
        /// </summary>
        IEnumerable<ISwSketchBlockInstance> Instances { get; }

        /// <summary>
        /// Entities of this sketch block definition
        /// </summary>
        ISwSketchEntityCollection Entities { get; }
    }

    internal class SwSketchBlockDefinition : SwFeature, ISwSketchBlockDefinition {
        public ISketchBlockDefinition SketchBlockDefinition { get; }

        public IEnumerable<ISwSketchBlockInstance> Instances {
            get {
                var instances = SketchBlockDefinition.GetInstances().ToSwArray<ISketchBlockInstance>();

                foreach(var inst in instances) {
                    yield return OwnerDocument.CreateObjectFromDispatch<ISwSketchBlockInstance>(inst);
                }
            }
        }

        public ISwSketchEntityCollection Entities { get; }

        public override bool IsAlive => this.CheckIsAlive(() => { var test = SketchBlockDefinition.LinkToFile; });

        public Vec3d InsertionPoint => SketchBlockDefinition.InsertionPoint.ToXa();

        internal SwSketchBlockDefinition(IFeature feat, SwDocument doc, SwApplication app, bool created) : base(GetSketchBlockDefinitionFeature(doc.Model, feat.Name), doc, app, created) {
            SketchBlockDefinition = (ISketchBlockDefinition)feat.GetSpecificFeature2();

            Entities = new SwSketchEntityCollection(doc.CreateObjectFromDispatch<SwSketchBase>(SketchBlockDefinition.GetSketch()), doc, app);
        }

        //Note: retrieving the pointer to the feature from the feature tree for the consistency as IFeature retrieved from ISketchBlockDefinition has a different pointer to IFeature in the tree
        private static IFeature GetSketchBlockDefinitionFeature(IModelDoc2 model, string name) {
            switch(model) {
                case IPartDoc part:
                    return (IFeature)part.FeatureByName(name);

                case IAssemblyDoc assm:
                    return (IFeature)assm.FeatureByName(name);

                case IDrawingDoc drw:
                    return (IFeature)drw.FeatureByName(name);

                default:
                    throw new NotSupportedException();
            }
        }
    }
}
