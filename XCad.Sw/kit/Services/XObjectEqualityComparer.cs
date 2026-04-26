using System.Collections.Generic;
using XCad.Sw;

namespace XCad.kit.Services {
    /// <summary>
    /// Represents the generic equality of the <see cref="ISwObject"/>
    /// </summary>
    /// <typeparam name="TObj">Specific type of <see cref="ISwObject"/></typeparam>
    public class XObjectEqualityComparer<TObj> : IEqualityComparer<TObj> where TObj : ISwObject {
        public bool Equals(TObj x, TObj y) {
            if(ReferenceEquals(x, y)) 
                return true;
            if(x == null || y == null) 
                return false;

            return x.Equals(y);
        }

        public int GetHashCode(TObj obj) => 0;
    }
}