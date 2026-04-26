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

namespace XCad.Sw.Delegates {
    /// <summary>
    /// Delegate for <see cref="IXServiceConsumer.ConfigureServices"/> event
    /// </summary>
    /// <param name="sender">Sender of this event</param>
    /// <param name="collection">Collection of services to configure</param>
    public delegate void ConfigureServicesDelegate(IXServiceConsumer sender, IXServiceCollection collection);
}
