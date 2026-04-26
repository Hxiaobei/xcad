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
    /// Delegate of data store events of <see cref="IXDocument"/> (e.g. <see cref="ISwDocument.StorageReadAvailable"/>
    /// </summary>
    /// <param name="doc">Sender document</param>
    public delegate void DataStoreAvailableDelegate(ISwDocument doc);
}
