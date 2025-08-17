using System;

namespace DemContainer {
    public interface IStaticContainerRegistrator {
        IContainerRegistrator ContainerRegistrator { get; }
        bool TryRegister<TInterface, TImplementation>() where TImplementation : TInterface;

        bool TryRegister<TInterface, TImplementation>(Func<IContainerResolver, TImplementation> resolvingCallback)
            where TImplementation : TInterface;
    }
    
    public sealed class StaticContainerRegistrator : IStaticContainerRegistrator {
        public IContainerRegistrator ContainerRegistrator { get; }
        
        public StaticContainerRegistrator(IContainerRegistrator containerRegistrator) {
            ContainerRegistrator = containerRegistrator;
        }

        public bool TryRegister<TInterface, TImplementation>() where TImplementation : TInterface {
            if (ContainerRegistrator.Registrations.ContainsKey(typeof(TInterface))) {
                return false;
            }
            
            ContainerRegistrator.Register<TInterface, TImplementation>();
            return true;
        }
        
        public bool TryRegister<TInterface, TImplementation>(Func<IContainerResolver, TImplementation> resolvingCallback) where TImplementation : TInterface {
            if (ContainerRegistrator.Registrations.ContainsKey(typeof(TInterface))) {
                return false;
            }
            
            ContainerRegistrator.Register<TInterface, TImplementation>(resolvingCallback);
            return true;
        }
    }
}