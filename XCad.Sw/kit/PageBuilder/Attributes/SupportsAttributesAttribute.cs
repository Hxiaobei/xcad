//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using XCad.UI.PropertyPage.Base;

namespace XCad.kit.PageBuilder.Attributes {
    public class SupportsAttributesAttribute : Attribute, IAttribute {
        public Type[] Types { get; private set; }

        public SupportsAttributesAttribute(params Type[] types) {
            Types = types;
        }
    }
}