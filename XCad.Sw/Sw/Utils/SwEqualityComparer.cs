using System.Collections.Generic;
using SolidWorks.Interop.swconst;
using XCad.Sw;
namespace SwMsg.Extension {
    public class SwEqualityComparer<T> : IEqualityComparer<T> {
        public bool Equals(T x, T y) => SwUtils.Sw.IsSame(x, y) == (int)swObjectEquality.swObjectSame;

        public int GetHashCode(T obj) { return 0; }
    }
}
