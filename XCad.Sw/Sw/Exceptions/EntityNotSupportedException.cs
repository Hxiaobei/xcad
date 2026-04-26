//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using XCad.Sw.Base;

namespace XCad.Sw.Exceptions {
    /// <summary>
    /// Exception is thrown if <see cref="IXRepository.PreCreate{T}"/> does not support the specific entity
    /// </summary>
    public class EntityNotSupportedException : NotSupportedException {
        /// <summary>
        /// Default constructor
        /// </summary>
        public EntityNotSupportedException(Type entType, IReadOnlyList<Type> supportedTypes)
            : base($"Entity {entType.FullName} is not supported by the repository. Supported types: {string.Join(", ", supportedTypes.Select(t => t.FullName))}") {
        }
    }
}
