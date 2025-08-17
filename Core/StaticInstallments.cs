using System;

namespace DemContainer {
    public static class StaticInstallments {
        public static IContainerRegistrator ContainerRegistrator {
            get {
                InitializeIfNecessary();
                return containerRegistrator;
            } 
            private set => containerRegistrator = value;
        }

        private static IContainerRegistrator containerRegistrator;

        public static IContainerResolver ContainerResolver {
            get {
                InitializeIfNecessary();
                return containerResolver;
            }
            private set => containerResolver = value;
        }
        private static IContainerResolver containerResolver;
        private static bool Initialized { get; set; } = false;

        public static bool TryRegister<TInterface, TImplementation>() where TImplementation : TInterface {
            InitializeIfNecessary();
            if (ContainerRegistrator.Registrations.ContainsKey(typeof(TInterface))) {
                return false;
            }
            
            ContainerRegistrator.Register<TInterface, TImplementation>();
            return true;
        }
        
        public static bool TryRegister<TInterface, TImplementation>(Func<IContainerResolver, TImplementation> resolvingCallback) where TImplementation : TInterface {
            InitializeIfNecessary();
            if (ContainerRegistrator.Registrations.ContainsKey(typeof(TInterface))) {
                return false;
            }
            
            ContainerRegistrator.Register<TInterface, TImplementation>(resolvingCallback);
            return true;
        }

        public static T Resolve<T>() {
            return ContainerResolver.Resolve<T>();
        }

        private static void InitializeIfNecessary() {
            if (Initialized) {
                return;
            }

            Initialized = true;

            ContainerRegistrator = new ContainerRegistrator();
            ContainerResolver = new ContainerResolver(ContainerRegistrator);
        }
    }
}