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

        [ContextMenu("Test Unit Selection")]
        private void TestUnitSelection()
        {
            Debug.Log("--- Testing Unit Selection ---");
            
            if (_unitService == null)
            {
                Debug.LogError("UnitService not available!");
                return;
            }

            // Тестируем выбор юнита
            _unitService.SelectUnit(1);
            _unitService.PlanMovement(new Vector2Int(5, 5));
            _unitService.ExecuteMove(new Vector2Int(5, 5));
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
    }
}
