//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Windows.Forms;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace XCad.Sw.Services {
    public interface IFeatureManagerTabControlProvider {
        IFeatMgrView ProvideComControl(IModelViewManager mdlViewMgr, string imgPath, string progId, string title);
        IFeatMgrView ProvideNetControl(IModelViewManager mdlViewMgr, Control ctrl, string imgPath, string title);
    }

    internal class FeatureManagerTabControlProvider : IFeatureManagerTabControlProvider {
        public IFeatMgrView ProvideComControl(IModelViewManager mdlViewMgr, string imgPath, string progId, string title)
            => mdlViewMgr.CreateFeatureMgrControl3(imgPath, progId, "", title,
                (int)swFeatMgrPane_e.swFeatMgrPaneBottom);

        public IFeatMgrView ProvideNetControl(IModelViewManager mdlViewMgr, Control ctrl, string imgPath, string title)
            => mdlViewMgr.CreateFeatureMgrWindowFromHandlex64(
                imgPath, ctrl.Handle.ToInt64(),
                title, (int)swFeatMgrPane_e.swFeatMgrPaneBottom);
    }
}
