//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using XCad.UI.PropertyPage.Base;

namespace XCad.UI.PropertyPage.Attributes {
    /// <summary>
    /// Control should not be created for this property
    /// </summary>
    public class ExcludeControlAttribute : Attribute, IIgnoreBindingAttribute {
    }
}