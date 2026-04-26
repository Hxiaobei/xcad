//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.IO;
using System.Threading;
using SolidWorks.Interop.sldworks;
using XCad.Structures;
using XCad.Sw.Documents;
using XCad.Sw.Exceptions;
using XCad.Sw.Features;
using XCad.UI;

namespace XCad.Sw.Sketch {
    public interface ISwSketchPicture : ISwSketchEntity, ISwFeature {
        /// <summary>
        /// Pointer to the sketch picture
        /// </summary>
        ISketchPicture SketchPicture { get; }
    }

    internal class SwSketchPicture : SwFeature, ISwSketchPicture {
        public ISketchPicture SketchPicture { get; private set; }


        private SwSketchBase m_OwnerSketch;

        internal SwSketchPicture(IFeature feat, SwDocument doc, SwApplication app, bool created) : base(feat, doc, app, created) {
            if(feat != null) {
                SketchPicture = feat.GetSpecificFeature2() as ISketchPicture;
            }
        }

        internal SwSketchPicture(ISketchPicture skPict, SwDocument doc, SwApplication app, bool created) : base(skPict.GetFeature(), doc, app, created) {
            SketchPicture = skPict;
        }

        internal SwSketchPicture(SwSketchBase ownerSketch, SwDocument doc, SwApplication app) : base(null, doc, app, false) {
            m_OwnerSketch = ownerSketch;
        }

        public override object Dispatch => SketchPicture;

        public IXImage Image {
            get {
                if(IsCommitted) {
                    throw new NotSupportedException();
                } else {
                    return m_Creator.CachedProperties.Get<IXImage>();
                }
            }
            set {
                if(IsCommitted) {
                    throw new CommitedElementReadOnlyParameterException();
                } else {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        public Rect2d Boundary {
            get {
                if(IsCommitted) {
                    double width = -1;
                    double height = -1;
                    double x = -1;
                    double y = -1;

                    SketchPicture.GetSize(ref width, ref height);
                    SketchPicture.GetOrigin(ref x, ref y);
                    var angle = SketchPicture.Angle;

                    var transform = Transform.RotationZ(angle);

                    var dirX = new Vec3d(1, 0, 0).MulVector(transform);
                    var dirY = new Vec3d(0, 1, 0).MulVector(transform);

                    return new Rect2d(width, height, new Vec3d(x + width / 2, y + height / 2, 0), dirX, dirY);
                } else {
                    return m_Creator.CachedProperties.Get<Rect2d>();
                }
            }
            set {
                if(IsCommitted) {
                    throw new CommitedElementReadOnlyParameterException();
                } else {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        /// <remarks>
        /// Sketch picture in SOLIDWORKS cannot be added into the block
        /// </remarks>
        public ISwSketchBlockInstance OwnerBlock => null;

        public IXSketchBase OwnerSketch => null;

        protected override IFeature InsertFeature(CancellationToken cancellationToken) {
            if(Image == null) {
                throw new Exception("Image is not specified");
            }

            if(Boundary == null) {
                throw new Exception("Boundary of the image is not specified");
            }

            using(var editor = m_OwnerSketch?.Edit()) {
                var tempFileName = "";

                try {
                    tempFileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".png");

                    File.WriteAllBytes(tempFileName, Image.Buffer);

                    var pict = OwnerDocument.Model.SketchManager.InsertSketchPicture(tempFileName);

                    if(pict != null) {
                        var orig = new Vec3d(Boundary.CenterPoint.X - Boundary.Width / 2, Boundary.CenterPoint.Y - Boundary.Height / 2, 0);

                        pict.SetOrigin(orig.X, orig.Y);
                        pict.SetSize(Boundary.Width, Boundary.Height, false);

                        var angle = Boundary.AxisX.GetAngle(new Vec3d(1, 0, 0));

                        //picture PMPage stays open after inserting the picture
                        const int swCommands_PmOK = -2;
                        OwnerApplication.Sw.RunCommand(swCommands_PmOK, "");

                        SketchPicture = pict;

                        return pict.GetFeature();
                    } else {
                        throw new Exception("Failed to insert picture");
                    }
                } finally {
                    if(File.Exists(tempFileName)) {
                        try {
                            File.Delete(tempFileName);
                        } catch {
                        }
                    }
                }
            }
        }
    }
}
