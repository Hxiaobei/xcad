//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using XCad.Sw.Enums;

namespace XCad.Sw {

    internal class SwVersion {
        public SwVersion_e Major { get; }

        public Version Version { get; }

        public int ServicePack { get; }
        public int ServicePackRevision { get; }

        internal SwVersion(Version version, int sp, int spRev) {
            Version = version;
            Major = (SwVersion_e)version.Major;

            ServicePack = sp;
            ServicePackRevision = spRev;
        }

    }
}
