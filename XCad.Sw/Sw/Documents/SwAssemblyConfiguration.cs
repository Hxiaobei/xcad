using SolidWorks.Interop.sldworks;

namespace XCad.Sw.Documents {
    public interface ISwAssemblyConfiguration : ISwConfiguration {
        ISwComponentCollection Components { get; }
    }

    internal class SwAssemblyConfiguration : SwConfiguration, ISwAssemblyConfiguration {
        internal SwAssemblyConfiguration(IConfiguration conf, SwAssembly assm, SwApplication app, bool created)
            : base(conf, assm, app, created) {
            Components = new SwAssemblyComponentCollection(assm, conf);
        }

        public ISwComponentCollection Components { get; }
    }
}