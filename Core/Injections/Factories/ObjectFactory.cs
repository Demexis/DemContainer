namespace DemContainer {
    public interface IObjectFactory {
        void Build(object obj);
    }
    
    public sealed class ObjectFactory : IObjectFactory {
        private readonly IContainerInjector containerInjector;
        private readonly IConstructorDependencyContainer constructorDependencyContainer;

        public ObjectFactory(IContainerInjector containerInjector, IConstructorDependencyContainer constructorDependencyContainer) {
            this.containerInjector = containerInjector;
            this.constructorDependencyContainer = constructorDependencyContainer;
        }

        public void Build(object obj) {
            containerInjector.Inject(obj);
            constructorDependencyContainer.ConstructObject(obj);
        }
    }
}