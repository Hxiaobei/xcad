//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using SolidWorks.Interop.swconst;
using XCad.Sw.Annotations;
using XCad.Sw.Documents;
using XCad.Sw.Features;
using XCad.Sw.Geometry;
using XCad.Sw.Sketch;

namespace XCad.Sw.Utils {
    internal static class SwSelectionHelper {
        //NOTE: this should be a list so we keep the order correct (not the case for the Dictionary)
        //this would allow to proritize the grouped selection types, like ISwBody (both surface and solid),
        //but still return the specific selection type (e.g. IXSolidBody)
        static readonly List<KeyValuePair<Type, swSelectType_e[]>> m_Map;

        static SwSelectionHelper() {
            m_Map = new List<KeyValuePair<Type, swSelectType_e[]>>();

            AddToMap<ISwEdge>(swSelectType_e.swSelEDGES);
            AddToMap<ISwFace>(swSelectType_e.swSelFACES);
            AddToMap<ISwVertex>(swSelectType_e.swSelVERTICES);

            AddToMap<ISwPlane>(swSelectType_e.swSelDATUMPLANES);
            AddToMap<ISwCoordinateSystem>(swSelectType_e.swSelCOORDSYS);
            AddToMap<ISwSketchBase>(swSelectType_e.swSelSKETCHES);

            AddToMap<ISwSketchPicture>(swSelectType_e.swSelSKETCHBITMAP);
            AddToMap<ISwSketchRegion>(swSelectType_e.swSelSKETCHREGION);
            AddToMap<ISwSketchPoint>(swSelectType_e.swSelSKETCHPOINTS, swSelectType_e.swSelEXTSKETCHPOINTS);
            AddToMap<ISwSketchText>(swSelectType_e.swSelSKETCHTEXT, swSelectType_e.swSelEXTSKETCHTEXT);
            AddToMap<ISwSketchSegment>(swSelectType_e.swSelSKETCHSEGS, swSelectType_e.swSelEXTSKETCHSEGS);
            AddToMap<ISwSketchBlockInstance>(swSelectType_e.swSelBLOCKINST, swSelectType_e.swSelSUBSKETCHINST);
            AddToMap<ISwSketchBlockDefinition>(swSelectType_e.swSelBLOCKDEF, swSelectType_e.swSelSUBSKETCHDEF);
            AddToMap<ISwSketchEntity>(swSelectType_e.swSelSKETCHSEGS, swSelectType_e.swSelEXTSKETCHSEGS, swSelectType_e.swSelSKETCHPOINTS, swSelectType_e.swSelEXTSKETCHPOINTS, swSelectType_e.swSelBLOCKINST, swSelectType_e.swSelSUBSKETCHINST);

            //AddToMap<ISwNote>(swSelectType_e.swSelNOTES);
            AddToMap<ISwDimension>(swSelectType_e.swSelDIMENSIONS);

            //AddToMap<ISwSheet>(swSelectType_e.swSelSHEETS);
            //AddToMap<ISwDrawingView>(swSelectType_e.swSelDRAWINGVIEWS);

            AddToMap<ISwComponent>(swSelectType_e.swSelCOMPONENTS);
            AddToMap<ISwSolidBody>(swSelectType_e.swSelSOLIDBODIES);
            AddToMap<ISwSheetBody>(swSelectType_e.swSelSURFACEBODIES);
            AddToMap<ISwBody>(swSelectType_e.swSelSOLIDBODIES, swSelectType_e.swSelSURFACEBODIES);
        }

        static void AddToMap<T>(params swSelectType_e[] selTypes) where T : ISwSelObject
           => m_Map.Add(new KeyValuePair<Type, swSelectType_e[]>(typeof(T), selTypes));

        internal static IReadOnlyList<swSelectType_e> GetSelectionType(Type type) {
            foreach(var map in m_Map)
                if(map.Key.IsAssignableFrom(type))
                    return map.Value;

            return null;
        }
    }
}
