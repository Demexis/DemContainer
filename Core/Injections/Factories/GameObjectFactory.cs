using UnityEngine;

namespace DemContainer {
    public interface IGameObjectFactory {
        void Build(GameObject gameObject, bool injectSerializeReferences = false);
    }
    
    public sealed class GameObjectFactory : IGameObjectFactory {
        private readonly IContainerInjector containerInjector;
        private readonly IConstructorDependencyContainer constructorDependencyContainer;

        public GameObjectFactory(IContainerInjector containerInjector, IConstructorDependencyContainer constructorDependencyContainer) {
            this.containerInjector = containerInjector;
            this.constructorDependencyContainer = constructorDependencyContainer;
        }

        public void Build(GameObject gameObject, bool injectSerializeReferences = false) {
            containerInjector.Inject(gameObject)
                             .AsGameObject(builder => {
                                 constructorDependencyContainer.ConstructObject(builder.instanceObject);

                                 if (injectSerializeReferences) {
                                     builder.WithSerializeReferences(injectionBuilder => {
                                         constructorDependencyContainer.ConstructObject(injectionBuilder
                                             .instanceObject);
                                     });
                                 }
                             });
        }
    }
}