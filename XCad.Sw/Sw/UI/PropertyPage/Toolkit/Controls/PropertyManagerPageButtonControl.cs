//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using XCad.kit.PageBuilder.Base;
using XCad.kit.PageBuilder.PageElements;
using XCad.kit.Services;
using XCad.UI.PropertyPage.Base;

namespace XCad.Sw.UI.PropertyPage.Toolkit.Controls {
    internal class PropertyManagerPageButtonControl : PropertyManagerPageBaseControl<Action, IPropertyManagerPageButton> {
#pragma warning disable CS0067

        protected override event ControlValueChangedDelegate<Action> ValueChanged;

#pragma warning restore CS0067

        private Action m_ButtonClickHandler;

        public PropertyManagerPageButtonControl(SwApplication app, IGroup parentGroup, IIconsCreator iconConv,
            IAttributeSet atts, IMetadata[] metadata, ref int numberOfUsedIds)
            : base(app, parentGroup, iconConv, atts, metadata, swPropertyManagerPageControlType_e.swControlType_Button, ref numberOfUsedIds) {
            m_Handler.ButtonPressed += OnButtonPressed;
        }

        protected override void SetOptions(IPropertyManagerPageButton ctrl, IControlOptionsAttribute opts, IAttributeSet atts) {
            ctrl.Caption = atts.Name;
        }

        private void OnButtonPressed(int id) {
            if(Id == id) {
                if(m_ButtonClickHandler == null) {
                    throw new NullReferenceException("Button click handler is not specified. Set the value of the delegate to the handler method");
                }

                m_ButtonClickHandler.Invoke();
            }
        }

        protected override Action GetSpecificValue() => m_ButtonClickHandler;

        protected override void SetSpecificValue(Action value) => m_ButtonClickHandler = value;

        protected override void Dispose(bool disposing) {
            if(disposing) {
                base.Dispose(disposing);

                m_Handler.ButtonPressed -= OnButtonPressed;
            }
        }
    }
}