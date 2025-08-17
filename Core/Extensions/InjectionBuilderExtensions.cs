using System;

namespace DemContainer {
    public static class InjectionBuilderExtensions {
        public static InjectionBuilder WithSerializeReferences(this InjectionBuilder injectionBuilder) {
            return WithSerializeReferences(injectionBuilder, _ => { });
        }

        public static InjectionBuilder WithSerializeReferences(this InjectionBuilder injectionBuilder,
            Action<InjectionBuilder> internalInjectionBuilder
        ) {
            if (injectionBuilder.instanceObject is not UnityEngine.Object unityObject) {
                return injectionBuilder;
            }

            var serializeReferences = unityObject.GetSerializeReferences();

            foreach (var serializeReference in serializeReferences) {
                internalInjectionBuilder.Invoke(injectionBuilder.containerInjector.Inject(serializeReference));
            }

            return injectionBuilder;
        }
    }
}