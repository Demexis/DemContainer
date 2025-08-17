using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DemContainer {
    public interface IContainerInjector {
        OperationsTableAnalyzer OperationsTableAnalyzer { get; }
        
        /// <summary>
        /// Finds all methods with [Inject] attribute and invokes them by resolving the parameter types.
        /// Supports properties and fields.
        /// Can be customized using the InjectionBuilder. 
        /// </summary>
        /// <param name="instanceObject">Subject of injection.</param>
        /// <returns>InjectionBuilder for customization.</returns>
        InjectionBuilder Inject(object instanceObject);
    }
    
    public sealed class ContainerInjector : IContainerInjector {
        public OperationsTableAnalyzer OperationsTableAnalyzer { get; } = new();
        
        private readonly IContainerResolver containerResolver;
        private readonly HashSet<object> injectedObjects = new();
        private readonly ContainerInjectorCache containerInjectorCache = new();
        private readonly ContainerInjectorTypesMap containerInjectorTypesMap;

        public ContainerInjector(IContainerResolver containerResolver) {
            this.containerResolver = containerResolver;
            containerInjectorTypesMap = new ContainerInjectorTypesMap(containerInjectorCache);
        }

        public InjectionBuilder Inject(object instanceObject) {
            if (injectedObjects.Contains(instanceObject)) {
                return new InjectionBuilder(this, instanceObject);
            }
            injectedObjects.Add(instanceObject);

            InjectInstance(instanceObject);

            return new InjectionBuilder(this, instanceObject);
        }

        private void InjectInstance(object instanceObject) {
            var type = instanceObject.GetType();
            
            while (true) {
                var localType = type;
                using (new OperationTimer("OBJECT_INJECTION", result => { OperationsTableAnalyzer.AddNewRecord(localType.Name, result.elapsed); })) {
                    if (!containerInjectorTypesMap.CheckType(type, out var containerInjectorTypeCache)) {
                        if (!TrySetBaseType()) {
                            break;
                        }
                    }
                    
                    var fieldsWithInjectAttribute = containerInjectorTypeCache.fieldInfos;
                    var propertiesWithInjectAttribute = containerInjectorTypeCache.propertyInfos;
                    var methodsWithInjectAttribute = containerInjectorTypeCache.methodInfos;
                    
                    if (containerInjectorTypeCache.containsFieldInfos) {
                        foreach (var fieldInfo in fieldsWithInjectAttribute) {
                            InjectField(instanceObject, fieldInfo);
                        }
                    }

                    if (containerInjectorTypeCache.containsPropertyInfos) {
                        foreach (var propertyInfo in propertiesWithInjectAttribute) {
                            InjectProperty(instanceObject, propertyInfo);
                        }
                    }

                    if (containerInjectorTypeCache.containsMethodInfos) {
                        foreach (var methodInfo in methodsWithInjectAttribute) {
                            InjectMethod(instanceObject, methodInfo);
                        }
                    }

                    if (!TrySetBaseType()) {
                        break;
                    }
                }
            }

            bool TrySetBaseType() {
                var baseType = type.BaseType;

                if (baseType == null) {
                    return false;
                }

                type = baseType;
                return true;
            }
        }

        private void InjectMethod(object instanceObject, MethodInfo methodInfo) {
            var parameters = methodInfo.GetParameters();
            var args = parameters.Select(x => containerResolver.Resolve(x.ParameterType));

            methodInfo.Invoke(instanceObject, args.ToArray());
        }
        
        private void InjectField(object instanceObject, FieldInfo fieldInfo) {
            var value = containerResolver.Resolve(fieldInfo.FieldType);
            fieldInfo.SetValue(instanceObject, value);
        }
        
        private void InjectProperty(object instanceObject, PropertyInfo propertyInfo) {
            var value = containerResolver.Resolve(propertyInfo.PropertyType);
            propertyInfo.SetValue(instanceObject, value);
        }
    }
}