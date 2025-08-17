using JetBrains.Annotations;
using System;
using UnityEngine;

namespace DemContainer {
    public sealed class InjectionBuilder {
        public readonly IContainerInjector containerInjector;
        [CanBeNull] public readonly object instanceObject;

        public InjectionBuilder(IContainerInjector containerInjector, [CanBeNull] object instanceObject) {
            this.containerInjector = containerInjector;
            this.instanceObject = instanceObject;
        }

        public InjectionBuilder AsGameObject() {
            return AsGameObject(_ => { });
        }
        
        public InjectionBuilder AsGameObject(Action<InjectionBuilder> internalInjectionBuilder) {
            if (instanceObject is not GameObject gameObject) {
                return this;
            }
            
            var components = gameObject.GetComponentsInChildren<MonoBehaviour>(true);
            foreach (var component in components) {
                if (component == null) {
                    Debug.LogError($"One of the {gameObject.name} components is null.");
                    continue;
                }

                if (!component) {
                    Debug.LogError($"Component {component.GetType().Name} on {gameObject.name} is dead.");
                    continue;
                }
                
                internalInjectionBuilder.Invoke(containerInjector.Inject(component));
            }
            
            return this;
        }
    }
}