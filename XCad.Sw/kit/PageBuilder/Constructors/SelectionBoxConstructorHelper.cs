//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using XCad.kit.PageBuilder.Base;
using XCad.kit.Reflection;
using XCad.Sw.Annotations;
using XCad.Sw.Documents;
using XCad.Sw.Geometry;
using XCad.UI.PropertyPage.Enums;

namespace XCad.kit.PageBuilder.Constructors {
    public static class SelectionBoxConstructorHelper {
        public static Type GetElementType(Type type) {
            if(type.IsAssignableToGenericType(typeof(IEnumerable<>))) {
                return type.GetArgumentsOfGenericType(typeof(IEnumerable<>)).First();
            } else {
                return type;
            }
        }

        public static BitmapLabelType_e? GetDefaultBitmapLabel(IAttributeSet atts) {
            var type = atts.ContextType;

            if(type.IsAssignableToGenericType(typeof(IEnumerable<>))) {
                type = type.GetArgumentsOfGenericType(typeof(IEnumerable<>)).First();
            }

            if(IsOfType<ISwFace>(type)) {
                return BitmapLabelType_e.SelectFace;
            } else if(IsOfType<ISwEdge>(type)) {
                return BitmapLabelType_e.SelectEdge;
            } else if(IsOfType<ISwComponent>(type)) {
                return BitmapLabelType_e.SelectComponent;
            } else if(IsOfType<ISwDimension>(type)) {
                return BitmapLabelType_e.LinearDistance;
            }

            return null;
        }

        private static bool IsOfType<T>(Type t) => typeof(T).IsAssignableFrom(t);
    }
}
