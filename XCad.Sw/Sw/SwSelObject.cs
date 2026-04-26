//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Runtime.InteropServices;
using System.Threading;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using XCad.Sw.Base;
using XCad.Sw.Documents;

namespace XCad.Sw {
    public interface ISwSelObject : ISwObject, IXTransaction {
        /// <summary>
        /// Identifies if this object is currently selected
        /// </summary>
        bool IsSelected { get; }

        /// <summary>
        /// Selects object in the document
        /// </summary>
        /// <param name="append">True to add selection to the current list, false to clear existing selection</param>
        void Select(bool append);

        /// <summary>
        /// Deletes this object
        /// </summary>
        void Delete();
    }

    /// <inheritdoc/>
    internal class SwSelObject : SwObject, ISwSelObject {
        public virtual bool IsCommitted => true;

        public bool IsSelected => SelectionIndex != -1;

        internal int SelectionIndex {
            get {
                var selMgr = OwnerModelDoc.ISelectionManager;

                for(int i = 1; i < selMgr.GetSelectedObjectCount2(-1) + 1; i++) {
                    if(IsSameDispatch(selMgr.GetSelectedObject6(i, -1))) {
                        return i;
                    }
                }

                return -1;
            }
        }

        internal SwSelObject(object disp, SwDocument doc, SwApplication app) : base(disp, doc, app) {
        }

        public void Select(bool append) => Select(append, null);

        internal virtual void Select(bool append, ISelectData selData) {
            if(OwnerModelDoc != null) {
                if(OwnerModelDoc.Extension.MultiSelect2(new DispatchWrapper[] { new DispatchWrapper(Dispatch) }, append, selData) != 1) {
                    throw new Exception("Failed to select");
                }
            } else {
                throw new Exception("Model doc is not initialized");
            }
        }

        public virtual void Commit(CancellationToken cancellationToken) {
        }

        public virtual void Delete() {
            Select(false);

            if(!OwnerModelDoc.Extension.DeleteSelection2((int)swDeleteSelectionOptions_e.swDelete_Absorbed)) {
                throw new Exception("Failed to delete the object");
            }
        }

        protected virtual bool IsSameDispatch(object disp)
            => OwnerApplication.Sw.IsSame(disp, Dispatch) == (int)swObjectEquality.swObjectSame;
    }
}