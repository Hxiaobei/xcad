//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Linq;
using System.Windows.Forms;
using SolidWorks.Interop.sldworks;
using XCad.kit;
using XCad.kit.Services;
using XCad.Sw.Services;
using XCad.Sw.UI.Commands.Exceptions;
using XCad.Sw.Utils;
using XCad.UI;

namespace XCad.Sw.UI.Toolkit {
    internal class FeatureManagerTabCreator<TControl> : CustomControlCreator<Tuple<IFeatMgrView, string>, TControl> {
        private readonly IServiceProvider m_SvcProvider;
        private readonly ModelViewManager m_ModelViewMgr;
        private readonly IFeatureManagerTabControlProvider m_TabProvider;

        internal FeatureManagerTabCreator(ModelViewManager modelViewMgr, IServiceProvider svcProvider) {
            m_ModelViewMgr = modelViewMgr;
            m_SvcProvider = svcProvider;
            m_TabProvider = m_SvcProvider.GetService<IFeatureManagerTabControlProvider>();
        }

        protected override Tuple<IFeatMgrView, string> HostComControl(string progId, string title, IXImage image,
            out TControl specCtrl) {
            using(var img = m_SvcProvider.GetService<IIconsCreator>().ConvertIcon(new FeatMgrViewIcon(image))) {
                var featMgrView = m_TabProvider.ProvideComControl(m_ModelViewMgr, img.FilePaths.First(), progId, title);

                specCtrl = default;

                if(featMgrView != null) {
                    specCtrl = (TControl)featMgrView.GetControl();
                }

                if(specCtrl == null) {
                    throw new ComControlHostException(progId);
                }

                return new Tuple<IFeatMgrView, string>(featMgrView, title);
            }
        }

        protected override Tuple<IFeatMgrView, string> HostNetControl(Control winCtrlHost, TControl ctrl,
            string title, IXImage image) {
            using(var img = m_SvcProvider.GetService<IIconsCreator>().ConvertIcon(new FeatMgrViewIcon(image))) {
                var featMgrView = m_TabProvider.ProvideNetControl(m_ModelViewMgr, winCtrlHost, img.FilePaths.First(), title);

                if(featMgrView != null) {
                    return new Tuple<IFeatMgrView, string>(featMgrView, title);
                } else {
                    throw new NetControlHostException(winCtrlHost.Handle);
                }
            }
        }
    }
}
