namespace DemContainer {
    public sealed class ConstructorDependencyContainer : BaseConstructorDependencyContainer {
        public ConstructorDependencyContainer(IContainerResolver objectResolver) : base(objectResolver.Resolve) { }
    }
}