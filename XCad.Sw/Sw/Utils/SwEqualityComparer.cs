using System.Collections.Generic;
using SolidWorks.Interop.swconst;
namespace XCad.Sw.Utils {
    public class SwEqualityComparer<T> : IEqualityComparer<T> {
        public bool Equals(T x, T y) => SwUtils.Sw.IsSame(x, y) == (int)swObjectEquality.swObjectSame;

        public int GetHashCode(T obj) { return 0; }
    }
}
