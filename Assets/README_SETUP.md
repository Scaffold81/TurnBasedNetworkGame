# 🎮 TurnBasedNetworkGame - Setup Instructions (Updated)

## ✅ Исправление ZenjectException для MonoBehaviour сервисов

**❌ Проблема:** ZenjectException при создании NetworkGameService и других MonoBehaviour сервисов.  
**✅ Решение:** Использование `.FromNewComponentOnNewGameObject()` для MonoBehaviour классов.

---

## 🔧 **Настройка проекта для тестирования:**

### **1. Создание GameConfig ScriptableObject**
```
1. В Project Window: 
   Create → TurnBasedGame → Game Config
2. Назовите файл: "DefaultGameConfig"
3. Поместите в: Assets/Resources/
4. Настройте параметры в Inspector
```

### **2. Настройка тестовой сцены**
```
1. Откройте SampleScene
2. Создайте пустой GameObject: "GameManager"
3. Добавьте компоненты:
   - GameInstaller (MonoInstaller)
   - GameInitializer
4. В GameInstaller назначьте GameConfig
5. Сохраните сцену
```

### **3. Настройка Zenject Context**
```
1. GameObject → Zenject → Scene Context
2. В Scene Context добавьте GameInstaller в Mono Installers
```

---

## 🎯 **Что уже работает:**

✅ **Полная DI архитектура** - все сервисы инжектируются через Zenject  
✅ **MonoBehaviour сервисы** - NetworkGameService, InputService, UIReactiveService корректно создаются  
✅ **Реактивные события** - R3 Observable система настроена  
✅ **Заглушки всех сервисов** - логирование для отладки  
✅ **Конфигурируемые настройки** - через GameConfig ScriptableObject  
✅ **Тестовая инициализация** - автоматическое создание тестовых юнитов  
✅ **Context Menu команды** - для ручного тестирования в эдиторе  

---

## 🧪 **Ожидаемые логи при запуске:**

```
[GameInstaller] Installing dependencies...
[GameInstaller] GameConfig bound
[GameInstaller] Core services installed
[GameInstaller] Game services installed
[GameInstaller] UI services installed
[GameInstaller] All dependencies installed successfully!

[GameStateService] Initialized with reactive properties
[GameEventsService] Initialized reactive events system
[NetworkGameService] Started (Stub implementation)
[InputService] Initialized (Stub)
[UIReactiveService] Initialized (Stub)

[GameInitializer] GameStateService: ✓
[GameInitializer] GameEventsService: ✓
[GameInitializer] UnitService: ✓
[GameInitializer] TurnService: ✓
[GameInitializer] NetworkGameService: ✓
[GameInitializer] GameConfig: ✓

[GameStateService] Adding unit 1 (Type: Ranged, Owner: Player1)
[GameStateService] Adding unit 2 (Type: Melee, Owner: Player1)
[GameStateService] Adding unit 3 (Type: Ranged, Owner: Player2)
[GameStateService] Adding unit 4 (Type: Melee, Owner: Player2)
Created 4 test units

[EVENT] Unit 1 selected by Player1
[EVENT] Unit 1 moved from (2, 2) to (3, 3)
[EVENT] Turn changed to Player2, turn #2, time left: 59.0s
```

---

## 🛠️ **Исправленные MonoBehaviour биндинги:**

```csharp
// ❌ Неправильно (вызывает ZenjectException):
Container.Bind<INetworkGameService>().To<NetworkGameService>().AsSingle();

// ✅ Правильно для MonoBehaviour:
Container.Bind<INetworkGameService>().To<NetworkGameService>()
    .FromNewComponentOnNewGameObject().AsSingle();
```

**Исправленные сервисы:**
- `NetworkGameService` (NetworkBehaviour)
- `InputService` (MonoBehaviour)  
- `UIReactiveService` (MonoBehaviour)

---

## 🚀 **Следующие шаги:**

### **Вариант 1:** Реализация NetworkGameService
- Настройка Netcode for GameObjects
- ServerRpc/ClientRpc методы
- Сетевая синхронизация состояния

### **Вариант 2:** Создание визуальных компонентов
- Префабы юнитов
- Игровое поле
- UI интерфейс

### **Вариант 3:** Полная реализация UnitService
- Выбор и управление юнитами
- Планирование пути
- Система атак

---

## 🐛 **Решение проблем:**

### **ZenjectException для MonoBehaviour:**
✅ **Исправлено** - используется `.FromNewComponentOnNewGameObject()`

### **Ошибки компиляции:**
- Проверьте, что все NuGet пакеты установлены (R3, ObservableCollections)
- Убедитесь, что Zenject и UniTask правильно импортированы

### **DI не работает:**
- Убедитесь, что Scene Context настроен
- Проверьте, что GameInstaller добавлен в Mono Installers
- GameConfig должен быть назначен в Inspector

---

## 📝 **Коммит:**

```bash
fix: Resolve ZenjectException for MonoBehaviour services in DI

- Fix NetworkGameService binding with FromNewComponentOnNewGameObject()
- Fix InputService binding for MonoBehaviour dependency injection
- Fix UIReactiveService binding for proper Zenject instantiation
- Add comments explaining MonoBehaviour vs regular class bindings
- All DI services now initialize correctly without exceptions
- Project ready for testing with proper service instantiation
```

---

**🎯 Готово к запуску! Все ZenjectException исправлены, DI работает корректно.**
