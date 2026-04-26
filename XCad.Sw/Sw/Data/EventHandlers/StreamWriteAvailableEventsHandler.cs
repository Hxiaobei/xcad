//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using XCad.Sw.Documents;
using XCad.Sw.Documents.Delegates;
using XCad.Sw.Utils;

namespace XCad.Sw.Data.EventHandlers {
    internal class StreamWriteAvailableEventsHandler : SwModelEventsHandler<DataStoreAvailableDelegate> {
        internal StreamWriteAvailableEventsHandler(SwDocument doc, ISwApplication app) : base(doc, app) {
        }

        protected override void SubscribeAssemblyEvents(AssemblyDoc assm) {
            assm.SaveToStorageNotify += OnWriteToStreamNotify;
        }

        protected override void SubscribeDrawingEvents(DrawingDoc drw) {
            drw.SaveToStorageNotify += OnWriteToStreamNotify;
        }

        protected override void SubscribePartEvents(PartDoc part) {
            part.SaveToStorageNotify += OnWriteToStreamNotify;
        }

        protected override void UnsubscribeAssemblyEvents(AssemblyDoc assm) {
            assm.SaveToStorageNotify -= OnWriteToStreamNotify;
        }

        protected override void UnsubscribeDrawingEvents(DrawingDoc drw) {
            drw.SaveToStorageNotify -= OnWriteToStreamNotify;
        }

        protected override void UnsubscribePartEvents(PartDoc part) {
            part.SaveToStorageNotify -= OnWriteToStreamNotify;
        }

        private int OnWriteToStreamNotify() {
            Delegate?.Invoke(m_Doc);
            return HResult.S_OK;
        }
    }
}
