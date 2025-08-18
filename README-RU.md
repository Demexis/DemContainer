[![GitHub Release](https://img.shields.io/github/v/release/Demexis/DemContainer.svg)](https://github.com/Demexis/Unity-Delegates/releases/latest)
[![MIT license](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
# DemContainer

Данный контейнер используется для инъекции зависимостей между системами и компонентами. Может использоваться и для обычных классов. Был спроектирован как замена использованию имплементации паттерна "Service Allocator".

## Содержание
- [Настройка](#setup)
- [Использование](#usage)
- [Детали](#details)

## Настройка

### Требования

* Unity 2022.2 или позднее

### Установка

Используйте __ОДИН__ из двух вариантов:

#### а) Юнити-пакет (Рекомендуется)
Скачайте юнити-пакет из [последнего релиза](../../releases).

#### б) Менеджер пакетов
1. Откройте Package Manager из Window > Package Manager.
2. Нажмите на кнопку "+" > Add package from git URL.
3. Введите в поле этот URL:
```
https://github.com/Demexis/DemContainer.git
```

Альтернативно, можете открыть *Packages/manifest.json* и добавить туда новую строку в блок "dependencies":

```json
{
    "dependencies": {
        "com.demcontainer": "https://github.com/Demexis/DemContainer.git"
    }
}
```

## Использование

__1) Создайте корневой инсталлер:__

```csharp
public sealed class GameEssentialsRootInstaller : BaseRootInstaller { }
```

__2) Создайте дочерний инсталлер, зарегистрируйте нужные типы:__

```csharp
using DemContainer;
using UnityEngine;

public sealed class PlayerSystemInstaller : BaseChildInstaller {
    [SerializeField] private GameObject playerPrefab;
    
    protected override void Configure(IContainerRegistrator containerRegistrator) {
        containerRegistrator.Register<IPlayerSystem, PlayerSystem>(_ =>
            new PlayerSystem(playerPrefab));
        
        // если бы не требовал конкретный экземпляр, 
        // а работал с регистрируемыми типами, можно было бы использовать:
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

__3) Добавьте созданные инсталлеры на сцену, передайте ссылку на дочерний инсталлер корневому инсталлеру.__

(INSERT IMAGE)

__4) Создайте компонент который использует `IPlayerSystem` чтобы заспавнить префаб игрока.__

```csharp
public sealed class SpawnPlayerPoint : MonoBehaviour {
    [Inject] private IPlayerSystem playerSystem;
    
    private void Start() {
        Instantiate(playerSystem.PlayerPrefab, transform.position, transform.rotation);
    }
}
```

__5) Добавьте `SpawnPlayerPoint` на игровой объект. Укажите этот объект или его родителя/предка в массив _"Injectable Objects"_.__

(INSERT IMAGE)

__6) Запустите сцену.__

(INSERT IMAGE)

## Детали

### Как это работает?

Всё начинается с корневого инсталлера, который не имеет ссылки на другой (родительский) корневой инсталлер. 

Через атрибут `[DefaultExecutionOrder(-5000)]` гарантируется, что `Awake()` и `Start()` для корневых инсталлеров исполняются раньше, чем пользовательские скрипты.

В `Awake()` корневого инсталлера происходит регистрация всех дочерних инсталлеров указанных в массиве. После этого, то же самое делает другой корневой инсталлер который в качестве родителя указывал предыдущий корневой инсталлер.

В `Start()` корневого инсталлера происходит резолвинг (создание экземпляров из регистраций) всех дочерних инсталлеров указанных в массиве. Это происходит путём вызова необязательного виртуального метода дочернего инсталлера - `void StartResolving(IContainerResolver containerResolver, IContainerInjector containerInjector, IContainerSubscriptions containerSubscriptions)`. Помимо этого, также инжектятся все необходимые зависимости в игровые объекты дочерних инсталлеров, указанных в массиве "Injectable Objects". После этого, то же самое делает другой корневой инсталлер который в качестве родителя указывал предыдущий корневой инсталлер.

### Callback регистрации

Как было показано в примере, позволяет пропихнуть в конструктор нерегистрированные типы данных.

```csharp
[SerializeField] private GameObject gameObject;
[SerializeField] private string configName;

...
containerRegistrator.Register<IA, A>(_ => new A(gameObject));
containerRegistrator.Register<IB, B>(resolver => new B(configName, resolver.Resolve<A>()));
containerRegistrator.Register<IC, C>(resolver => new C(resolver.Resolve<A>(), resolver.Resolve<B>()));
...
```

Если в регистрации типа не был указан кастомный калбек, то при резолве будет находится конструктор и инжектится его параметры. Классы без явного конструктора всегда имеют конструктор без параметров по умолчанию, так что отсутствие зависимостей не является проблемой при резолвинге.
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

### "Ленивый" (Lazy) резолвинг

Во время `Start()` создадутся все типы которые были заданы в регистрациях. 

В большинстве случаев, автоматический резолвинг типов желателен, потому что некоторые системы и сервисы работают в фоновом режиме, но напрямую их никто никогда не требует и не использует.

Если по техническим причинам необходимо задержать создание типа до того момента, когда его кто-то потребует, можно воспользоваться методом `.Lazy()` для регистраций:
```csharp
containerRegistrator.Register<IPlayerSystem, PlayerSystem>(_ =>
            new PlayerSystem(playerPrefab)).Lazy();
```

### Поддержка IDisposable

Во время уничтожения игрового объекта на котором висит начальный корневой инсталлер, в методе `OnDestroy()` вызывается метод `Dispose()` для всех созданных по регистрациям типов которые реализуют интерфейс `IDisposable`.

Обычно, этот момент происходит при выходе из игры, а также смене сцены, если на объекте явно не использован `Object.DontDestroyOnLoad(Object target)`.

В методе `Dispose()` могут происходить отписки от ивентов других типов, освобождение ресурсов, сохранение конфигурации в файл, и прочее.

### Подписка на тип в контейнере

Это возможность отлавливать все типы, которые являются, наследуют, или реализуют указанный тип. Обычно применяется в кастомных PlayerLoop-ах. Как реальный пример, можно собирать все создающиеся системы/сервисы которые имеют данные важные для игровых сохранений:
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
Сначала проверяются все уже зарезолвленные типы. В будущем, другие резолвящиеся типы тоже будут проверяться на наличие указанного типа (в примере - `ISavableService`).

### Способы инъекций

В примере было показано, как компонент `SpawnPlayerPoint` использовал атрибут `[Inject]`:
```csharp
[Inject] private IPlayerSystem playerSystem;
```
Можно помечать атрибутом `[Inject]` не только поля, но также свойства и методы:
```csharp
// !!! { set; } обязателен для свойства к которому применяется [Inject] !!!
[Inject] private IPlayerSystem PlayerSystem { get; set; }
```

```csharp
private IPlayerSystem playerSystem;

[Inject]
private void Construct(IPlayerSystem playerSystem) {
    this.playerSystem = playerSystem;
}
```

Зависимости также можно передать по интерфейсу `DemContainer.IConstructor`:
```csharp
public sealed class SpawnPlayerPoint : MonoBehaviour, IConstructor<IPlayerSystem> {
    public void Construct(IPlayerSystem playerSystem) {
        ...
    }
    ...
}
```

### Общие фабрики
Вернёмся к примеру со спавном игрока:
```csharp
private void Start() {
    Instantiate(playerSystem.PlayerPrefab, transform.position, transform.rotation);
}
```
Созданный игровой объект в реальной ситуации будет обвешан десятками дочерних объектов с сотнями компонентами, многие из которых могут требовать зависимости.

Важно подчеркнуть, что эти зависимости не передадутся автоматически при использовании `UnityEngine.Object.Instantiate(...)`. В таких случаях можно воспользоваться одной из трёх основных загатовленных фабрик:
* `IGameObjectFactory`
* `IUnityObjectFactory`
* `IObjectFactory`

Для рекурсивной инъекции зависимостей в компоненты игрового объекта и всех его дочерних игровых объектов, нужно использовать `IGameObjectFactory`:
```csharp
[Inject] private IPlayerSystem playerSystem;
[Inject] private IGameObjectFactory gameObjectFactory;

private void Start() {
    var playerObject = Instantiate(playerSystem.PlayerPrefab, transform.position, transform.rotation);
    gameObjectFactory.Build(playerObject);
}
```

Для инъекции в типы производных от `UnityEngine.Object` используется `IUnityObjectFactory`.

Для остальных типов производных от `System.Object` используется `IObjectFactory`.

### SerializeReference

Для типов помеченных атрибутом `[Serializable]` и использующихся в компонентах через поля/свойства с атрибутом `[SerializeReference]`, инъекция зависимостей предусмотрена, но отдаётся под контроль пользователю.

Внедрить зависимости в `SerializeReference` можно при помощи `IGameObjectFactory` и/или `IUnityObjectFactory`, используя параметр `injectSerializeReferences`:
```csharp
gameObjectFactory.Build(playerObject, injectSerializeReferences: true);
```

Если есть доступ к компоненту, включая внутри себя:
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

### Статические инсталлеры
Предназначены для регистрации глобальных систем или сервисов, которые должны оставаться созданными на протяжении всей игры, даже при смене сцены.

Регистрации в статических инсталлерах __НЕЛЬЗЯ__ пометить `.Lazy()`.

`IDisposable` не работает для типов зарегистрированных через статический инсталлер. Метод `Dispose()` __НЕ БУДЕТ__ вызываться при закрытии игры или смене сцены.

Использование:
1) Создать статический корневой инсталлер:
```csharp
public sealed class StaticGameEssentialsRootInstaller : BaseStaticRootInstaller { }
```

2) Создать статический дочерний инсталлер:
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

3) Использовать в нестатических инсталлерах или компонентах:
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

4) Связать их в инспекторе, и не забыть указать другому корневому инсталлеру в качестве родителя.

(INSERT IMAGE)

## Ограничения

⚠️ Контейнером не поддерживаются обобщённые типы (`<T>`).

⚠️ Для указанных игровых объектов (_"Injectable Objects"_) дочерние инсталлеры не инжектят зависимости в `SerializeReference`.