/*
 * Copyright (c) 2025 Попыкин Владимир Николаевич
 * All rights reserved.
 * 
 * This software is the exclusive property of Попыкин Владимир Николаевич.
 * No part of this software may be reproduced, distributed, transmitted,
 * modified, or used in any form or by any means without the prior written
 * permission of the copyright owner.
 * 
 * Unauthorized use, reproduction, or distribution of this software is
 * strictly prohibited and may result in severe civil and criminal penalties.
 */

using UnityEngine;
using Zenject;
using R3;
using TurnBasedGame.Core;
using TurnBasedGame.Core.Interfaces;

namespace TurnBasedGame
{
    /// <summary>
    /// Главный инициализатор игры - тестирует DI и заглушки сервисов
    /// </summary>
    public class GameInitializer : MonoBehaviour
    {
        [Header("=== ТЕСТИРОВАНИЕ DI ===")]
        [SerializeField] 
        private bool testDIOnStart = true;

        [SerializeField] 
        private bool createTestUnits = true;

        // Инжектируемые сервисы
        [Inject] private IGameStateService _gameState;
        [Inject] private IGameEventsService _gameEvents;
        [Inject] private IUnitService _unitService;
        [Inject] private ITurnService _turnService;
        [Inject] private INetworkGameService _networkGame;
        [Inject] private IGameFieldService _gameFieldService;
        [Inject] private IPathfindingService _pathfindingService;
        [Inject] private GameConfig _gameConfig;

        private void Start()
        {
            Debug.Log("=== GAME INITIALIZER STARTED ===");

            if (testDIOnStart)
            {
                TestDependencyInjection();
            }

            if (createTestUnits)
            {
                CreateTestUnits();
            }

            TestReactiveEvents();

            Debug.Log("=== GAME INITIALIZATION COMPLETE ===");
        }

        private void TestDependencyInjection()
        {
            Debug.Log("--- Testing Dependency Injection ---");

            // Проверяем, что все сервисы инжектированы
            Debug.Log($"GameStateService: {(_gameState != null ? "✓" : "✗")}");
            Debug.Log($"GameEventsService: {(_gameEvents != null ? "✓" : "✗")}");
            Debug.Log($"UnitService: {(_unitService != null ? "✓" : "✗")}");
            Debug.Log($"TurnService: {(_turnService != null ? "✓" : "✗")}");
            Debug.Log($"NetworkGameService: {(_networkGame != null ? "✓" : "✗")}");
            Debug.Log($"GameFieldService: {(_gameFieldService != null ? "✓" : "✗")}");
            Debug.Log($"PathfindingService: {(_pathfindingService != null ? "✓" : "✗")}");
            Debug.Log($"GameConfig: {(_gameConfig != null ? "✓" : "✗")}");

            if (_gameConfig != null)
            {
                Debug.Log($"Config - Field Size: {_gameConfig.FieldWidth}x{_gameConfig.FieldHeight}");
                Debug.Log($"Config - Turn Duration: {_gameConfig.TurnDuration}s");
                Debug.Log($"Config - Starting Units Per Type: {_gameConfig.StartingUnitsPerType}");
                Debug.Log($"Config - Features: Streaming={_gameConfig.EnableFieldStreaming}, LOS={_gameConfig.EnableLineOfSight}, AntiCheat={_gameConfig.EnableAntiCheat}");
            }
        }

        private void CreateTestUnits()
        {
            Debug.Log("--- Creating Test Units ---");

            if (_gameConfig == null || _gameState == null)
            {
                Debug.LogError("Cannot create test units - missing services!");
                return;
            }

            // Создаем тестовые юниты для обеих сторон
            var unitId = 1;

            // Юниты игрока 1
            var rangedUnit1 = _gameConfig.CreateRangedUnit(unitId++, PlayerId.Player1, new Vector2Int(2, 2));
            var meleeUnit1 = _gameConfig.CreateMeleeUnit(unitId++, PlayerId.Player1, new Vector2Int(3, 2));

            // Юниты игрока 2  
            var rangedUnit2 = _gameConfig.CreateRangedUnit(unitId++, PlayerId.Player2, new Vector2Int(17, 17));
            var meleeUnit2 = _gameConfig.CreateMeleeUnit(unitId++, PlayerId.Player2, new Vector2Int(16, 17));

            // Добавляем в состояние игры
            _gameState.AddUnit(rangedUnit1);
            _gameState.AddUnit(meleeUnit1);
            _gameState.AddUnit(rangedUnit2);
            _gameState.AddUnit(meleeUnit2);

            Debug.Log($"Created {_gameState.Units.Count} test units");
            Debug.Log($"Player 1 units: {_gameState.GetUnitCount(PlayerId.Player1)}");
            Debug.Log($"Player 2 units: {_gameState.GetUnitCount(PlayerId.Player2)}");

            // Устанавливаем начальное состояние игры
            var gameState = GameState.Default;
            gameState.phase = GamePhase.Playing;
            gameState.currentPlayer = PlayerId.Player1;
            _gameState.UpdateGameState(gameState);
        }

        private void TestReactiveEvents()
        {
            Debug.Log("--- Testing Reactive Events ---");

            if (_gameEvents == null)
            {
                Debug.LogError("GameEventsService not available!");
                return;
            }

            // Подписываемся на события для тестирования
            _gameEvents.UnitSelected.Subscribe(evt => 
                Debug.Log($"[EVENT] Unit {evt.unitId} selected by {evt.playerId}")
            );

            _gameEvents.UnitMoved.Subscribe(evt => 
                Debug.Log($"[EVENT] Unit {evt.unitId} moved from {evt.fromPosition} to {evt.toPosition}")
            );

            _gameEvents.TurnChanged.Subscribe(evt => 
                Debug.Log($"[EVENT] Turn changed to {evt.newCurrentPlayer}, turn #{evt.turnNumber}, time left: {evt.turnTimeLeft:F1}s")
            );

            _gameEvents.GameStateChanged.Subscribe(evt => 
                Debug.Log($"[EVENT] Game state changed: Phase={evt.newState.phase}, Turn={evt.newState.currentTurn}, Player={evt.newState.currentPlayer}")
            );

            // Тестируем события
            _gameEvents.PublishUnitSelected(1, PlayerId.Player1);
            _gameEvents.PublishUnitMoved(1, new Vector2Int(2, 2), new Vector2Int(3, 3), PlayerId.Player1);
            _gameEvents.PublishTurnChanged(PlayerId.Player2, 2, 59f);

            Debug.Log("Reactive events test completed");
        }

        [ContextMenu("Test Unit Selection Advanced")]
        private void TestUnitSelectionAdvanced()
        {
            Debug.Log("--- Testing Advanced Unit Selection ---");
            
            if (_unitService == null)
            {
                Debug.LogError("UnitService not available!");
                return;
            }

            // Тест 1: Выбор юнита
            Debug.Log("Test 1: Selecting unit...");
            _unitService.SelectUnit(1);
            
            var selectedUnit = _unitService.SelectedUnit.CurrentValue;
            if (selectedUnit.HasValue)
            {
                Debug.Log($"Selected unit: {selectedUnit.Value.id}, type: {selectedUnit.Value.type}, owner: {selectedUnit.Value.owner}");
            }
            else
            {
                Debug.LogWarning("No unit selected!");
            }

            // Тест 2: Планирование движения
            Debug.Log("Test 2: Planning movement...");
            _unitService.PlanMovement(new Vector2Int(5, 5));
            
            var movementPath = _unitService.MovementPath.CurrentValue;
            if (movementPath != null && movementPath.Length > 0)
            {
                Debug.Log($"Movement path planned: {movementPath.Length} steps");
                Debug.Log($"Path: {string.Join(" -> ", movementPath)}");
            }
            else
            {
                Debug.LogWarning("No movement path planned!");
            }

            // Тест 3: Поиск целей для атаки
            Debug.Log("Test 3: Finding attack targets...");
            var attackTargets = _unitService.AttackTargets.CurrentValue;
            if (attackTargets != null && attackTargets.Length > 0)
            {
                Debug.Log($"Attack targets found: {attackTargets.Length} targets");
                Debug.Log($"Target IDs: {string.Join(", ", attackTargets)}");
            }
            else
            {
                Debug.Log("No attack targets available");
            }

            // Тест 4: Проверка возможностей
            Debug.Log("Test 4: Checking unit capabilities...");
            if (selectedUnit.HasValue)
            {
                var canMove = _unitService.CanUnitMove(selectedUnit.Value.id);
                var canAttack = _unitService.CanUnitAttack(selectedUnit.Value.id);
                Debug.Log($"Unit {selectedUnit.Value.id} can move: {canMove}, can attack: {canAttack}");
            }

            // Тест 5: Отмена выбора
            Debug.Log("Test 5: Deselecting unit...");
            _unitService.DeselectUnit();
            
            var deselectedUnit = _unitService.SelectedUnit.CurrentValue;
            Debug.Log($"Unit deselected: {!deselectedUnit.HasValue}");

            Debug.Log("Advanced unit selection tests completed");
        }

        [ContextMenu("Test Turn Management")]
        private void TestTurnManagement()
        {
            Debug.Log("--- Testing Turn Management ---");
            
            if (_turnService == null)
            {
                Debug.LogError("TurnService not available!");
                return;
            }

            Debug.Log($"Current Player: {_turnService.GetCurrentPlayer()}");
            Debug.Log($"Current Turn: {_turnService.GetCurrentTurnNumber()}");
            Debug.Log($"Time Left: {_turnService.GetRemainingTime():F1}s");
            Debug.Log($"Is My Turn: {_turnService.IsMyTurn.CurrentValue}");
            Debug.Log($"Can Move: {_turnService.CanMove.CurrentValue}");
            Debug.Log($"Can Attack: {_turnService.CanAttack.CurrentValue}");

            _turnService.EndTurn();
        }

        [ContextMenu("Show Game State")]
        private void ShowGameState()
        {
            Debug.Log("--- Current Game State ---");
            
            if (_gameState == null)
            {
                Debug.LogError("GameStateService not available!");
                return;
            }

            var state = _gameState.GameState.CurrentValue;
            Debug.Log($"Phase: {state.phase}");
            Debug.Log($"Turn: {state.currentTurn}");
            Debug.Log($"Current Player: {state.currentPlayer}");
            Debug.Log($"Can Move: {state.canMove}");
            Debug.Log($"Can Attack: {state.canAttack}");
            Debug.Log($"Time Left: {state.turnTimeLeft:F1}s");
            Debug.Log($"Infinite Speed: {state.infiniteSpeedEnabled}");
            Debug.Log($"Total Units: {_gameState.Units.Count}");

            foreach (var unit in _gameState.Units)
            {
                Debug.Log($"  Unit {unit.id}: {unit.type} owned by {unit.owner} at {unit.position}");
            }
        }

        [ContextMenu("Test Pathfinding")]
        private void TestPathfinding()
        {
            Debug.Log("--- Testing Pathfinding Service ---");
            
            if (_pathfindingService == null)
            {
                Debug.LogError("PathfindingService not available!");
                return;
            }

            // Тест 1: Простой путь
            var path1 = _pathfindingService.FindPath(new Vector2Int(0, 0), new Vector2Int(5, 5));
            Debug.Log($"Simple path (0,0) -> (5,5): {(path1 != null ? $"Found {path1.Length} points" : "No path")}");
            
            if (path1 != null)
            {
                Debug.Log($"Path length: {_pathfindingService.CalculatePathLength(path1)}");
                Debug.Log($"Path valid: {_pathfindingService.IsPathValid(path1, 10)}");
                
                // Выводим путь
                var pathStr = string.Join(" -> ", path1);
                Debug.Log($"Path points: {pathStr}");
            }

            // Тест 2: Путь с ограничением дистанции
            var path2 = _pathfindingService.FindPath(new Vector2Int(0, 0), new Vector2Int(10, 10), 5);
            Debug.Log($"Limited path (0,0) -> (10,10) max 5: {(path2 != null ? $"Found {path2.Length} points" : "No path - too far")}");

            // Тест 3: Проверка проходимости
            var walkable1 = _pathfindingService.IsPositionWalkable(new Vector2Int(5, 5));
            var walkable2 = _pathfindingService.IsPositionWalkable(new Vector2Int(-1, -1));
            Debug.Log($"Position (5,5) walkable: {walkable1}");
            Debug.Log($"Position (-1,-1) walkable: {walkable2}");

            // Тест 4: Путь с избеганием юнитов
            var unitPositions = new Vector2Int[] { new Vector2Int(2, 2), new Vector2Int(3, 3) };
            var path3 = _pathfindingService.FindPathAvoidingUnits(new Vector2Int(1, 1), new Vector2Int(4, 4), unitPositions);
            Debug.Log($"Path avoiding units: {(path3 != null ? $"Found {path3.Length} points" : "No path")}");

            Debug.Log("Pathfinding tests completed");
        }

        [ContextMenu("Test Game Field")]
        private void TestGameField()
        {
            Debug.Log("--- Testing Game Field Service ---");
            
            if (_gameFieldService == null)
            {
                Debug.LogError("GameFieldService not available!");
                return;
            }

            // Тест 1: Генерация поля
            _gameFieldService.GenerateField(15, 15);
            var field = _gameFieldService.GetField();
            Debug.Log($"Generated field: {field.width}x{field.height}");
            Debug.Log($"Player 1 spawns: {field.player1Spawns.Length} positions");
            Debug.Log($"Player 2 spawns: {field.player2Spawns.Length} positions");

            // Тест 2: Проверка позиций
            var validPos = _gameFieldService.IsValidPosition(new Vector2Int(5, 5));
            var invalidPos = _gameFieldService.IsValidPosition(new Vector2Int(-1, -1));
            Debug.Log($"Position (5,5) valid: {validPos}");
            Debug.Log($"Position (-1,-1) valid: {invalidPos}");

            // Тест 3: Препятствия
            _gameFieldService.AddObstacle(new Vector2Int(7, 7));
            var hasObstacle = _gameFieldService.IsObstacleAt(new Vector2Int(7, 7));
            var noObstacle = _gameFieldService.IsObstacleAt(new Vector2Int(8, 8));
            Debug.Log($"Obstacle at (7,7): {hasObstacle}");
            Debug.Log($"Obstacle at (8,8): {noObstacle}");

            // Тест 4: Поиск пути
            var path = _gameFieldService.FindPath(new Vector2Int(1, 1), new Vector2Int(10, 10));
            Debug.Log($"Path from (1,1) to (10,10): {(path != null ? $"Found {path.Length} points" : "No path")}");

            // Тест 5: Line of Sight
            var los1 = _gameFieldService.HasLineOfSight(new Vector2Int(1, 1), new Vector2Int(5, 5), 10);
            var los2 = _gameFieldService.HasLineOfSight(new Vector2Int(1, 1), new Vector2Int(7, 7), 10); // Через препятствие
            Debug.Log($"LOS (1,1) -> (5,5): {los1}");
            Debug.Log($"LOS (1,1) -> (7,7) through obstacle: {los2}");

            // Тест 6: Позиции спавна
            var player1Spawns = _gameFieldService.GetSpawnPositions(PlayerId.Player1);
            var player2Spawns = _gameFieldService.GetSpawnPositions(PlayerId.Player2);
            Debug.Log($"Player 1 spawn positions: {string.Join(", ", player1Spawns)}");
            Debug.Log($"Player 2 spawn positions: {string.Join(", ", player2Spawns)}");

            // Тест 7: Статистика поля
            _gameFieldService.PrintFieldStatistics();

            Debug.Log("Game field tests completed");
        }
    }
}
