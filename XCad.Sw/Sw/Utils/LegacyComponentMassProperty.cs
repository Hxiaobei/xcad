//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Linq;
using SolidWorks.Interop.sldworks;
using XCad.Sw.Documents;
using XCad.Sw.Geometry.Exceptions;

namespace XCad.Sw.Utils {
    /// <summary>
    /// Component specific mass property information
    /// </summary>
    internal class LegacyComponentMassProperty {
        internal ISwDocument3D Document { get; }
        internal ISwComponent Component { get; }
        internal IMassProperty MassProperty { get; }
        internal IXUnits UserUnit { get; }

        internal LegacyComponentMassProperty(ISwDocument3D doc, ISwComponent component, IMassProperty massProperty, IXUnits userUnit) {
            Document = doc;
            Component = component;
            MassProperty = massProperty;
            UserUnit = userUnit;
        }
    }

    /// <summary>
    /// Helper class to retrieve the IMassProperty for the specific component's model for workarounds purposes
    /// </summary>
    internal class LegacyComponentMassPropertyLazy : Lazy<LegacyComponentMassProperty> {
        internal LegacyComponentMassPropertyLazy(Func<ISwComponent[]> compsFunc, Func<IXUnits> unitsFunc = null)
            : base(() => CreateComponentMassProperty(compsFunc.Invoke(), unitsFunc?.Invoke())) {
        }

        private static LegacyComponentMassProperty CreateComponentMassProperty(ISwComponent[] comps, IXUnits units) {
            if(comps?.Length == 1) {
                var comp = comps.First();
                var refDoc = comp.ReferencedDocument;

                if(!refDoc.IsCommitted) {
                    throw new NotLoadedMassPropertyComponentException(comp);
                }

                var massPrps = refDoc.Model.Extension.CreateMassProperty();

                //NOTE: always resolving the system units as it is requried to get units from the assembly (not the component) for the units and also by some reasons incorrect COG is returned for the user units
                massPrps.UseSystemUnits = true;

                return new LegacyComponentMassProperty(refDoc, comp, massPrps, units);
            } else {
                throw new NotSupportedException("Only single component is supported for scope in the assembly");
            }
        }
    }
}
