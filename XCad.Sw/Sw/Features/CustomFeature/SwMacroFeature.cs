using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using XCad.kit.CustomFeature;
using XCad.kit.Exceptions;
using XCad.kit.Services;
using XCad.Structures;
using XCad.Sw.Annotations;
using XCad.Sw.Base;
using XCad.Sw.Base.Enums;
using XCad.Sw.Documents;
using XCad.Sw.Extensions;
using XCad.Sw.Features.CustomFeature.Attributes;
using XCad.Sw.Features.CustomFeature.Enums;
using XCad.Sw.Features.CustomFeature.Exceptions;
using XCad.Sw.Features.CustomFeature.Structures;
using XCad.Sw.Features.CustomFeature.Toolkit;
using XCad.Sw.Geometry;
using XCad.Sw.Utils;


namespace XCad.Sw.Features.CustomFeature {

    public interface ISwMacroFeature : ISwFeature {
        /// <summary>
        /// Type of the definition of this custom feature
        /// </summary>
        Type DefinitionType { get; set; }

        /// <summary>
        /// Transformation of this feature
        /// </summary>
        /// <remarks>This is useful when the feature is inserted in the context of the assembly</remarks>
        Transform TargetTransformation { get; }

        /// <summary>
        /// Referenced configuration
        /// </summary>
        ISwConfiguration Configuration { get; }
    }

    internal class SwMacroFeature : SwFeature, ISwMacroFeature {

        private IMacroFeatureData m_FeatData;
        private Type m_DefinitionType;

        public Type DefinitionType {
            get {
                if(IsCommitted && m_DefinitionType == null) {
                    var progId = FeatureData.GetProgId();
                    if(!string.IsNullOrEmpty(progId)) {
                        m_DefinitionType = Type.GetTypeFromProgID(progId);
                    }
                }

                return m_DefinitionType;
            }
            set {
                if(!IsCommitted) {
                    m_DefinitionType = value;
                } else {
                    throw new CommittedElementPropertyChangeNotSupported();
                }
            }
        }

        public IMacroFeatureData FeatureData => m_FeatData ?? (m_FeatData = (IMacroFeatureData)Feature.GetDefinition());

        private readonly IFeatureManager m_FeatMgr;

        internal SwMacroFeature(IFeature feat, SwDocument doc, SwApplication app, bool created)
            : base(feat, doc, app, created) {
            m_FeatMgr = doc.Model.FeatureManager;
        }

        public ISwConfiguration Configuration => OwnerDocument.CreateObjectFromDispatch<SwConfiguration>(FeatureData.CurrentConfiguration);

        public Transform TargetTransformation {
            get {
                if(IsCommitted) {
                    var featTransform = FeatureData.GetEditTargetTransform();
                    return featTransform != null
                        ? featTransform.ToXa()
                        : Transform.Identity;
                }

                if(OwnerDocument is ISwAssembly assembly) {
                    var editComp = assembly.EditingComponent;
                    if(editComp != null) {
                        return editComp.Transformation;
                    }
                }

                return Transform.Identity;
            }
        }

        protected override IFeature InsertFeature(CancellationToken cancellationToken)
            => InsertComFeatureBase(null, null, null, null, null, null, null);

        protected IFeature InsertComFeatureBase(string[] paramNames, int[] paramTypes, string[] paramValues,
                int[] dimTypes, double[] dimValues, object[] selection, object[] editBodies) {
            ValidateDefinitionType();

            var options = DefinitionType.TryGetAttribute<CustomFeatureOptionsAttribute>()?.Flags
                          ?? CustomFeatureOptions_e.Default;

            var baseName = MacroFeatureInfo.GetBaseName(DefinitionType);
            var progId = MacroFeatureInfo.GetProgId(DefinitionType);

            if(string.IsNullOrEmpty(progId))
                throw new NullReferenceException("Prog id for macro feature cannot be extracted");


            var icons = MacroFeatureIconInfo.GetIcons(DefinitionType,
                CompatibilityUtils.SupportsHighResIcons(SwUtils.Sw, CompatibilityUtils.HighResIconsScope_e.MacroFeature));

            using(var selSet = new SelectionGroup(OwnerDocument, false)) {
                if(selection?.Any() == true) selSet.AddRange(selection);


                var feat = (IFeature)m_FeatMgr.InsertMacroFeature3(baseName,
                    progId, null, paramNames, paramTypes,
                    paramValues, dimTypes, dimValues, editBodies, icons, (int)options);

                return feat;
            }
        }

        protected virtual void ValidateDefinitionType() {
            if(!typeof(SwMacroFeatureDefinition).IsAssignableFrom(DefinitionType))
                throw new DefinitionTypeMismatch(DefinitionType, typeof(SwMacroFeatureDefinition));

        }
    }

    public interface ISwMacroFeature<TParams> : ISwMacroFeature
        where TParams : class {
        /// <summary>
        /// Parameters of this feature
        /// </summary>
        TParams Parameters { get; set; }

        /// <summary>
        /// Gets the transformation matrix of the specified entity of the macro feature
        /// </summary>
        /// <param name="entity">Entity to get the transformation from</param>
        /// <returns>Entity transformation matrix</returns>
        /// <remarks>Entity is a selection object which is specified in the <see cref="Parameters"/></remarks>
        Transform GetEntityTransformation(ISwSelObject entity);
    }

    internal class SwMacroFeatureEditor(SwFeature feat, IMacroFeatureData featData) : SwFeatureEditor<IMacroFeatureData>(feat, featData) {
        protected override void CancelEdit(IMacroFeatureData featData) => featData.ReleaseSelectionAccess();

        protected override bool StartEdit(IMacroFeatureData featData, ISwDocument doc, ISwComponent comp)
            => featData.AccessSelections(doc?.Model, comp?.Component);
    }

    internal class SwMacroFeature<TParams> : SwMacroFeature, ISwMacroFeature<TParams>
        where TParams : class {
        private readonly CustomFeatureParametersParser m_ParamsParser;
        private TParams m_ParametersCache;

        internal static SwMacroFeature CreateSpecificInstance(IFeature feat, SwDocument doc, SwApplication app, Type paramType) {
            var macroFeatType = typeof(SwMacroFeature<>).MakeGenericType(paramType);

#if DEBUG
            //NOTE: this is a test to ensure that if constructor is changed the reflection will not be broken and this call will fail at compile time
            var test = new SwMacroFeature<object>(feat, doc, app, true);
#endif
            var constr = macroFeatType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null,
                new Type[] { typeof(IFeature), typeof(SwDocument), typeof(SwApplication), typeof(bool) }, null);

            if(constr == null) {
                Debug.Assert(false, "Modify the parameters above");
                throw new Exception("Failed to create instance of the macro feature - incorrect parameters");
            }

            return (SwMacroFeature)constr.Invoke(new object[] { feat, doc, app, feat != null });
        }

        /// <summary>
        /// Indicates that the parameters should be read from the cache
        /// </summary>
        /// <remarks>This is used when consumer will be accessing the parameters multiple time
        /// and macro feature definition is not changed during this time (e.g. while regenerating)</remarks>
        internal bool UseCachedParameters { get; set; }

        private Dictionary<ISwSelObject, Transform> m_EntitiesTransformsCache;

        //NOTE: this constructor is used in the reflection of SwObjectFactory
        internal SwMacroFeature(IFeature feat, SwDocument doc, SwApplication app, bool created)
            : base(feat, doc, app, created) { m_ParamsParser = CustomFeatureParametersParser.Instance; }

        public override IEditor<ISwFeature> Edit() => new SwMacroFeatureEditor(this, FeatureData);

        public TParams Parameters {
            get {
                if(IsCommitted && (!UseCachedParameters || m_ParametersCache == null)) {
                    m_ParametersCache = ReadParameters(out _, out _, out _, out _, out _);
                }

                return m_ParametersCache;
            }
            set {
                m_ParametersCache = value;

                if(IsCommitted && !UseCachedParameters) {
                    WriteParameters(value, out _);
                }
            }
        }

        public Transform GetEntityTransformation(ISwSelObject entity) {
            if(m_EntitiesTransformsCache?.TryGetValue(entity, out var transform) == true) {
                return transform;
            } else {
                if(entity is ISwEntity e) {
                    var ownerComp = e.Component;

                    if(ownerComp != null) return ownerComp.Transformation;
                }

                return Transform.Identity;
            }
        }

        internal TParams ReadParameters(out ISwDimension[] dispDims, out string[] dispDimParams, out ISwBody[] editBodies,
            out SelectionInfo[] sels, out CustomFeatureOutdateState_e state) {
            dispDims = null;

            try {
                ExtractRawParameters(out var rawParams, out dispDims, out sels, out editBodies);

                m_ParamsParser.ConvertParameters(typeof(TParams), OwnerDocument, this, ref rawParams, ref sels, ref dispDims, ref editBodies);

                var parameters = (TParams)m_ParamsParser.BuildParameters(typeof(TParams), ref rawParams, ref dispDims, ref editBodies, ref sels, out dispDimParams);

                m_EntitiesTransformsCache = (sels ?? new SelectionInfo[0]).Where(s => s != null)
                    .ToDictionary(s => s.Selection, s => s.Transformation, new XObjectEqualityComparer<ISwSelObject>());

                state = GetState(dispDims);

                m_ParametersCache = parameters;

                return parameters;
            } catch(Exception ex) {
                if(dispDims != null) {
                    foreach(var dim in dispDims.ToSwArray<SwDimension>()) {
                        dim.Dispose();
                    }
                }

                OwnerApplication.Logger.Log(ex);

                throw;
            }
        }

        internal void ApplyParametersCache() {
            if(!IsCommitted)
                throw new Exception("Feature is not committed");

            if(!UseCachedParameters)
                throw new Exception("Feature is not editing");

            if(m_ParametersCache == null)
                throw new Exception("Feature does not have parameters cache");


            WriteParameters(m_ParametersCache, out _);
        }

        protected override IFeature InsertFeature(CancellationToken cancellationToken)
            => InsertComFeatureWithParameters();

        protected override void ValidateDefinitionType() {
            if(!typeof(SwMacroFeatureDefinition<TParams>).IsAssignableFrom(DefinitionType))
                throw new DefinitionTypeMismatch(DefinitionType, typeof(SwMacroFeatureDefinition<TParams>));

        }

        private IFeature InsertComFeatureWithParameters() {

            var parsed = m_ParamsParser.Parse(Parameters);

            SeparateParameters(parsed.Attributes, out string[] paramNames, out int[] paramTypes, out string[] paramValues);

            //TODO: add dim types conversion

            return InsertComFeatureBase(
                paramNames, paramTypes, paramValues,
                parsed.DimTypes?.Select(d => (int)d)?.ToArray(), parsed.DimValues,
                parsed.Selections.ToSwArray<SwSelObject>()?.Select(s => s.Dispatch)?.ToArray(),
                parsed.EditBodies.ToSwArray<SwBody>()?.Select(b => b.Body)?.ToArray());
        }

        private void WriteParameters(object parameters, out CustomFeatureOutdateState_e state) {

            var parsed = m_ParamsParser.Parse(parameters);

            var dispDims = GetDimensions();
            if(dispDims != null && dispDims.Length != parsed.DimValues.Length)
                throw new ParametersMismatchException("Dimensions mismatch");

            state = GetState(dispDims);

            SetParametersToFeature(parsed.Selections, parsed.EditBodies, dispDims, parsed.DimValues, parsed.Attributes);
        }

        private void ExtractRawParameters(out Dictionary<string, object> parameters,
            out ISwDimension[] dimensions, out SelectionInfo[] selection, out ISwBody[] editBodies) {

            var featData = FeatureData;

            featData.GetParameters(out var retParamNames, out var paramTypes, out var retParamValues);
            featData.GetSelections3(out var retSelObj, out var selObjType, out var selMarks, out var selDrViews, out var retCompXforms);

            //TODO: if entity is missing then the order of the retSelObj will be incorrect (null references are always at the end) which may break the indices

            dimensions = GetDimensions();

            var editBodiesObj = featData.EditBodies;

            if(editBodiesObj != null) {
                editBodies = editBodiesObj.ToSwArray<IBody2>()
                    .Select(b => OwnerDocument.CreateObjectFromDispatch<ISwBody>(b)).ToArray();
            } else {
                editBodies = null;
            }

            var paramValues = retParamValues as string[];

            if(retParamNames is string[] paramNames) {
                parameters = new Dictionary<string, object>();

                for(int i = 0; i < paramNames.Length; i++) {
                    object paramValue;

                    switch((swMacroFeatureParamType_e)((int[])paramTypes)[i]) {
                        case swMacroFeatureParamType_e.swMacroFeatureParamTypeInteger:
                            if(!int.TryParse(paramValues[i], out var intVal)) {
                                intVal = int.MinValue;
                            }
                            paramValue = intVal;
                            break;
                        case swMacroFeatureParamType_e.swMacroFeatureParamTypeDouble:
                            if(!double.TryParse(paramValues[i], NumberStyles.Any, CultureInfo.InvariantCulture, out var dblVal)) {
                                dblVal = double.NaN;
                            }
                            paramValue = dblVal;
                            break;
                        case swMacroFeatureParamType_e.swMacroFeatureParamTypeString:
                            paramValue = paramValues[i];
                            break;
                        default:
                            throw new NotSupportedException("Macro feature parameter type is not supported");
                    }

                    parameters.Add(paramNames[i], paramValue);
                }
            } else {
                parameters = null;
            }

            if(retSelObj == null) {
                selection = null;
                return;
            }

            var selObjects = retSelObj.ToSwArray<object>();
            var compXforms = retCompXforms.ToSwArray<object>();

            selection = new SelectionInfo[selObjects.Length];
            for(int i = 0; i < selObjects.Length; i++) {
                var s = selObjects[i];
                if(s != null) {
                    var transform = (IMathTransform)compXforms[i];
                    var matrix = transform != null ? transform.ToXa() : Transform.Identity;
                    selection[i] = new SelectionInfo(OwnerDocument.CreateObjectFromDispatch<SwSelObject>(s), matrix);
                }
            }
        }

        private ISwDimension[] GetDimensions() {
            var dispDimsObj = FeatureData.GetDisplayDimensions().ToSwArray<IDisplayDimension>();

            if(dispDimsObj != null) {
                var dimensions = new ISwDimension[dispDimsObj.Length];

                for(int i = 0; i < dispDimsObj.Length; i++) {
                    var dim = new SwMacroFeatureDimension(dispDimsObj[i], OwnerDocument, OwnerApplication);

                    dimensions[i] = dim;
                    dispDimsObj[i] = null;
                }

                return dimensions;
            } else {
                return null;
            }
        }

        private CustomFeatureOutdateState_e GetState(ISwDimension[] dispDims) {
            var state = CustomFeatureOutdateState_e.UpToDate;

            if(dispDims != null && dispDims.Any(d => d is SwDimensionPlaceholder)) {
                state |= CustomFeatureOutdateState_e.Dimensions;
            }
            return state;
        }

        private Version GetVersion(string name) {

            FeatureData.GetStringByName(name, out string versVal);

            if(!Version.TryParse(versVal, out Version dimsVersion)) {
                dimsVersion = new Version();
            }

            return dimsVersion;
        }

        private void SeparateParameters(CustomFeatureAttribute[] param, out string[] paramNames, out int[] paramTypes, out string[] paramValues) {
            if(param != null) {
                paramNames = new string[param.Length];
                paramTypes = new int[param.Length];
                paramValues = new string[param.Length];

                for(int i = 0; i < param.Length; i++) {
                    paramNames[i] = param[i].Name;

                    var paramType = param[i].Type;

                    if(paramType == typeof(int)) {
                        paramTypes[i] = (int)swMacroFeatureParamType_e.swMacroFeatureParamTypeInteger;
                        paramValues[i] = Convert.ToString(param[i].Value);
                    } else if(paramType == typeof(double)) {
                        paramTypes[i] = (int)swMacroFeatureParamType_e.swMacroFeatureParamTypeDouble;
                        paramValues[i] = Convert.ToString(param[i].Value, CultureInfo.InvariantCulture);
                    } else {
                        paramTypes[i] = (int)swMacroFeatureParamType_e.swMacroFeatureParamTypeString;
                        paramValues[i] = Convert.ToString(param[i].Value);
                    }
                }
            } else {
                paramNames = null;
                paramTypes = null;
                paramValues = null;
            }
        }

        private void SetParametersToFeature(ISwSelObject[] selection, ISwBody[] editBodies,
            ISwDimension[] dims, double[] dimValues, CustomFeatureAttribute[] param) {
            try {
                var featData = FeatureData;

                if(selection != null && selection.Length > 0) {
                    var dispWraps = Array.ConvertAll(selection, s => new DispatchWrapper(((SwSelObject)s).Dispatch));

                    featData.SetSelections2(dispWraps, new int[selection.Length], new IView[selection.Length]);
                } else {
                    featData.SetSelections2(null, null, null);
                }

                if(editBodies != null && editBodies.Length > 0) {
                    featData.EditBodies = Array.ConvertAll(editBodies, b => ((SwBody)b).Body);
                } else {
                    //TODO: this seems to be not working and old edit bodies will still be assigned
                    featData.EditBodies = null;
                }

                if(dims != null) {
                    for(int i = 0; i < dims.Length; i++) {
                        dims[i].Value = dimValues[i];
                        ((SwDimension)dims[i]).Dispose();
                    }
                }

                var state = GetState(dims);

                if(param != null && param.Length > 0) {
                    //macro feature dimensions cannot be changed in the existing feature
                    //reverting the dimensions version
                    if(state.HasFlag(CustomFeatureOutdateState_e.Dimensions)) {
                        var vers = GetVersion(CustomFeatureParametersParser.VERSION_DIMENSIONS_NAME);

                        var paramIndex = Array.FindIndex(param, p => p.Name == CustomFeatureParametersParser.VERSION_DIMENSIONS_NAME);

                        param[paramIndex] = new CustomFeatureAttribute(param[paramIndex].Name, param[paramIndex].Type, vers.ToString());
                    }

                    string[] paramNames;
                    string[] paramValues;
                    int[] paramTypes;

                    SeparateParameters(param, out paramNames, out paramTypes, out paramValues);

                    OwnerApplication.Logger.Log($"Writing macro feature parameters: {string.Join(", ", paramNames)} of types {string.Join(", ", paramTypes)} to values {string.Join(", ", paramValues)}", LoggerMessageSeverity_e.Debug);

                    featData.SetParameters(paramNames, paramTypes, paramValues);

                    UpdateParameters(featData, param);
                }
            } finally {
                if(dims != null) {
                    foreach(SwDimension dim in dims) {
                        dim.Dispose();
                    }
                }
            }
        }

        /// <summary>
        /// Parameters are not updated when SetParameters is called from OnRebuild method, updating one by one fixes the issue
        /// </summary>
        private void UpdateParameters(IMacroFeatureData featData, CustomFeatureAttribute[] param) {
            if(param != null) {
                for(int i = 0; i < param.Length; i++) {
                    var paramName = param[i].Name;
                    var paramType = param[i].Type;

                    if(paramType == typeof(int)) {
                        featData.SetIntegerByName(paramName, (int)param[i].Value);
                    } else if(paramType == typeof(double)) {
                        featData.SetDoubleByName(paramName, (double)param[i].Value);
                    } else {
                        featData.SetStringByName(paramName, Convert.ToString(param[i].Value));
                    }
                }
            }
        }
    }
}