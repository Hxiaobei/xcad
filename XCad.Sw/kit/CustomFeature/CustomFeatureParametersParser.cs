using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using XCad;
using XCad.kit.Exceptions;
using XCad.kit.Reflection;
using XCad.kit.Utils;
using XCad.Sw;
using XCad.Sw.Annotations;
using XCad.Sw.Documents;
using XCad.Sw.Features.CustomFeature;
using XCad.Sw.Features.CustomFeature.Attributes;
using XCad.Sw.Features.CustomFeature.Enums;
using XCad.Sw.Features.CustomFeature.Structures;
using XCad.Sw.Geometry;

namespace XCad.kit.CustomFeature {
    /// <summary>
    /// Structured result of parsing custom feature parameters
    /// </summary>
    public class ParseResult {
        public CustomFeatureAttribute[] Attributes { get; }
        public ISwSelObject[] Selections { get; }
        public CustomFeatureDimensionType_e[] DimTypes { get; }
        public double[] DimValues { get; }
        public ISwBody[] EditBodies { get; }

        internal ParseResult(CustomFeatureAttribute[] atts, ISwSelObject[] sels,
            CustomFeatureDimensionType_e[] dimTypes, double[] dimVals, ISwBody[] editBodies) {
            Attributes = atts;
            Selections = sels;
            DimTypes = dimTypes;
            DimValues = dimVals;
            EditBodies = editBodies;
        }
    }

    /// <summary>
    /// Helper utility allowing to parse and convert parameters of the custom feature to the class
    /// </summary>
    public class CustomFeatureParametersParser {
        private class DimensionParamData {
            internal CustomFeatureDimensionType_e Type { get; private set; }
            internal double Value { get; private set; }

            internal DimensionParamData(CustomFeatureDimensionType_e type, double val) {
                Type = type;
                Value = val;
            }
        }

        private class PropertyObject<TObject> {
            internal TObject Object { get; private set; }
            internal string PropertyName { get; private set; }

            internal PropertyObject(string prpName, TObject obj) {
                PropertyName = prpName;
                Object = obj;
            }
        }

        #region Reflection Cache (优化2)

        private enum PropertyCategory {
            Selection,
            Dimension,
            EditBody,
            Data
        }

        private class PropertyMetadata {
            internal PropertyInfo Property { get; set; }
            internal PropertyCategory Category { get; set; }
            internal CustomFeatureDimensionType_e DimensionType { get; set; }
        }

        private static readonly ConcurrentDictionary<Type, PropertyMetadata[]> s_MetadataCache
            = new();

        private static PropertyMetadata[] GetOrBuildMetadata(Type paramsType) {
            return s_MetadataCache.GetOrAdd(paramsType, type => {
                var result = new List<PropertyMetadata>();

                foreach(var prp in type.GetProperties()) {
                    if(prp.TryGetAttribute<ParameterExcludeAttribute>() != null)
                        continue;

                    var prpType = prp.PropertyType;
                    var dimAtt = prp.TryGetAttribute<ParameterDimensionAttribute>();
                    var editBodyAtt = prp.TryGetAttribute<ParameterEditBodyAttribute>();

                    if(dimAtt != null) {
                        result.Add(new PropertyMetadata {
                            Property = prp,
                            Category = PropertyCategory.Dimension,
                            DimensionType = dimAtt.DimensionType
                        });
                    } else if(editBodyAtt != null) {
                        result.Add(new PropertyMetadata {
                            Property = prp,
                            Category = PropertyCategory.EditBody
                        });
                    } else if(typeof(ISwSelObject).IsAssignableFrom(prpType)
                          || typeof(IEnumerable<ISwSelObject>).IsAssignableFrom(prpType)) {
                        result.Add(new PropertyMetadata {
                            Property = prp,
                            Category = PropertyCategory.Selection
                        });
                    } else {
                        if(typeof(IConvertible).IsAssignableFrom(prpType)) {
                            result.Add(new PropertyMetadata {
                                Property = prp,
                                Category = PropertyCategory.Data
                            });
                        } else {
                            throw new NotSupportedException(
                                $"{prp.Name} is not supported as the parameter of macro feature. Currently only types implementing IConvertible are supported (e.g. primitive types, string, DateTime, decimal)");
                        }
                    }
                }

                return result.ToArray();
            });
        }

        #endregion

        /// <summary>
        /// Name of the attribute which is holding version of dimensions
        /// </summary>
        public const string VERSION_DIMENSIONS_NAME = "__dimsVersion";

        /// <summary>
        /// Name of the attribute which is holding version of parameters
        /// </summary>
        public const string VERSION_PARAMETERS_NAME = "__paramsVersion";

        /// <summary>
        /// Singleton instance of the parser (优化4)
        /// </summary>
        public static CustomFeatureParametersParser Instance { get; } = new CustomFeatureParametersParser();

        private readonly FaultObjectFactory m_FaultObjectFactory;

        public CustomFeatureParametersParser() {
            m_FaultObjectFactory = new FaultObjectFactory();
        }

        /// <summary>
        /// Reads the parameters from the feature definition
        /// </summary>
        public object BuildParameters(Type paramsType,
            ref Dictionary<string, object> featPrps,
            ref ISwDimension[] featDims, ref ISwBody[] featEditBodies,
            ref SelectionInfo[] featSels, out string[] dispDimParams) {
            var parameters = featPrps != null
                ? featPrps.Where(kvp => kvp.Key != VERSION_PARAMETERS_NAME && kvp.Key != VERSION_DIMENSIONS_NAME)
                          .ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
                : new Dictionary<string, object>();

            var resParams = Activator.CreateInstance(paramsType);

            var dispDimParamsMap = new SortedDictionary<int, string>();

            var featDimsLocal = featDims ?? [];
            var featEditBodiesLocal = featEditBodies ?? [];
            var featSelsLocal = featSels ?? [];

            TraverseParametersDefinition(resParams,
                (obj, prp) => {
                    AssignObjectsToProperty(obj, featSelsLocal.Select(s => s.Selection).ToArray(), prp, parameters);
                },
                (dimType, obj, prp) => {
                    var dimIndices = GetObjectIndices(prp, parameters);

                    if(dimIndices.Length != 1) {
                        throw new InvalidOperationException(
                            "It could only be one index associated with dimension");
                    }

                    var dimInd = dimIndices.First();

                    if(featDimsLocal.Length > dimInd) {
                        var dispDim = featDimsLocal[dimInd];

                        var val = dispDim.Value;

                        if(!double.IsNaN(val)) {
                            prp.SetValue(obj, val, null);
                        }

                        dispDimParamsMap.Add(dimInd, prp.Name);
                    } else {
                        throw new IndexOutOfRangeException(
                            $"Dimension at index {dimInd} is not present in the macro feature");
                    }
                },
                (obj, prp) => {
                    AssignObjectsToProperty(obj, featEditBodiesLocal, prp, parameters);
                },
                (obj, prp) => {
                    if(TryGetParameterValue(parameters, prp.Name, out object paramVal)) {
                        object val = null;

                        if(paramVal != null) {
                            if(prp.PropertyType.IsEnum) {
                                val = Enum.Parse(prp.PropertyType, paramVal.ToString());
                            } else {
                                val = Convert.ChangeType(paramVal, prp.PropertyType);
                            }
                        }

                        prp.SetValue(obj, val, null);
                    }
                });

            dispDimParams = dispDimParamsMap.Values.ToArray();

            return resParams;
        }

        /// <summary>
        /// Parses the custom feature data from the parameters structure
        /// </summary>
        public ParseResult Parse(object parameters) {
            if(parameters == null) {
                throw new ArgumentNullException(nameof(parameters));
            }

            var paramAttsList = new List<CustomFeatureAttribute>();

            var selectionList = new List<PropertyObject<ISwSelObject>>();
            var dimsList = new List<PropertyObject<DimensionParamData>>();
            var editBodiesList = new List<PropertyObject<ISwBody>>();

            TraverseParametersDefinition(parameters,
                (obj, prp) => {
                    ReadObjectsValueFromProperty(obj, prp, selectionList);
                },
                (dimType, obj, prp) => {
                    var val = Convert.ToDouble(prp.GetValue(obj, null));
                    dimsList.Add(new PropertyObject<DimensionParamData>(
                        prp.Name, new DimensionParamData(dimType, val)));
                },
                (obj, prp) => {
                    ReadObjectsValueFromProperty(obj, prp, editBodiesList);
                },
                (obj, prp) => {
                    var val = prp.GetValue(obj, null);
                    paramAttsList.Add(new CustomFeatureAttribute(prp.Name, prp.PropertyType, val));
                });

            // 版本特性处理
            var versionAttr = parameters.GetType().TryGetAttribute<ParametersVersionAttribute>();
            if(versionAttr != null) {
                void SetVersionParam(string n, Version v) {
                    var versParamIndex = paramAttsList.FindIndex(l => l.Name == n);
                    if(versParamIndex == -1) {
                        paramAttsList.Add(new CustomFeatureAttribute(n, typeof(string), v.ToString()));
                    } else {
                        var curParam = paramAttsList[versParamIndex];
                        paramAttsList[versParamIndex] = new CustomFeatureAttribute(curParam.Name, curParam.Type, v.ToString());
                    }
                }

                SetVersionParam(VERSION_PARAMETERS_NAME, versionAttr.Version);
                SetVersionParam(VERSION_DIMENSIONS_NAME, versionAttr.Version);
            }

            var selection = AddParametersForObjects(selectionList, paramAttsList);
            var dimParams = AddParametersForObjects(dimsList, paramAttsList);
            var editBodies = AddParametersForObjects(editBodiesList, paramAttsList);

            var atts = paramAttsList.ToArray();

            CustomFeatureDimensionType_e[] dimTypes = null;
            double[] dimValues = null;

            if(dimParams != null) {
                dimTypes = dimParams.Select(d => d.Type).ToArray();
                dimValues = dimParams.Select(d => d.Value).ToArray();
            }

            return new ParseResult(atts, selection, dimTypes, dimValues, editBodies);
        }

        /// <summary>
        /// Converts the parameters using the assigned converters
        /// </summary>
        public void ConvertParameters(Type paramsType, ISwDocument doc, ISwMacroFeature feat, ref Dictionary<string, object> parameters,
            ref SelectionInfo[] selection, ref ISwDimension[] dispDims, ref ISwBody[] editBodies) {
            var paramsVersion = new Version();
            var dimsVersion = new Version();

            if(parameters?.Any() == true) {
                foreach(var featRawParam in parameters) {
                    var paramName = featRawParam.Key;
                    var paramVal = featRawParam.Value;

                    if(paramName == VERSION_PARAMETERS_NAME) {
                        paramsVersion = new Version(paramVal.ToString());
                    } else if(paramName == VERSION_DIMENSIONS_NAME) {
                        dimsVersion = new Version(paramVal.ToString());
                    }
                }
            }

            // 简化版本转换器获取
            var versionAttr = paramsType.TryGetAttribute<ParametersVersionAttribute>();
            var versConv = versionAttr?.VersionConverter;
            var curParamVersion = versionAttr?.Version ?? new Version();

            if(curParamVersion != paramsVersion) {
                if(curParamVersion > paramsVersion) {
                    if(versConv != null) {
                        if(versConv.ContainsKey(curParamVersion)) {
                            foreach(var conv in versConv.Where(
                                v => v.Key > paramsVersion && v.Key <= curParamVersion)
                                .OrderBy(v => v.Key)) {
                                conv.Value.Convert(doc, feat, ref parameters, ref selection, ref dispDims, ref editBodies);
                            }
                        } else {
                            throw new NullReferenceException($"{curParamVersion} version of parameters {paramsType.FullName} is not registered");
                        }
                    } else {
                        throw new NullReferenceException("Version converter is not set");
                    }
                } else {
                    throw new FutureVersionParametersException(paramsType, curParamVersion, paramsVersion);
                }
            }
        }

        /// <summary>
        /// Traverses the definition of the parameters class with custom handler for each parameter group.
        /// Uses cached reflection metadata for performance (优化2).
        /// </summary>
        public void TraverseParametersDefinition(object parameters,
                    Action<object, PropertyInfo> selParamHandler,
                    Action<CustomFeatureDimensionType_e, object, PropertyInfo> dimParamHandler,
                    Action<object, PropertyInfo> editBodyHandler,
                    Action<object, PropertyInfo> dataParamHandler) {
            var metadata = GetOrBuildMetadata(parameters.GetType());

            foreach(var meta in metadata) {
                switch(meta.Category) {
                    case PropertyCategory.Selection:
                        selParamHandler.Invoke(parameters, meta.Property);
                        break;
                    case PropertyCategory.Dimension:
                        dimParamHandler.Invoke(meta.DimensionType, parameters, meta.Property);
                        break;
                    case PropertyCategory.EditBody:
                        editBodyHandler.Invoke(parameters, meta.Property);
                        break;
                    case PropertyCategory.Data:
                        dataParamHandler.Invoke(parameters, meta.Property);
                        break;
                }
            }
        }

        private T[] AddParametersForObjects<T>(List<PropertyObject<T>> objects,
            List<CustomFeatureAttribute> paramList)
            where T : class {
            T[] retVal = null;

            if(objects != null && objects.Any()) {
                var allObjects = objects.Select(o => o.Object)
                    .Distinct()
                    .Where(o => o != null).ToList();

                // 优化：建立索引映射，O(n) 替代原 IndexOf 的 O(n²)
                var indexMap = new Dictionary<T, int>();
                for(int i = 0; i < allObjects.Count; i++) {
                    if(!indexMap.ContainsKey(allObjects[i]))
                        indexMap[allObjects[i]] = i;
                }

                var paramsGroup = objects.GroupBy(o => o.PropertyName).ToDictionary(g => g.Key,
                    g => string.Join(",", g.Select(e => e.Object == null ? "-1" : indexMap[e.Object].ToString()).ToArray()));

                paramList.AddRange(paramsGroup.Select(g => new CustomFeatureAttribute(g.Key, typeof(string), g.Value)));

                retVal = allObjects.ToArray();
            }

            return retVal;
        }

        private void AssignObjectsToProperty(object resParams, Array availableObjects,
            PropertyInfo prp, Dictionary<string, object> parameters) {
            var indices = GetObjectIndices(prp, parameters);

            if(indices != null && indices.Any()) {
                availableObjects ??= new object[0];
                object val = null;

                // 优化3: 改进索引越界处理，使用 FaultObject 替代硬抛异常
                if(typeof(IList).IsAssignableFrom(prp.PropertyType)) {
                    var lst = (IList)prp.GetValue(resParams, null);

                    if(lst != null) {
                        lst.Clear();
                    } else {
                        lst = (IList)Activator.CreateInstance(prp.PropertyType);
                    }

                    val = lst;

                    if(indices.Length == 1 && indices.First() == -1) {
                        val = null; //no entities in the list
                    } else {
                        foreach(var obj in indices.Select(i => {
                            object elem;

                            if(i == -1) {
                                elem = null;
                            } else if(i >= availableObjects.Length) {
                                // 优化3: 索引越界时创建占位对象而非抛异常
                                var elemType = prp.PropertyType.GetArgumentsOfGenericType(typeof(IList<>))[0];
                                elem = m_FaultObjectFactory.CreateFaultObject(elemType);
                            } else {
                                elem = availableObjects.GetValue(i);

                                if(elem == null) {
                                    var elemType = prp.PropertyType.GetArgumentsOfGenericType(typeof(IList<>))[0];
                                    elem = m_FaultObjectFactory.CreateFaultObject(elemType);
                                }
                            }

                            return elem;
                        })) {
                            lst.Add(obj);
                        }
                    }
                } else {
                    if(indices.Length > 1) {
                        throw new InvalidOperationException($"Multiple selection indices at {prp.Name} could only be associated with the List");
                    }

                    var index = indices.First();

                    if(index == -1) {
                        val = null;
                    } else if(index >= availableObjects.Length) {
                        // 优化3: 索引越界时创建占位对象而非抛异常
                        val = m_FaultObjectFactory.CreateFaultObject(prp.PropertyType);
                    } else {
                        val = availableObjects.GetValue(index) ?? m_FaultObjectFactory.CreateFaultObject(prp.PropertyType);
                    }
                }

                prp.SetValue(resParams, val, null);
            } else {
                throw new NullReferenceException($"Indices are not set for {prp.PropertyType.Name}");
            }
        }

        /// <summary>
        /// 优化3: 使用 TryParse 替代 Parse，防止损坏数据导致崩溃
        /// </summary>
        private int[] GetObjectIndices(PropertyInfo prp, Dictionary<string, object> parameters) {
            if(!parameters.TryGetValue(prp.Name, out object indValues))
                return [];

            var parts = indValues.ToString().Split(',');
            var result = new List<int>(parts.Length);

            foreach(var part in parts) {
                if(int.TryParse(part.Trim(), out var idx)) {
                    result.Add(idx);
                } else {
                    result.Add(-1); // 无法解析时视为无效索引
                }
            }

            return result.ToArray();
        }

        private bool TryGetParameterValue(Dictionary<string, object> parameters, string name, out object value) {
            if(parameters == null) {
                throw new ArgumentNullException(nameof(parameters));
            }

            return parameters.TryGetValue(name, out value);
        }

        private void ReadObjectsValueFromProperty<T>(object parameters,
                    PropertyInfo prp, List<PropertyObject<T>> list)
                    where T : class {
            var val = prp.GetValue(parameters, null);

            if(val is IList) {
                if((val as IList).Count != 0) {
                    foreach(T lstElem in val as IList) {
                        list.Add(new PropertyObject<T>(prp.Name, lstElem));
                    }
                } else {
                    list.Add(new PropertyObject<T>(prp.Name, null));
                }
            } else {
                list.Add(new PropertyObject<T>(prp.Name, val as T));
            }
        }
    }
}