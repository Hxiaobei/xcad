//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.ComponentModel;


namespace XCad.Sw.Base.Attributes {
    /// <summary>
    /// Decorates the display name for the element (e.g. command, user control, object etc.)
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public class TitleAttribute : DisplayNameAttribute {
        /// <summary>
        /// Constructor for element title
        /// </summary>
        /// <param name="dispName">Display name of the element</param>
        public TitleAttribute(string dispName) : base(dispName) {
        }

        /// <inheritdoc cref="TitleAttribute(string)"/>
        /// <param name="resType">Type of the static class (usually Resources)</param>
        /// <param name="dispNameResName">Resource name of the string for display name</param>
        public TitleAttribute(Type resType, string dispNameResName)
            : this(TypeExtension.GetResource<string>(resType, dispNameResName)) {
        }
    }
}