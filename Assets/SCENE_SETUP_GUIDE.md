# 🎮 Инструкция по настройке сцены TurnBasedNetworkGame

## 📋 **Пошаговая настройка для тестирования архитектуры**

---

## 🎯 **Шаг 1: Создание GameConfig**

### **1.1 Создание ScriptableObject:**
```
1. В Project Window правый клик
2. Create → TurnBasedGame → Game Config
3. Назовите файл: "DefaultGameConfig"
4. Поместите в папку: Assets/Resources/
```

### **1.2 Настройка параметров GameConfig:**
```
В Inspector для DefaultGameConfig установите:

=== ОСНОВНЫЕ ПРАВИЛА ИГРЫ ===
• Turn Duration: 60
• Draw Resolution Turn: 15
• Enable Infinite Speed: ✓

=== РАЗМЕРЫ ПОЛЯ ===
• Field Width: 20
• Field Height: 20
• Starting Units Per Type: 2

=== ХАРАКТЕРИСТИКИ ЮНИТОВ ===
• Ranged Unit Speed: 2
• Ranged Unit Attack Range: 4
• Melee Unit Speed: 4
• Melee Unit Attack Range: 1

=== ОПЦИОНАЛЬНЫЕ ФУНКЦИИ ===
• Enable Field Streaming: ✓
• Enable Line Of Sight: ✓
• Enable Anti Cheat: ✓
• Use Geometric Targeting: ✓

=== ВИЗУАЛЬНЫЕ НАСТРОЙКИ ===
• Show Movement Path: ✓
• Show Attack Range: ✓
• Show Unit Selection: ✓
```

---

## 🎯 **Шаг 2: Настройка GameManager**

### **2.1 Создание GameManager:**
```
1. В Hierarchy: правый клик → Create Empty
2. Назовите: "GameManager"
3. Position: (0, 0, 0)
```

### **2.2 Добавление компонентов GameManager:**
```
В Inspector для GameManager добавьте компоненты:

1. Add Component → TurnBasedGame.Installers → Game Installer
2. Add Component → TurnBasedGame → Game Initializer
```

### **2.3 Настройка GameInstaller:**
```
В компоненте Game Installer:
• Game Config: перетащите DefaultGameConfig из Assets/Resources/
```

### **2.4 Настройка GameInitializer:**
```
В компоненте Game Initializer:
• Test DI On Start: ✓
• Create Test Units: ✓
```

---

## 🎯 **Шаг 3: Настройка Zenject Scene Context**

### **3.1 Создание Scene Context:**
```
1. В меню: GameObject → Zenject → Scene Context
2. Объект "SceneContext" появится в Hierarchy
```

### **3.2 Настройка Scene Context:**
```
В Inspector для SceneContext:

=== INSTALLERS ===
• Mono Installers: установите Size = 1
• Element 0: перетащите GameManager из Hierarchy

=== PARENT CONTRACT NAMES ===
• Оставьте пустым

=== AUTO RUN ===
• Auto Run: ✓
```

---

## 🎯 **Шаг 4: Настройка камеры (опционально)**

### **4.1 Настройка Main Camera:**
```
В Inspector для Main Camera:
• Position: (10, 10, -10)
• Rotation: (45, 0, 0)
• Projection: Orthographic
• Size: 12
```

---

## 🎯 **Шаг 5: Сохранение и тестирование**

### **5.1 Сохранение сцены:**
```
1. File → Save Scene As...
2. Назовите: "GameTestScene"
3. Сохраните в: Assets/Scenes/
```

### **5.2 Проверка настройки:**
```
В Hierarchy должны быть:
✓ Main Camera
✓ Directional Light  
✓ GameManager (с GameInstaller и GameInitializer)
✓ SceneContext (с ссылкой на GameManager)
```

---

## 🚀 **Шаг 6: Запуск и тестирование**

### **6.1 Запуск игры:**
```
1. Нажмите Play
2. Откройте Console Window (Window → General → Console)
```

### **6.2 Ожидаемые логи при успешном запуске:**
```
[GameInstaller] Installing dependencies...
[GameInstaller] GameConfig bound
[GameInstaller] Core services installed
[GameInstaller] Game services installed
[GameInstaller] UI services installed
[GameInstaller] All dependencies installed successfully!

[GameStateService] Initialized with reactive properties
[GameEventsService] Initialized reactive events system
[PathfindingService] Initialized with A* algorithm
[NetworkGameService] Started (Stub implementation)

=== GAME INITIALIZER STARTED ===
--- Testing Dependency Injection ---
GameStateService: ✓
GameEventsService: ✓
UnitService: ✓
TurnService: ✓
NetworkGameService: ✓
PathfindingService: ✓
GameConfig: ✓

--- Creating Test Units ---
[GameStateService] Adding unit 1 (Type: Ranged, Owner: Player1)
[GameStateService] Adding unit 2 (Type: Melee, Owner: Player1)
[GameStateService] Adding unit 3 (Type: Ranged, Owner: Player2)
[GameStateService] Adding unit 4 (Type: Melee, Owner: Player2)
Created 4 test units

--- Testing Reactive Events ---
[EVENT] Unit 1 selected by Player1
[EVENT] Unit 1 moved from (2, 2) to (3, 3)
[EVENT] Turn changed to Player2, turn #2, time left: 59.0s

=== GAME INITIALIZATION COMPLETE ===
```

---

## 🧪 **Шаг 7: Ручное тестирование**

### **7.1 Context Menu тесты:**
```
В Inspector для GameManager найдите GameInitializer:
• Правый клик → "Test Unit Selection"
• Правый клик → "Test Turn Management"  
• Правый клик → "Test Pathfinding"
• Правый клик → "Show Game State"
```

### **7.2 Ожидаемые результаты Test Pathfinding:**
```
--- Testing Pathfinding Service ---
[PathfindingService] Finding path from (0, 0) to (5, 5)
[PathfindingService] Path found with 6 points, length: 7
Simple path (0,0) -> (5,5): Found 6 points
Path length: 7
Path valid: True
Path points: (0, 0) -> (1, 1) -> (2, 2) -> (3, 3) -> (4, 4) -> (5, 5)
Position (5,5) walkable: True
Position (-1,-1) walkable: False
```

---

## 🐛 **Решение проблем**

### **❌ Ошибка: "GameConfig is null"**
```
Решение:
1. Убедитесь, что DefaultGameConfig создан в Assets/Resources/
2. Проверьте, что он назначен в GameInstaller
3. Перезапустите игру
```

### **❌ Ошибка: "ZenjectException"**
```
Решение:
1. Проверьте, что SceneContext настроен
2. Убедитесь, что GameManager добавлен в Mono Installers
3. Проверьте, что все компоненты добавлены правильно
```

### **❌ Ошибка: "Cannot create test units"**
```
Решение:
1. Проверьте Console на предыдущие ошибки DI
2. Убедитесь, что все сервисы инициализированы
3. Проверьте, что GameConfig загружен
```

### **❌ Нет логов в Console**
```
Решение:
1. Откройте Console Window
2. Убедитесь, что фильтры включены (Info, Warning, Error)
3. Очистите Console и перезапустите игру
```

---

## ✅ **Проверочный чек-лист**

Перед запуском убедитесь:

- [ ] DefaultGameConfig создан в Assets/Resources/
- [ ] GameManager имеет GameInstaller и GameInitializer
- [ ] GameInstaller ссылается на DefaultGameConfig
- [ ] SceneContext создан и настроен
- [ ] GameManager добавлен в Mono Installers SceneContext
- [ ] Test DI On Start включен в GameInitializer
- [ ] Create Test Units включен в GameInitializer
- [ ] Сцена сохранена

---

## 🎉 **Готово!**

После успешной настройки:
- ✅ Все сервисы инициализированы корректно
- ✅ DI система работает без ошибок  
- ✅ Реактивные события функционируют
- ✅ PathfindingService готов к использованию
- ✅ Тестовые юниты созданы
- ✅ Архитектура готова к развитию

**Теперь можно разрабатывать игровую логику поэтапно!** 🚀
