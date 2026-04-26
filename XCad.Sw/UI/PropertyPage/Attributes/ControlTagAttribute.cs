//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using XCad.UI.PropertyPage.Base;

namespace XCad.UI.PropertyPage.Attributes {
    public class ControlTagAttribute : Attribute, IControlTagAttribute {
        /// <inheritdoc/>
        public object Tag { get; }

        public ControlTagAttribute(object tag) {
            Tag = tag;
        }
    }
}