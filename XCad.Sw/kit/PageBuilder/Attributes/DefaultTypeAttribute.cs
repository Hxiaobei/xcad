//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using XCad.kit.PageBuilder.Base.Attributes;

namespace XCad.kit.PageBuilder.Attributes {
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class DefaultTypeAttribute : Attribute, IDefaultTypeAttribute {
        public Type Type { get; private set; }

        public DefaultTypeAttribute(Type type) {
            Type = type;
        }
    }
}