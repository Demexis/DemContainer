using UnityEngine;

namespace DemContainer {
    public interface IUnityObjectFactory {
        void Build(Object unityObject, bool injectSerializeReferences = false);
    }
    
    public sealed class UnityObjectFactory : IUnityObjectFactory {
        private readonly IContainerInjector containerInjector;
        private readonly IConstructorDependencyContainer constructorDependencyContainer;

        public UnityObjectFactory(IContainerInjector containerInjector, IConstructorDependencyContainer constructorDependencyContainer) {
            this.containerInjector = containerInjector;
            this.constructorDependencyContainer = constructorDependencyContainer;
        }

        public void Build(Object unityObject, bool injectSerializeReferences = false) {
            var builder = containerInjector.Inject(unityObject);
            constructorDependencyContainer.ConstructObject(unityObject);

            if (injectSerializeReferences) {
                builder.WithSerializeReferences(injectionBuilder => {
                    constructorDependencyContainer.ConstructObject(injectionBuilder.instanceObject);
                });
            }
        }
    }
}