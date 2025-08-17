using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DemContainer {
    public sealed class ContainerInjectorTypeCache {
        public bool checkBase;
        [CanBeNull] public ContainerInjectorTypeCache baseCache;

        public bool containsFieldInfos;
        public IEnumerable<FieldInfo> fieldInfos;
        public bool containsPropertyInfos;
        public IEnumerable<PropertyInfo> propertyInfos;
        public bool containsMethodInfos;
        public IEnumerable<MethodInfo> methodInfos;
    }
    
    public sealed class ContainerInjectorTypesMap {
        private readonly List<Type> cacheSkipTypes = new();
        private readonly Dictionary<Type, ContainerInjectorTypeCache> cacheCheckedTypes = new();

        private readonly ContainerInjectorCache containerInjectorCache;

        public ContainerInjectorTypesMap(ContainerInjectorCache containerInjectorCache) {
            this.containerInjectorCache = containerInjectorCache;
        }

        public bool CheckType(Type type, out ContainerInjectorTypeCache typeCache) {
            if (!cacheCheckedTypes.TryGetValue(type, out typeCache)) {
                cacheCheckedTypes[type] = typeCache = new ContainerInjectorTypeCache();

                typeCache.fieldInfos = containerInjectorCache.GetFields(type);
                typeCache.containsFieldInfos = typeCache.fieldInfos.Count() != 0;
                
                typeCache.propertyInfos = containerInjectorCache.GetProperties(type);
                typeCache.containsPropertyInfos = typeCache.propertyInfos.Count() != 0;
                
                typeCache.methodInfos = containerInjectorCache.GetMethods(type);
                typeCache.containsMethodInfos = typeCache.methodInfos.Count() != 0;

                if (type.BaseType != null) {
                    typeCache.checkBase = CheckType(type.BaseType, out typeCache.baseCache);
                }
                
                if (!typeCache.containsFieldInfos
                    && !typeCache.containsPropertyInfos
                    && !typeCache.containsMethodInfos
                    && !typeCache.checkBase) {
                    cacheSkipTypes.Add(type);
                }
            }

            return !cacheSkipTypes.Contains(type);
        } 
    }
}