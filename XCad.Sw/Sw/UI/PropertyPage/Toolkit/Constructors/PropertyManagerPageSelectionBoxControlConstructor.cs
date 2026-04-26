//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using SolidWorks.Interop.sldworks;
using XCad.kit;
using XCad.kit.PageBuilder.Attributes;
using XCad.kit.PageBuilder.Base;
using XCad.kit.Services;
using XCad.Sw.Base;
using XCad.Sw.Base.Enums;
using XCad.Sw.UI.PropertyPage.Toolkit.Controls;
using XCad.UI.PropertyPage.Base;

namespace XCad.Sw.UI.PropertyPage.Toolkit.Constructors {
    [DefaultType(typeof(ISwSelObject))]
    [DefaultType(typeof(IEnumerable<ISwSelObject>))]
    internal class PropertyManagerPageSelectionBoxControlConstructor
        : PropertyManagerPageBaseControlConstructor<PropertyManagerPageSelectionBoxControl, IPropertyManagerPageSelectionbox> {
        public PropertyManagerPageSelectionBoxControlConstructor(SwApplication app, IIconsCreator iconsConv)
            : base(app, iconsConv) {
        }

        protected override PropertyManagerPageSelectionBoxControl Create(IGroup parentGroup, IAttributeSet atts, IMetadata[] metadata, ref int numberOfUsedIds)
            => new PropertyManagerPageSelectionBoxControl(m_App, parentGroup, m_IconConv, atts, metadata, ref numberOfUsedIds);

        public override void PostProcessControls(IEnumerable<IPropertyManagerPageControlEx> ctrls) {
            var logger = m_App.Services.GetService<IXLogger>();

            var selBoxes = ctrls.OfType<PropertyManagerPageSelectionBoxControl>().ToArray();

            var autoAssignSelMarksCtrls = selBoxes
                .Where(s => s.SelectionBox.Mark == -1).ToList();

            var assignedMarks = ctrls.OfType<PropertyManagerPageSelectionBoxControl>()
                .Except(autoAssignSelMarksCtrls).Select(c => c.SelectionBox.Mark).ToList();

            ValidateMarks(assignedMarks);

            if(selBoxes.Length == 1) {
                if(autoAssignSelMarksCtrls.Any()) {
                    autoAssignSelMarksCtrls[0].SelectionBox.Mark = 0;
                }
            } else {
                int index = 0;

                autoAssignSelMarksCtrls.ForEach(c => {
                    int mark;
                    do {
                        mark = (int)Math.Pow(2, index);
                        index++;
                    } while(assignedMarks.Contains(mark));

                    c.SelectionBox.Mark = mark;
                });
            }

            logger.Log($"Assigned selection box marks: {string.Join(", ", selBoxes.Select(s => s.SelectionBox.Mark).ToArray())}", LoggerMessageSeverity_e.Debug);
        }

        private void ValidateMarks(List<int> assignedMarks) {
            var logger = m_App.Services.GetService<IXLogger>();

            if(assignedMarks.Count > 1) {
                var dups = assignedMarks.GroupBy(m => m).Where(g => g.Count() > 1).Select(g => g.Key);

                if(dups.Any()) {
                    logger.Log($"Potential issue for selection boxes as there are duplicate selection marks: {string.Join(", ", dups.ToArray())}", LoggerMessageSeverity_e.Warning);
                }

                var joinedMarks = assignedMarks.Where(m => m != 0 && !IsPowerOfTwo(m));

                if(joinedMarks.Any()) {
                    logger.Log($"Potential issue for selection boxes as not all marks are power of 2: {string.Join(", ", joinedMarks.ToArray())}", LoggerMessageSeverity_e.Warning);
                }

                if(assignedMarks.Any(m => m == 0)) {
                    logger.Log($"Potential issue for selection boxes as some of the marks is 0 which means that all selections allowed", LoggerMessageSeverity_e.Warning);
                }
            }
        }

        private bool IsPowerOfTwo(int mark) {
            return (mark != 0) && ((mark & (mark - 1)) == 0);
        }
    }
}