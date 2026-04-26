//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Windows.Forms;
using SolidWorks.Interop.sldworks;

namespace XCad.Sw.Services {
    public interface IModelViewControlProvider {
        object ProvideComControl(IModelViewManager mdlViewMgr, string progId, string title);
        bool ProvideNetControl(IModelViewManager mdlViewMgr, Control ctrl, string title);
    }

    internal class ModelViewControlProvider : IModelViewControlProvider {
        public object ProvideComControl(IModelViewManager mdlViewMgr, string progId, string title)
            => mdlViewMgr.AddControl3(title, progId, "", true);

        public bool ProvideNetControl(IModelViewManager mdlViewMgr, Control ctrl, string title)
            => mdlViewMgr.DisplayWindowFromHandlex64(title, ctrl.Handle.ToInt64(), true);
    }
}
