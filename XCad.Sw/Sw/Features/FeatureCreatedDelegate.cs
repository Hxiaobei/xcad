//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using XCad.Sw.Documents;

namespace XCad.Sw.Features {
    /// <summary>
    /// Delegate of <see cref="ISwFeatureRepository.FeatureCreated"/> notification
    /// </summary>
    /// <param name="doc">Document where new feature is added</param>
    /// <param name="feature">Feature which is added to the document</param>
    public delegate void FeatureCreatedDelegate(ISwDocument doc, ISwFeature feature);
}
