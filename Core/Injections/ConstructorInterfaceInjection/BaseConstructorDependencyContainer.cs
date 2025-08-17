using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DemContainer {
    public interface IConstructorDependencyContainer {
        OperationsTableAnalyzer OperationsTableAnalyzer { get; }
        void ConstructComponents(GameObject gameObject);
        void ConstructObject(Object injectableObject);
        void ConstructObjects(IEnumerable<Object> injectableObjects);
        void ConstructObject(object injectableObject);
        void ConstructObjects(IEnumerable<object> injectableObjects);
    }
    
    public class BaseConstructorDependencyContainer : IConstructorDependencyContainer {
        public OperationsTableAnalyzer OperationsTableAnalyzer { get; } = new();
        
        private readonly List<object> injectedObjects = new();

        private readonly Func<Type, object> resolveSingletonTypeCallback;

        public BaseConstructorDependencyContainer(Func<Type, object> resolveSingletonTypeCallback) {
            this.resolveSingletonTypeCallback = resolveSingletonTypeCallback;
        }

        public void ConstructComponents(GameObject gameObject) {
            var monoBehaviours = gameObject.GetComponentsInChildren<MonoBehaviour>();

            try {
                ConstructObjects(monoBehaviours.ToArray<Object>());
            } catch (Exception) {
                Debug.LogError($"Caught exception on {gameObject.name} constructing.");
                throw;
            }
        }
        
        public void ConstructObjects(IEnumerable<Object> injectableObjects) {
            foreach (var injectableObject in injectableObjects) {
                ConstructObject(injectableObject);
            }
        }
        
        /// <summary>
        /// Resolves the UnityObject and all Serialize References in it.
        /// </summary>
        /// <param name="injectableObject"></param>
        public void ConstructObject(Object injectableObject) {
            ConstructObjects(injectableObject.GetSerializeReferences());
            ConstructObject(injectableObject as object);
        }

        public void ConstructObjects(IEnumerable<object> injectableObjects) {
            foreach (var obj in injectableObjects) {
                ConstructObject(obj);
            }
        }
        
        public void ConstructObject(object injectableObject) {
            using (new OperationTimer("CONSTRUCTOR_DI_TIMER", result => {
                       OperationsTableAnalyzer.AddNewRecord(injectableObject.GetType().Name, result.elapsed);
                   })) {
                if (injectableObject == null) {
                    Debug.LogError("Injectable object is null.");
                    return;
                }
                
                if (injectedObjects.Contains(injectableObject)) {
                    return;
                }

                injectedObjects.Add(injectableObject);
                FindConstructor(injectableObject);
            }
        }

        private static bool IsConstructorInterface(Type type) {
            var typeFullName = type.FullName;
            
            if (typeFullName == null) {
                Debug.LogError("Type full name is null.");
                return false;
            }
            
            return typeFullName.Contains(typeof(IConstructor).FullName!)
                || typeFullName.Contains(typeof(IConstructor<>).FullName!)
                || typeFullName.Contains(typeof(IConstructor<,>).FullName!)
                || typeFullName.Contains(typeof(IConstructor<,,>).FullName!)
                || typeFullName.Contains(typeof(IConstructor<,,,>).FullName!)
                || typeFullName.Contains(typeof(IConstructor<,,,,>).FullName!)
                || typeFullName.Contains(typeof(IConstructor<,,,,,>).FullName!)
                || typeFullName.Contains(typeof(IConstructor<,,,,,,>).FullName!)
                || typeFullName.Contains(typeof(IConstructor<,,,,,,,>).FullName!)
                || typeFullName.Contains(typeof(IConstructor<,,,,,,,,>).FullName!)
                || typeFullName.Contains(typeof(IConstructor<,,,,,,,,,>).FullName!);
        }
        
        private void FindConstructor(object injectableObject) {
            var interfaces = injectableObject.GetType().GetInterfaces();
            
            foreach (var interfaceType in interfaces) {
                if (!IsConstructorInterface(interfaceType)) {
                    continue;
                }
                
                try {
                    RegisterConstructor(injectableObject, interfaceType);
                } catch (Exception ex) {
                    Debug.LogError($"Caught exception on {injectableObject.GetType().Name} construction. Exception: {ex}");
                    throw;
                }
            }
        }

        private void RegisterConstructor(object injectableObject, Type constructorInterfaceType) {
            var methodInfo = constructorInterfaceType.GetMethod(nameof(IConstructor.Construct));

            if (methodInfo == null) {
                Debug.LogError("Ahtung! Method info is null.");
                return;
            }
            
            var genericParameterTypes = constructorInterfaceType.GetGenericArguments();
            
            var parameterObjects = genericParameterTypes.Select(x => resolveSingletonTypeCallback.Invoke(x));
            
            methodInfo.Invoke(injectableObject, parameterObjects.ToArray());
        }
    }
}