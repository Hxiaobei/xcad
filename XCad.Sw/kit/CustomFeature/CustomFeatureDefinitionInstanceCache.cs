using System;
using System.Collections.Generic;
using XCad.Sw.Features.CustomFeature;

namespace XCad.kit.CustomFeature {
    /// <summary>
    /// Manages the cache of custom feature servers
    /// </summary>
    public static class CustomFeatureDefinitionInstanceCache {
        private static readonly Dictionary<Type, ISwMacroFeatureDefinition> m_Instances
            = [];

        /// <summary>
        /// Registers instance of the custom feature server
        /// </summary>
        /// <param name="inst">Instance to register</param>
        public static void RegisterInstance(ISwMacroFeatureDefinition inst) {
            var type = inst.GetType();

            if(m_Instances.ContainsKey(type)) return;

            m_Instances.Add(type, inst);
        }

        /// <summary>
        /// Returns the instance of custom feature server
        /// </summary>
        /// <param name="defType">Type of custom feature definition</param>
        /// <returns>Instance of the custom feature server</returns>
        public static ISwMacroFeatureDefinition GetInstance(Type defType) {
            if(!typeof(ISwMacroFeatureDefinition).IsAssignableFrom(defType)) {
                throw new InvalidCastException($"{defType.FullName} must implement {typeof(ISwMacroFeatureDefinition).FullName}");
            }

            if(!m_Instances.TryGetValue(defType, out ISwMacroFeatureDefinition inst)) {
                var ctor = defType.GetConstructor(Type.EmptyTypes);
                if(ctor == null) {
                    throw new InvalidOperationException(
                        $"Type '{defType.FullName}' must have a public parameterless constructor to be used as a macro feature definition.");
                }

                inst = (ISwMacroFeatureDefinition)Activator.CreateInstance(defType);
                RegisterInstance(inst);
            }

            return inst;
        }
    }
}
