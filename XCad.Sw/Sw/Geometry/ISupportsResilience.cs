//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************


//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

namespace XCad.Sw.Geometry {
    /// <summary>
    /// Indicates that this object can be resilient to the regeneration operations
    /// </summary>
    public interface ISupportsResilience : ISwObject {
        /// <summary>
        /// Is object resilient to regeneration
        /// </summary>
        bool IsResilient { get; }

        /// <summary>
        /// Converts this object to resilient object
        /// </summary>
        /// <returns>Resilient object</returns>
        ISwObject CreateResilient();
    }

    /// <inheritdoc/>
    /// <typeparam name="T">Specific object type</typeparam>
    public interface ISupportsResilience<T> : ISupportsResilience
        where T : ISwObject {
        /// <summary>
        /// Specific implementation of resilient object
        /// </summary>
        /// <returns>Resilient object</returns>
        new T CreateResilient();
    }
}
