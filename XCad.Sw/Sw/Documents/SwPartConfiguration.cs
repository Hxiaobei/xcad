//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;

namespace XCad.Sw.Documents {
    public interface ISwPartConfiguration : ISwConfiguration {

    }

    internal class SwPartConfiguration : SwConfiguration, ISwPartConfiguration {
        private readonly SwPart m_Part;

        internal SwPartConfiguration(IConfiguration conf, SwPart part, SwApplication app, bool created)
            : base(conf, part, app, created) {
            m_Part = part;
        }

    }
}