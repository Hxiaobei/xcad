//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using SolidWorks.Interop.sldworks;
using XCad.Sw.Documents;

namespace XCad.Sw.Utils {
    /// <summary>
    /// Freezes feature tree and view
    /// </summary>
    internal class UiFreeze : IDisposable {
        private readonly IModelDoc2 m_Model;
        private readonly IModelView m_View;

        internal UiFreeze(ISwDocument doc) {
            m_Model = doc.Model;

            m_Model.FeatureManager.EnableFeatureTree = false;
            m_Model.FeatureManager.EnableFeatureTreeWindow = false;

            m_View = m_Model.IActiveView;

            if(m_View != null) {
                m_View.EnableGraphicsUpdate = false;
            }
        }

        public void Dispose() {
            m_Model.FeatureManager.EnableFeatureTree = true;
            m_Model.FeatureManager.EnableFeatureTreeWindow = true;

            if(m_View != null) {
                m_View.EnableGraphicsUpdate = true;
            }
        }
    }
}
