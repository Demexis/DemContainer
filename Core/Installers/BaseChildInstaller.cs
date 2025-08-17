using UnityEngine;

namespace DemContainer {
    public abstract class BaseChildInstaller : MonoBehaviour {
        public GameObject[] InjectableObjects {
            get => injectableObjects;
            set => injectableObjects = value;
        }
        [SerializeField] private GameObject[] injectableObjects;

        public bool IsInstalled { get; private set; }
        
        public void Register(IContainerRegistrator containerRegistrator) {
            if (IsInstalled) {
                Debug.LogWarning("Already registered installer - " + GetType().Name);
                return;
            }

            IsInstalled = true;

            Configure(containerRegistrator);
        }

        protected abstract void Configure(IContainerRegistrator containerRegistrator);

        public void Resolve(IContainerResolver containerResolver, IContainerInjector containerInjector, IContainerSubscriptions containerSubscriptions) {
            StartResolving(containerResolver, containerInjector, containerSubscriptions);

            var gameObjectFactory = containerResolver.Resolve<IGameObjectFactory>();

            for (var i = 0; i < injectableObjects.Length; i++) {
                var obj = injectableObjects[i];
                if (obj == null) {
                    Debug.LogError($"Null object (Index = {i}) in {GetType().Name} ({gameObject.name}).");
                    continue;
                }
                gameObjectFactory.Build(obj);
            }
        }

        protected virtual void StartResolving(IContainerResolver containerResolver,
            IContainerInjector containerInjector, IContainerSubscriptions containerSubscriptions
        ) { }
    }
}