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

namespace XCad.Sw.Documents.Delegates {
    /// <summary>
    /// Delegate for <see cref="ISwConfigurationRepository.ConfigurationActivated"/> event
    /// </summary>
    /// <param name="doc">Document owner of this configuration</param>
    /// <param name="newConf">Configuration which is activated</param>
    public delegate void ConfigurationActivatedDelegate(ISwDocument3D doc, ISwConfiguration newConf);
}