//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using SolidWorks.Interop.sldworks;

namespace XCad.Sw.Documents.Services {
    public abstract class SwDocumentHandler : IDocumentHandler {
        protected const int S_OK = 0;
        protected const int S_FAIL = 1;

        protected ISldWorks Application { get; private set; }
        protected IModelDoc2 Model { get; private set; }

        protected ISwDocument Document { get; private set; }

        public void Init(ISwApplication app, ISwDocument model) {
            Document = model;

            Application = app.Sw;

            OnInit(app, Document);

            Model = Document.Model;

            switch(Model) {
                case PartDoc part:
                    AttachPartEvents(part);
                    break;

                case AssemblyDoc assm:
                    AttachAssemblyEvents(assm);
                    break;

                case DrawingDoc drw:
                    AttachDrawingEvents(drw);
                    break;

                default:
                    throw new NotSupportedException("Not a SOLIDWORKS document");
            }
        }

        protected virtual void OnInit(ISwApplication app, ISwDocument doc) {
        }

        protected virtual void AttachPartEvents(PartDoc part) {
        }

        protected virtual void AttachAssemblyEvents(AssemblyDoc assm) {
        }

        protected virtual void AttachDrawingEvents(DrawingDoc drw) {
        }

        protected virtual void DetachPartEvents(PartDoc part) {
        }

        protected virtual void DetachAssemblyEvents(AssemblyDoc assm) {
        }

        protected virtual void DetachDrawingEvents(DrawingDoc drw) {
        }

        public void Dispose() {
            switch(Model) {
                case PartDoc part:
                    DetachPartEvents(part);
                    break;

                case AssemblyDoc assm:
                    DetachAssemblyEvents(assm);
                    break;

                case DrawingDoc drw:
                    DetachDrawingEvents(drw);
                    break;

                default:
                    throw new NotSupportedException("Not a SOLIDWORKS document");
            }

            Dispose(true);
        }

        protected virtual void Dispose(bool disposing) {
        }
    }
}
