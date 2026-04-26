//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using XCad.Structures;
using XCad.Sw.Base;

namespace XCad.Sw.Geometry {
    public interface ISwRegion : IXTransaction {
        /// <summary>
        /// Boundary of this region
        /// </summary>
        ISwLoop OuterLoop { get; set; }

        /// <summary>
        /// Inner loops in the region
        /// </summary>
        ISwLoop[] InnerLoops { get; set; }
    }

    public interface ISwPlanarRegion : ISwRegion {
        ISwPlanarSheetBody PlanarSheetBody { get; }

        Plane Plane { get; }
    }


}
