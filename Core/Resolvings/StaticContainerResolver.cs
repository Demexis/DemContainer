namespace DemContainer {
    public interface IStaticContainerResolver {
        IContainerResolver ContainerResolver { get; }
        
        T Resolve<T>();
    }
    
    public sealed class StaticContainerResolver : IStaticContainerResolver {
        public IContainerResolver ContainerResolver { get; }

        public StaticContainerResolver(IContainerResolver containerResolver) {
            ContainerResolver = containerResolver;
        }

        public T Resolve<T>() {
            return ContainerResolver.Resolve<T>();
        }
    }
}