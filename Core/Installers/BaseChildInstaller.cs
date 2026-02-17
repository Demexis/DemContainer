using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DemContainer {
    public abstract class BaseChildInstaller : MonoBehaviour {
        [field: SerializeField] public List<GameObject> InjectableObjects { get; set; } = new();
        [SerializeField, HideInInspector, Obsolete] private GameObject[] injectableObjects;

        public bool IsInstalled { get; private set; }

        private void OnValidate() {
            if (injectableObjects == null) {
                return;
            }

            if (injectableObjects.Length == 0) {
                return;
            }

            InjectableObjects = new List<GameObject>();
            InjectableObjects.AddRange(injectableObjects);
            injectableObjects = Array.Empty<GameObject>();
            EditorUtility.SetDirty(this);
            EditorUtility.SetDirty(gameObject);
            Debug.LogError("SetDirty component: " + GetType().Name);
        }

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

            for (var i = 0; i < InjectableObjects.Count; i++) {
                var obj = InjectableObjects[i];
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