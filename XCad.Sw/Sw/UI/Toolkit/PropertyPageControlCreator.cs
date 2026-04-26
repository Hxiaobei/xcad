//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Windows.Forms;
using SolidWorks.Interop.sldworks;
using XCad.Sw.UI.Commands.Exceptions;
using XCad.Sw.UI.PropertyPage.Toolkit.Controls;
using XCad.Sw.Utils;
using XCad.UI;
using XCad.UI.PropertyPage;

namespace XCad.Sw.UI.Toolkit {
    internal class PropertyPageControlCreator<TControl>
        : CustomControlCreator<IXCustomControl, TControl> {
        private readonly IPropertyManagerPageWindowFromHandle m_PmpCtrl;

        internal PropertyPageControlCreator(IPropertyManagerPageWindowFromHandle pmpCtrl) {
            m_PmpCtrl = pmpCtrl;
        }

        protected override IXCustomControl HostNetControl(Control winCtrlHost, TControl ctrl, string title, IXImage image) {
            if(m_PmpCtrl.SetWindowHandlex64(winCtrlHost.Handle.ToInt64())) {
                if(ctrl is IXCustomControl) {
                    if(ctrl is System.Windows.FrameworkElement) {
                        return new WpfCustomControlWrapper((IXCustomControl)ctrl);
                    } else {
                        return (IXCustomControl)ctrl;
                    }
                } else {
                    if(ctrl is System.Windows.FrameworkElement) {
                        return new WpfCustomControl((System.Windows.FrameworkElement)(object)ctrl, winCtrlHost);
                    }

                    throw new NotSupportedException($"'{ctrl.GetType()}' must implement '{typeof(IXCustomControl).FullName}' or inherit '{typeof(System.Windows.FrameworkElement).FullName}'");
                }
            } else {
                throw new NetControlHostException(winCtrlHost.Handle);
            }
        }

        protected override IXCustomControl HostComControl(string progId, string title, IXImage image, out TControl specCtrl)
            => throw new NotImplementedException("ActiveX controls are not implemented yet");
    }
}
