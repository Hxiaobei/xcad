//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;

namespace XCad.Sw.Attributes {
    /// <summary>
    /// Registers add-in as the SOLIDWORKS partner product
    /// </summary>
    /// <remarks>
    /// Default constructor
    /// </remarks>
    /// <param name="partnerKey">Partner key of the product</param>
    [AttributeUsage(AttributeTargets.Class)]
    public class PartnerProductAttribute(string partnerKey) : Attribute {
        /// <summary>
        /// Partner key of the product
        /// </summary>
        public string PartnerKey { get; } = partnerKey;
    }
}
