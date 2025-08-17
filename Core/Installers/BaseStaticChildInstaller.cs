using UnityEngine;

namespace DemContainer {
    public abstract class BaseStaticChildInstaller : MonoBehaviour {
        public bool IsInstalled { get; private set; }
        
        public void Register(IStaticContainerRegistrator containerRegistrator) {
            if (IsInstalled) {
                Debug.LogWarning("Already registered installer - " + GetType().Name);
                return;
            }

            IsInstalled = true;

            Configure(containerRegistrator);
        }

        protected abstract void Configure(IStaticContainerRegistrator containerRegistrator);

        public void Resolve(IStaticContainerResolver containerResolver) {
            StartResolving(containerResolver);
        }

        protected virtual void StartResolving(IStaticContainerResolver containerResolver) { }
    }
}