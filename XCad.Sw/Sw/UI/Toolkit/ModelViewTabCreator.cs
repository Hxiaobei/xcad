//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Windows.Forms;
using SolidWorks.Interop.sldworks;
using XCad.kit;
using XCad.Sw.Services;
using XCad.Sw.UI.Commands.Exceptions;
using XCad.Sw.Utils;
using XCad.UI;

namespace XCad.Sw.UI.Toolkit {
    internal class ModelViewTabCreator<TControl> : CustomControlCreator<string, TControl> {
        private readonly IServiceProvider m_SvcProvider;
        private readonly ModelViewManager m_ModelViewMgr;
        private readonly IModelViewControlProvider m_CtrlProvider;

        internal ModelViewTabCreator(ModelViewManager modelViewMgr, IServiceProvider svcProvider) {
            m_ModelViewMgr = modelViewMgr;
            m_SvcProvider = svcProvider;
            m_CtrlProvider = m_SvcProvider.GetService<IModelViewControlProvider>();
        }

        protected override string HostComControl(string progId, string title, IXImage image,
            out TControl specCtrl) {
            specCtrl = (TControl)m_CtrlProvider.ProvideComControl(m_ModelViewMgr, progId, title);

            if(specCtrl != null) {
                return title;
            } else {
                throw new ComControlHostException(progId);
            }
        }

        protected override string HostNetControl(Control winCtrlHost, TControl ctrl,
            string title, IXImage image) {
            if(m_CtrlProvider.ProvideNetControl(m_ModelViewMgr, winCtrlHost, title)) {
                return title;
            } else {
                throw new NetControlHostException(winCtrlHost.Handle);
            }
        }
    }
}
