using XCad.Sw.Base;
using XCad.Sw.Geometry;

namespace XCad.Sw.Documents {
    /// <summary>
    /// Save as operation for the document
    /// </summary>
    /// <remarks>Created via <see cref="ISwDocument.PreCreateSaveAsOperation(string)"/></remarks>
    /// <exception cref="Exceptions.SaveDocumentFailedException"/>
    public interface IXSaveOperation : IXTransaction {
        /// <summary>
        /// Output file path
        /// </summary>
        string FilePath { get; }
    }

    /// <summary>
    /// Save operation of <see cref="ISwDrawing"/> files
    /// </summary>
    public interface ISwDrawingSaveOperation : IXSaveOperation {
    }

    /// <summary>
    /// Save operation of <see cref="ISwDocument3D"/> files
    /// </summary>
    public interface ISwDocument3DSaveOperation : IXSaveOperation {
        /// <summary>
        /// Bodies to export
        /// </summary>
        /// <remarks>If not specified all bodies are exported</remarks>
        ISwBody[] Bodies { get; set; }
    }

    /// <summary>
    /// Step format type
    /// </summary>
    public enum StepFormat_e {
        /// <summary>
        /// STEP AP 203
        /// </summary>
        Ap203,

        /// <summary>
        /// STEP AP 214
        /// </summary>
        Ap214,

        /// <summary>
        /// STEP AP 242
        /// </summary>
        Ap242
    }

    /// <summary>
    /// Save options of step format
    /// </summary>
    public interface IXStepSaveOperation : ISwDocument3DSaveOperation {
        /// <summary>
        /// Step format
        /// </summary>
        StepFormat_e Format { get; set; }
    }

    /// <summary>
    /// Save options for PDF format
    /// </summary>
    public interface IXPdfSaveOperation : IXSaveOperation {
    }

    /// <summary>
    /// Save options for PDF format in 3D document
    /// </summary>
    public interface ISwDocument3DPdfSaveOperation : ISwDocument3DSaveOperation, IXPdfSaveOperation {
        /// <summary>
        /// Save PDF as 3D PDF
        /// </summary>
        bool Pdf3D { get; set; }
    }

    /// <summary>
    /// Save options for PDF format in drawing document
    /// </summary>
    public interface ISwDrawingPdfSaveOperation : IXPdfSaveOperation, ISwDrawingSaveOperation {

    }

    /// <summary>
    /// Options to export splines in <see cref="IXDxfDwgSaveOperation.SplineExportOptions"/>
    /// </summary>
    public enum SplineExportOptions_e {
        /// <summary>
        /// Exports splines as splines
        /// </summary>
        Splines,

        /// <summary>
        /// Exports splines as polylines
        /// </summary>
        Polylines
    }

    /// <summary>
    /// Save options for DXF/DWG format
    /// </summary>
    public interface IXDxfDwgSaveOperation : ISwDrawingSaveOperation {
        /// <summary>
        /// File path to a layers map file
        /// </summary>
        string LayersMapFilePath { get; set; }

        /// <summary>
        /// True to include hidden layers, False to only export visible layers
        /// </summary>
        bool ExportHiddentLayers { get; set; }

        /// <summary>
        /// Options to export splines
        /// </summary>
        SplineExportOptions_e SplineExportOptions { get; set; }
    }
}