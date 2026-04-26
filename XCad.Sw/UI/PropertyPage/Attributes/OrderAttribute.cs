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
    /// Defines the order of control generation based on the data model
    /// </summary>
    public interface IOrderAttribute : IAttribute {
        /// <summary>
        /// Order of this control
        /// </summary>
        int Order { get; }
    }

    /// <inheritdoc/>
    public class OrderAttribute : Attribute, IOrderAttribute {
        /// <inheritdoc/>
        public int Order { get; }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="order"></param>
        public OrderAttribute(int order) {
            Order = order;
        }
    }
}
