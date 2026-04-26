//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Windows.Forms;
using SolidWorks.Interop.sldworks;

namespace XCad.Sw.Services {
    public interface ITaskPaneControlProvider {
        object ProvideComControl(ITaskpaneView taskPaneView, string progId);
        bool ProvideNetControl(ITaskpaneView taskPaneView, Control ctrl);
    }

    internal class TaskPaneControlProvider : ITaskPaneControlProvider {
        public object ProvideComControl(ITaskpaneView taskPaneView, string progId)
            => taskPaneView.AddControl(progId, "");

        public bool ProvideNetControl(ITaskpaneView taskPaneView, Control ctrl)
            => taskPaneView.DisplayWindowFromHandle(ctrl.Handle.ToInt32());
    }
}
