//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;

namespace XCad.UI.PropertyPage.Attributes {
    /// <summary>
    /// Indicates that this property provides dynamic control for property page
    /// </summary>
    /// <remarks>Specify the handler in <see cref="IXEx.CreatePage{TData}(Delegates.CreateDynamicControlsDelegate)"/> to provide controls</remarks>
    public class DynamicControlsAttribute : Attribute {
        /// <summary>
        /// User tag
        /// </summary>
        public object Tag { get; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public DynamicControlsAttribute() {
        }

        /// <summary>
        /// Constructor with tag
        /// </summary>
        /// <param name="tag">Tag to associate with dynamic controls</param>
        public DynamicControlsAttribute(object tag) {
            Tag = tag;
        }
    }
}
