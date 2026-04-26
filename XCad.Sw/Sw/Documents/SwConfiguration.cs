
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using XCad.kit.Services;
using XCad.Sw.Annotations;
using XCad.Sw.Base;
using XCad.Sw.Documents.Enums;
using XCad.Sw.Documents.Exceptions;
using XCad.Sw.Enums;
using XCad.Sw.Features;
using XCad.Sw.Utils;
using XCad.UI;

namespace XCad.Sw.Documents {
    public interface ISwConfiguration : ISwSelObject, IXTransaction, IDimensionable {

        IConfiguration Configuration { get; }

        double Quantity { get; }

        string Name { get; set; }

        string PartNumber { get; }

        BomChildrenSolving_e BomChildrenSolving { get; }

        IXImage Preview { get; }
    }

    [DebuggerDisplay("{" + nameof(Name) + "}")]
    internal class SwConfiguration : SwSelObject, ISwConfiguration {
        internal const string QTY_PROPERTY = "UNIT_OF_MEASURE";

        public IConfiguration Configuration => m_Creator.Element;

        private readonly SwDocument3D m_Doc;

        public virtual string Name {
            get => m_Creator.IsCreated ? Configuration.Name : m_Creator.CachedProperties.Get<string>();
            set {
                if(m_Creator.IsCreated)
                    Configuration.Name = value;
                else
                    m_Creator.CachedProperties.Set(value);
            }
        }

        IXRepository<ISwDimension> IDimensionable.Dimensions => Dimensions;

        public ISwDimensionsCollection Dimensions => m_DimensionsLazy.Value;

        private readonly Lazy<ISwDimensionsCollection> m_DimensionsLazy;

        public override bool IsCommitted => m_Creator.IsCreated;

        private readonly IElementCreator<IConfiguration> m_Creator;

        internal SwConfiguration(IConfiguration conf, SwDocument3D doc, SwApplication app, bool created) : base(conf, doc, app) {
            m_Doc = doc;
            m_Creator = new ElementCreator<IConfiguration>(Create, conf, created);
            m_DimensionsLazy = new Lazy<ISwDimensionsCollection>(CreateDimensions);
        }

        public override object Dispatch => Configuration;

        public IXImage Preview {
            get {
                if(OwnerApplication.IsInProcess())
                    return PictureDispUtils.PictureDispToXImage(OwnerApplication.Sw.GetPreviewBitmap(m_Doc.Path, Name));
                else
                    return new XDrawingImage(m_Doc.GetThumbnailImage());
            }
        }

        public string PartNumber => GetPartNumber(Configuration);

        public double Quantity {
            get {
                // 尝试从配置属性中获取数量值
                var qtyPrp = GetPropertyValue(Configuration.CustomPropertyManager, QTY_PROPERTY);
                if(string.IsNullOrEmpty(qtyPrp))
                    qtyPrp = GetPropertyValue(m_Doc.Model.Extension.CustomPropertyManager[""], QTY_PROPERTY);

                if(string.IsNullOrEmpty(qtyPrp))
                    return 1;

                var qtyStr = GetPropertyValue(Configuration.CustomPropertyManager, qtyPrp);
                if(string.IsNullOrEmpty(qtyStr))
                    qtyStr = GetPropertyValue(m_Doc.Model.Extension.CustomPropertyManager[""], qtyPrp);

                return double.TryParse(qtyStr, out var qty) ? qty : 1;
            }
        }

        public BomChildrenSolving_e BomChildrenSolving {
            get {
                if(!(m_Doc is ISwAssembly))
                    return BomChildrenSolving_e.Show;

                var bomDispOpt = Configuration.ChildComponentDisplayInBOM;
                switch((swChildComponentInBOMOption_e)bomDispOpt) {
                    case swChildComponentInBOMOption_e.swChildComponent_Show:
                        return BomChildrenSolving_e.Show;
                    case swChildComponentInBOMOption_e.swChildComponent_Hide:
                        return BomChildrenSolving_e.Hide;
                    case swChildComponentInBOMOption_e.swChildComponent_Promote:
                        return BomChildrenSolving_e.Promote;
                    default:
                        throw new NotSupportedException($"Not supported BOM display option: {bomDispOpt}");
                }
            }
        }

        public virtual ISwConfiguration Parent {
            get {
                var conf = Configuration.GetParent();
                return conf != null ? OwnerDocument.CreateObjectFromDispatch<ISwConfiguration>(conf) : null;
            }
        }

        private string GetPropertyValue(ICustomPropertyManager prpMgr, string prpName) {
            string resVal;
            if(OwnerApplication.IsVersionNewerOrEqual(SwVersion_e.Sw2018))
                prpMgr.Get6(prpName, false, out _, out resVal, out _, out _);
            else if(OwnerApplication.IsVersionNewerOrEqual(SwVersion_e.Sw2014))
                prpMgr.Get5(prpName, false, out _, out resVal, out _);
            else
                prpMgr.Get4(prpName, false, out _, out resVal);
            return resVal;
        }

        private string GetPartNumber(IConfiguration conf) {
            switch((swBOMPartNumberSource_e)conf.BOMPartNoSource) {
                case swBOMPartNumberSource_e.swBOMPartNumber_ConfigurationName:
                    return conf.Name;
                case swBOMPartNumberSource_e.swBOMPartNumber_DocumentName:
                    return Path.GetFileNameWithoutExtension(m_Doc.Title);
                case swBOMPartNumberSource_e.swBOMPartNumber_ParentName:
                    return GetPartNumber(conf.GetParent());
                case swBOMPartNumberSource_e.swBOMPartNumber_UserSpecified:
                    return conf.AlternateName;
                default:
                    throw new NotSupportedException();
            }
        }

        public override void Commit(CancellationToken cancellationToken) => m_Creator.Create(cancellationToken);

        protected virtual ISwDimensionsCollection CreateDimensions()
            => new SwFeatureManagerDimensionsCollection(new SwDocumentFeatureManager(m_Doc, m_Doc.OwnerApplication, new Context(this)), new Context(this));

        private IConfiguration Create(CancellationToken cancellationToken) {
            IConfiguration conf;
            if(OwnerApplication.IsVersionNewerOrEqual(SwVersion_e.Sw2018))
                conf = m_Doc.Model.ConfigurationManager.AddConfiguration2(Name, "", "", (int)swConfigurationOptions2_e.swConfigOption_DontActivate, "", "", false);
            else
                conf = m_Doc.Model.ConfigurationManager.AddConfiguration(Name, "", "", (int)swConfigurationOptions2_e.swConfigOption_DontActivate, "", "");

            if(conf == null)
                throw new Exception("Failed to create configuration");

            return conf;
        }

    }

    internal abstract class SwComponentConfiguration : SwConfiguration {
        private static IConfiguration GetConfiguration(SwComponent comp, string compName) {
            var doc = comp.ReferencedDocument;
            return doc.IsCommitted ? (IConfiguration)doc.Model.GetConfigurationByName(compName) : null;
        }

        protected readonly SwComponent m_Comp;

        internal SwComponentConfiguration(SwComponent comp, SwApplication app, string confName)
            : this(GetConfiguration(comp, confName), (SwDocument3D)comp.ReferencedDocument, app, comp.Component.ReferencedConfiguration) {
            m_Comp = comp;
        }

        public override ISwConfiguration Parent {
            get {
                var conf = Configuration.GetParent();
                return conf != null ? m_Comp.GetReferencedConfiguration(conf.Name) : null;
            }
        }

        private SwComponentConfiguration(IConfiguration conf, SwDocument3D doc, SwApplication app, string name)
            : base(conf, doc, app, conf != null) {
            if(conf == null)
                Name = name;
        }

        protected override ISwDimensionsCollection CreateDimensions()
            => new SwFeatureManagerDimensionsCollection(
                new SwComponentFeatureManager(m_Comp, m_Comp.RootAssembly, OwnerApplication, new Context(this)), new Context(this));
    }

    internal class SwPartComponentConfiguration : SwComponentConfiguration, ISwPartConfiguration {
        public SwPartComponentConfiguration(SwPartComponent comp, SwApplication app, string confName) : base(comp, app, confName) { }
    }

    internal class SwAssemblyComponentConfiguration : SwComponentConfiguration, ISwAssemblyConfiguration {
        public SwAssemblyComponentConfiguration(SwComponent comp, SwApplication app, string confName) : base(comp, app, confName) { }
        public ISwComponentCollection Components => m_Comp.Children;
    }

    internal class SwViewOnlyUnloadedConfiguration : SwConfiguration {
        public override string Name {
            get => m_ViewOnlyConfName;
            set => throw new NotSupportedException("Name of view-only configuration cannot be changed");
        }

        private readonly string m_ViewOnlyConfName;

        internal SwViewOnlyUnloadedConfiguration(string confName, SwDocument3D doc, SwApplication app)
            : base(null, doc, app, false) {
            m_ViewOnlyConfName = confName;
        }

        public override void Commit(CancellationToken cancellationToken) => throw new InactiveLdrConfigurationNotSupportedException();
        public override object Dispatch => throw new InactiveLdrConfigurationNotSupportedException();
    }

    internal class SwLdrAssemblyUnloadedConfiguration : SwAssemblyConfiguration {
        public override string Name {
            get => m_LdrConfName;
            set => throw new NotSupportedException("Name of inactive LDR configuration cannot be changed");
        }

        private readonly string m_LdrConfName;

        internal SwLdrAssemblyUnloadedConfiguration(SwAssembly assm, SwApplication app, string confName)
            : base(null, assm, app, false) {
            m_LdrConfName = confName;
        }

        public override void Commit(CancellationToken cancellationToken) => throw new InactiveLdrConfigurationNotSupportedException();
        public override object Dispatch => throw new InactiveLdrConfigurationNotSupportedException();
    }

    internal class SwLdrPartUnloadedConfiguration : SwPartConfiguration {
        public override string Name {
            get => m_LdrConfName;
            set => throw new NotSupportedException("Name of inactive LDR configuration cannot be changed");
        }

        private readonly string m_LdrConfName;

        internal SwLdrPartUnloadedConfiguration(SwPart part, SwApplication app, string confName)
            : base(null, part, app, false) {
            m_LdrConfName = confName;
        }

        public override void Commit(CancellationToken cancellationToken) => throw new InactiveLdrConfigurationNotSupportedException();
        public override object Dispatch => throw new InactiveLdrConfigurationNotSupportedException();
    }
}