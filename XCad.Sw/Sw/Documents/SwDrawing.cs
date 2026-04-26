using System.Threading;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using XCad.Sw.Base;

namespace XCad.Sw.Documents {
    public interface ISwDrawing : ISwDocument {
        IDrawingDoc Drawing { get; }

        /// <summary>
        /// Drawing specific options
        /// </summary>
        new ISwDrawingOptions Options { get; }

        /// <summary>
        /// <see cref="ISwDrawing"/> specific save as operation
        /// </summary>
        new ISwDrawingSaveOperation PreCreateSaveAsOperation(string filePath);
    }

    public interface ISwDrawingOptions : ISwDocumentOptions {
        ISwDrawingDetailingOptions Detailing { get; }
    }

    public interface ISwDrawingDetailingOptions {
        /// <summary>
        /// Display cosmetic threads
        /// </summary>
        bool DisplayCosmeticThreads { get; set; }

        /// <summary>
        /// Auto insert center marks for slots
        /// </summary>
        bool AutoInsertCenterMarksForSlots { get; set; }

        /// <summary>
        /// Auto insert center marks for fillets
        /// </summary>
        bool AutoInsertCenterMarksForFillets { get; set; }

        /// <summary>
        /// Auto insert center marks for holes
        /// </summary>
        bool AutoInsertCenterMarksForHoles { get; set; }

        /// <summary>
        /// Auto insert dowel symbols
        /// </summary>
        bool AutoInsertDowelSymbols { get; set; }
    }

    internal class SwDrawingDetailingOptions : ISwDrawingDetailingOptions {
        private readonly SwDrawing m_Draw;

        internal SwDrawingDetailingOptions(SwDrawing draw) {
            m_Draw = draw;
        }

        public bool DisplayCosmeticThreads {
            get => m_Draw.GetUserPreferenceToggle(swUserPreferenceToggle_e.swDisplayCosmeticThreads);
            set => m_Draw.SetUserPreferenceToggle(swUserPreferenceToggle_e.swDisplayCosmeticThreads, value);
        }

        public bool AutoInsertCenterMarksForSlots {
            get => m_Draw.GetUserPreferenceToggle(swUserPreferenceToggle_e.swDetailingAutoInsertCenterMarksForSlots);
            set => m_Draw.SetUserPreferenceToggle(swUserPreferenceToggle_e.swDetailingAutoInsertCenterMarksForSlots, value);
        }

        public bool AutoInsertCenterMarksForFillets {
            get => m_Draw.GetUserPreferenceToggle(swUserPreferenceToggle_e.swDetailingAutoInsertCenterMarksForFillets);
            set => m_Draw.SetUserPreferenceToggle(swUserPreferenceToggle_e.swDetailingAutoInsertCenterMarksForFillets, value);
        }

        public bool AutoInsertCenterMarksForHoles {
            get => m_Draw.GetUserPreferenceToggle(swUserPreferenceToggle_e.swDetailingAutoInsertCenterMarksForHoles);
            set => m_Draw.SetUserPreferenceToggle(swUserPreferenceToggle_e.swDetailingAutoInsertCenterMarksForHoles, value);
        }

        public bool AutoInsertDowelSymbols {
            get => m_Draw.GetUserPreferenceToggle(swUserPreferenceToggle_e.swDetailingAutoInsertDowelSymbols);
            set => m_Draw.SetUserPreferenceToggle(swUserPreferenceToggle_e.swDetailingAutoInsertDowelSymbols, value);
        }
    }

    internal class SwDrawingOptions : SwDocumentOptions, ISwDrawingOptions {
        internal SwDrawingOptions(SwDrawing draw) : base(draw) {
            Detailing = new SwDrawingDetailingOptions(draw);
        }

        public ISwDrawingDetailingOptions Detailing { get; }
    }

    internal class SwDrawing : SwDocument, ISwDrawing {

        public IDrawingDoc Drawing => Model as IDrawingDoc;

        internal protected override swDocumentTypes_e? DocumentType => swDocumentTypes_e.swDocDRAWING;

        public override IXDocumentOptions Options => m_Options;

        ISwDrawingOptions ISwDrawing.Options => m_Options;

        private readonly SwDrawingOptions m_Options;

        internal SwDrawing(IDrawingDoc drawing, SwApplication app, IXLogger logger, bool isCreated)
            : base((IModelDoc2)drawing, app, logger, isCreated) {
            m_Options = new SwDrawingOptions(this);
        }

        protected override void CommitCache(IModelDoc2 model, CancellationToken cancellationToken) {
            base.CommitCache(model, cancellationToken);
        }

        protected override bool IsDocumentTypeCompatible(swDocumentTypes_e docType) => docType == swDocumentTypes_e.swDocDRAWING;

        protected override SwAnnotationCollection CreateAnnotations() => new SwDrawingAnnotationCollection(this);

        ISwDrawingSaveOperation ISwDrawing.PreCreateSaveAsOperation(string filePath) {
            var ext = System.IO.Path.GetExtension(filePath);

            switch(ext.ToLower()) {
                case ".pdf":
                    return new SwDrawingPdfSaveOperation(this, filePath);

                case ".dxf":
                case ".dwg":
                    return new SwDxfDwgSaveOperation(this, filePath);

                default:
                    return new SwDrawingSaveOperation(this, filePath);
            }
        }

        public override IXSaveOperation PreCreateSaveAsOperation(string filePath) => ((ISwDrawing)this).PreCreateSaveAsOperation(filePath);

    }
}