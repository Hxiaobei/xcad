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
    internal class StorageWriteAvailableEventsHandler : SwModelEventsHandler<DataStoreAvailableDelegate> {
        internal StorageWriteAvailableEventsHandler(SwDocument doc, ISwApplication app) : base(doc, app) {
        }

        protected override void SubscribeAssemblyEvents(AssemblyDoc assm) {
            assm.SaveToStorageStoreNotify += OnWriteToStorageNotify;
        }

        protected override void SubscribeDrawingEvents(DrawingDoc drw) {
            drw.SaveToStorageStoreNotify += OnWriteToStorageNotify;
        }

        protected override void SubscribePartEvents(PartDoc part) {
            part.SaveToStorageStoreNotify += OnWriteToStorageNotify;
        }

        protected override void UnsubscribeAssemblyEvents(AssemblyDoc assm) {
            assm.SaveToStorageStoreNotify -= OnWriteToStorageNotify;
        }

        protected override void UnsubscribeDrawingEvents(DrawingDoc drw) {
            drw.SaveToStorageStoreNotify -= OnWriteToStorageNotify;
        }

        protected override void UnsubscribePartEvents(PartDoc part) {
            part.SaveToStorageStoreNotify -= OnWriteToStorageNotify;
        }

        private int OnWriteToStorageNotify() {
            Delegate?.Invoke(m_Doc);
            return HResult.S_OK;
        }
    }
}
