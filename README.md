[![GitHub Release](https://img.shields.io/github/v/release/Demexis/DemContainer.svg)](https://github.com/Demexis/Unity-Delegates/releases/latest)
[![MIT license](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
# DemContainer

<table>
  <tr></tr>
  <tr>
    <td colspan="3">Readme Languages:</td>
  </tr>
  <tr></tr>
  <tr>
    <td nowrap width="100">
      <a href="https://github.com/Demexis/DemContainer">
        <span>English</span>
      </a>  
    </td>
    <td nowrap width="100">
      <a href="https://github.com/Demexis/DemContainer/blob/master/README-RU.md">
        <span>Русский</span>
      </a>  
    </td>
  </tr>
</table>

This container is used to inject dependencies between systems and components. It can also be used for regular classes. It was designed as a replacement for the “Service Allocator” pattern implementation.

## Table of Contents
- [Setup](#setup)
  - [Requirements](#requirements)
  - [Installation](#installation)
- [Usage](#usage)
- [Details](#details)
  - [How does it work?](#how-does-it-work)
  - [Registration Callback](#registration-callback)
  - [Lazy Resolving](#lazy-resolving)
  - [Injection Methods](#injection-methods)
  - [Common Factories](#common-factories)
  - [IDisposable Support](#idisposable-support)
  - [Subscription to a Type in a Container](#subscription-to-a-type-in-a-container)
  - [SerializeReference](#serializereference)
  - [Static Installers](#static-installers)
- [Limitations](#constraints)

## Setup

### Requirements

* Unity 2022.2 or later

### Installation

Use __ONE__ of two options:

#### a) Unity Package (Recommended)
Download a unity package from [the latest release](../../releases).

#### b) Package Manager
1. Open Package Manager from Window > Package Manager.
2. Click the "+" button > Add package from git URL.
3. Enter the following URL:
```
https://github.com/Demexis/DemContainer.git
```

Alternatively, open *Packages/manifest.json* and add the following to the dependencies block:

```json
{
    "dependencies": {
        "com.demcontainer": "https://github.com/Demexis/DemContainer.git"
    }
}
```

## Usage

__1) Create a root installer:__

```csharp
public sealed class GameEssentialsRootInstaller : BaseRootInstaller { }
```

__2) Create a child installer, register the necessary types:__

```csharp
using DemContainer;
using UnityEngine;

public sealed class PlayerSystemInstaller : BaseChildInstaller {
    [SerializeField] private GameObject playerPrefab;
    
    protected override void Configure(IContainerRegistrator containerRegistrator) {
        containerRegistrator.Register<IPlayerSystem, PlayerSystem>(_ =>
            new PlayerSystem(playerPrefab));
        
        // If it didn't require a specific instance, 
        // but worked with registered types, you could use:
        // containerRegistrator.Register<IPlayerSystem, PlayerSystem>();
    }
    protected override void StartResolving(IContainerResolver containerResolver, IContainerInjector containerInjector,
        IContainerSubscriptions containerSubscriptions
    ) {
        var playerSystem = containerResolver.Resolve<IPlayerSystem>();
        Debug.Log("Player prefab's name: " + playerSystem.PlayerPrefab.name);
    }
}

public interface IPlayerSystem {
    GameObject PlayerPrefab { get; }
}

public sealed class PlayerSystem : IPlayerSystem {
    public GameObject PlayerPrefab { get; }
    public PlayerSystem(GameObject playerPrefab) { PlayerPrefab = playerPrefab; }
}
```

__3) Add the created installers to the scene, pass the reference of the child installer to the root installer.__

<img width="661" height="556" alt="demcontainer_link_root_and_child_installer" src="https://github.com/user-attachments/assets/e2943ea7-c80e-4796-85cb-0147933526a0" />


__4) Create a component that uses `IPlayerSystem` to spawn the player prefab.__

```csharp
public sealed class SpawnPlayerPoint : MonoBehaviour {
    [Inject] private IPlayerSystem playerSystem;
    
    private void Start() {
        Instantiate(playerSystem.PlayerPrefab, transform.position, transform.rotation);
    }
}
```

__5) Add `SpawnPlayerPoint` to the game object. Specify this object or its parent/ancestor in the _“Injectable Objects”_ array.__

<img width="1081" height="556" alt="demcontainer_link_injectable_objects" src="https://github.com/user-attachments/assets/acc4b6e6-7955-46e3-8d99-b3737a44a8c7" />


__6) Start the scene.__

<img width="1161" height="752" alt="demcontainer_success_scene_launch" src="https://github.com/user-attachments/assets/3e04d5dd-60d0-433b-8b2f-5d9541369faf" />


## Details

### How does it work?

It all starts with the root installer, which does not have a reference to another (parent) root installer.

The `[DefaultExecutionOrder(-5000)]` attribute ensures that `Awake()` and `Start()` for root installers are executed before user scripts.

In `Awake()` of the root installer, all child installers specified in the array are registered. After that, the same is done by another root installer that specified the previous root installer as its parent.

In `Start()` of the root installer, all child installers specified in the array are resolved (instances are created from registrations). An optional virtual method of the child installer is called - `void StartResolving(IContainerResolver containerResolver, IContainerInjector containerInjector, IContainerSubscriptions containerSubscriptions)`. In addition, all necessary dependencies are injected into the game objects of the child installers specified in the “Injectable Objects” array. After that, the same is done by another root installer that specified the previous root installer as its parent.

### Registration Callback

As shown in the example, it allows unregistered data types to be pushed into the constructor.

```csharp
[SerializeField] private GameObject gameObject;
[SerializeField] private string configName;

...
containerRegistrator.Register<IA, A>(_ => new A(gameObject));
containerRegistrator.Register<IB, B>(resolver => new B(configName, resolver.Resolve<A>()));
containerRegistrator.Register<IC, C>(resolver => new C(resolver.Resolve<A>(), resolver.Resolve<B>()));
...
```

If no custom callback was specified in the type registration, then the constructor will be found during resolving and its parameters will be injected. Classes without an explicit constructor always have a default parameterless constructor, so the lack of dependencies is not a problem during resolving.
```csharp
containerRegistrator.Register<ID, D>();
containerRegistrator.Register<IE, E>();
...
    
public interface ID { }
public sealed class D : ID { }

public interface IE { }
public sealed class E : IE {
    private readonly ID d;
    
    public E(ID d) {
        this.d = d;
    }
}
```

### Lazy Resolving

During `Start()`, all types specified in the registrations will be created.

In most cases, automatic type resolving is desirable because some systems and services run in the background, but no one ever directly requests or uses them.

If, for technical reasons, it is necessary to delay the creation of a type until someone actually requires it, you can use the `.Lazy()` method for registrations:
```csharp
containerRegistrator.Register<IPlayerSystem, PlayerSystem>(_ =>
            new PlayerSystem(playerPrefab)).Lazy();
```

### Injection Methods

The example showed how the `SpawnPlayerPoint` component used the `[Inject]` attribute:
```csharp
[Inject] private IPlayerSystem playerSystem;
```
You can also mark properties and methods with the `[Inject]` attribute:
```csharp
// !!! { set; } is required for the property to which [Inject] is applied !!!
[Inject] private IPlayerSystem PlayerSystem { get; set; }
```

```csharp
private IPlayerSystem playerSystem;

[Inject]
private void Construct(IPlayerSystem playerSystem) {
    this.playerSystem = playerSystem;
}
```

Dependencies can also be passed via the `DemContainer.IConstructor` interface:
```csharp
public sealed class SpawnPlayerPoint : MonoBehaviour, IConstructor<IPlayerSystem> {
    public void Construct(IPlayerSystem playerSystem) {
        ...
    }
    ...
}
```

### Common Factories
Let's go back to the example with the player's spawn:
```csharp
private void Start() {
    Instantiate(playerSystem.PlayerPrefab, transform.position, transform.rotation);
}
```
In a real-world scenario, the created game object will be hung with dozens of child objects with hundreds of components, many of which may require dependencies.

It is important to note that these dependencies will not be automatically resolved when using `UnityEngine.Object.Instantiate(...)`. In such cases, you can use one of the three main predefined factories:
* `IGameObjectFactory`
* `IUnityObjectFactory`
* `IObjectFactory`

To recursively inject dependencies into the game object and all its child game objects, use `IGameObjectFactory`:
```csharp
[Inject] private IPlayerSystem playerSystem;
[Inject] private IGameObjectFactory gameObjectFactory;

private void Start() {
    var playerObject = Instantiate(playerSystem.PlayerPrefab, transform.position, transform.rotation);
    gameObjectFactory.Build(playerObject);
}
```

For injection into types derived from `UnityEngine.Object`, use `IUnityObjectFactory`.

For other types derived from `System.Object`, use `IObjectFactory`.

### IDisposable Support

When destroying a game object on which the initial root installer is hanging, the `Dispose()` method is called in the `OnDestroy()` method for all types created by registration that implement the `IDisposable` interface.

Usually, this happens when exiting the game or changing active scene, unless `Object.DontDestroyOnLoad(Object target)` is explicitly used on the object.

The `Dispose()` method can be used to unsubscribe from events of other types, release resources, save configuration to a file, etc.

### Subscription to a Type in a Container

This is an opportunity to capture all types that are, inherit, or implement the specified type. It is typically used in custom PlayerLoops. As a real-world example, you can collect all created systems/services that have data important for game saves:
```csharp
public sealed class SaveServicesInstaller : BaseChildInstaller {
    private IServiceSaver system;
    
    protected override void Configure(IContainerRegistrator containerRegistrator) {
        containerRegistrator.Register<IServiceSaver, ServiceSaver>();
        ...
    }
    protected override void StartResolving(IContainerResolver containerResolver, 
        IContainerInjector containerInjector,
        IContainerSubscriptions containerSubscriptions
    ) {
        system = containerResolver.Resolve<IServiceSaver>();
        containerSubscriptions.Subscribe<ISavableService>(savableService => {
            system.Register(savableService);
        });
    }
}
```
First, all already resolved types are checked. In the future, other resolvable types will also be checked for the specified type (in the example - `ISavableService`).

### SerializeReference

For types marked with the `[Serializable]` attribute and used in components via fields/properties with the `[SerializeReference]` attribute, dependency injection is provided but left under the user's control.

You can inject dependencies into `SerializeReference` using `IGameObjectFactory` and/or `IUnityObjectFactory`, using the `injectSerializeReferences` parameter:
```csharp
gameObjectFactory.Build(playerObject, injectSerializeReferences: true);
```

If there is access to the component, including within itself:
```csharp
public sealed class TestComponentWithSR : MonoBehaviour {
    [SerializeReference] private TestDataSR testDataSR = new();
    [SerializeReference] private List<TestDataSR> listTestDataSR = new() {
        new TestDataSR(),
        new TestDataSR(),
    };
    
    [Inject] private IUnityObjectFactory unityObjectFactory;
    
    private void Start() {
        unityObjectFactory.Build(this, injectSerializeReferences: true);
        
        testDataSR.Test("Injected?");
        
        foreach (var testSR in listTestDataSR) {
            testSR.Test("Injected in list?");
        }
    }
}

[Serializable]
public sealed class TestDataSR {
    [Inject] private IPlayerSystem playerSystem;

    public void Test(string prefix) {
        Debug.Log($"{prefix} ({(playerSystem != null)})");
    }
}
```

```
Injected? (True)
Injected in list? (True)
Injected in list? (True)
```

### Static Installers
Designed for registering global systems or services that must remain created throughout the game, even when the active scene changes.

Registrations in static installers __CANNOT__ be marked with `.Lazy()`.

`IDisposable` does not work for types registered in the static installer. The `Dispose()` method __WON'T__ be called when the game is closed or the scene is changed.

Usage:
1) Create a static root installer:
```csharp
public sealed class StaticGameEssentialsRootInstaller : BaseStaticRootInstaller { }
```

2) Create a static child installer:
```csharp
public sealed class StaticSystemInstaller : BaseStaticChildInstaller {
    [SerializeField] private ScriptableObject scriptableObject;
    
    protected override void Configure(IStaticContainerRegistrator containerRegistrator) {
        containerRegistrator.TryRegister<IStaticScriptableStorage, StaticScriptableStorage>(_ =>
            new StaticScriptableStorage(scriptableObject));
    }
}

public interface IStaticScriptableStorage {
    ScriptableObject ScriptableObject { get; }
}

public sealed class StaticScriptableStorage : IStaticScriptableStorage {
    public ScriptableObject ScriptableObject { get; }

    public StaticScriptableStorage(ScriptableObject scriptableObject) {
        ScriptableObject = scriptableObject;
    }
}
```

3) Use in non-static installers or components:
```csharp
public sealed class WelcomeStorage : MonoBehaviour {
    [Inject] private IStaticScriptableStorage staticScriptableStorage;
    ...
    
    private void Start() {
        Debug.Log($"Hello, {staticScriptableStorage.ScriptableObject.name}!");
        ...
    }
}
```

4) Link them in the inspector, and don't forget to specify the other root installer as the parent.

<img width="1368" height="600" alt="demcontainer_link_static_root_child_installers" src="https://github.com/user-attachments/assets/6769c091-ad19-4172-97d2-2772b77b06c0" />


## Limitations

⚠️ Containers do not support generic types (`<T>`).

⚠️ For the specified game objects (_“Injectable Objects”_), child installers do not inject dependencies into `SerializeReference`.
