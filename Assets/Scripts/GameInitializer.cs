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
using TurnBasedGame.Core;
using TurnBasedGame.Core.Interfaces;
using TurnBasedGame.Services;

namespace TurnBasedGame
{
    /// <summary>
    /// Главный инициализатор игры
    /// Проверяет DI и запускает базовую логику игры
    /// </summary>
    public class GameInitializer : MonoBehaviour
    {
        [Header("=== НАСТРОЙКИ ИНИЦИАЛИЗАЦИИ ===")]
        [SerializeField] 
        private bool validateDI = true;

        [SerializeField] 
        private bool createTestUnits = true;

        [SerializeField] 
        private bool startGameAutomatically = true;

        // Инжектируемые сервисы
        [Inject] private IGameStateService _gameState;
        [Inject] private IGameEventsService _gameEvents;
        [Inject] private INetworkGameService _networkGame;
        [Inject] private IUnitService _unitService;
        [Inject] private ITurnService _turnService;
        [Inject] private IGameFieldService _gameFieldService;
        [Inject] private GameConfig _gameConfig;

        private void Start()
        {
            LogHeader("GAME INITIALIZER STARTED");

            if (validateDI)
            {
                ValidateDependencyInjection();
            }

            if (createTestUnits)
            {
                CreateInitialUnits();
            }

            if (startGameAutomatically)
            {
                StartGame();
            }

            LogHeader("GAME INITIALIZATION COMPLETE");
        }

        #region Валидация DI

        private void ValidateDependencyInjection()
        {
            LogSection("Validating Dependency Injection");

            bool allValid = true;

            allValid &= CheckService("GameStateService", _gameState);
            allValid &= CheckService("GameEventsService", _gameEvents);
            allValid &= CheckService("NetworkGameService", _networkGame);
            allValid &= CheckService("UnitService", _unitService);
            allValid &= CheckService("TurnService", _turnService);
            allValid &= CheckService("GameFieldService", _gameFieldService);
            allValid &= CheckService("GameConfig", _gameConfig);

            if (allValid)
            {
                LogSuccess("All services injected successfully");
            }
            else
            {
                LogError("Some services failed to inject!");
            }

            if (_gameConfig != null)
            {
                LogInfo($"Game Config: {_gameConfig.FieldWidth}x{_gameConfig.FieldHeight}, {_gameConfig.TurnDuration}s turns");
            }
        }

        private bool CheckService(string serviceName, object service)
        {
            bool isValid = service != null;
            string status = isValid ? "✓" : "✗";
            LogInfo($"{status} {serviceName}: {(isValid ? "OK" : "MISSING")}");
            return isValid;
        }

        #endregion

        #region Инициализация игры

        private void CreateInitialUnits()
        {
            LogSection("Creating Initial Units");

            if (_gameConfig == null || _gameState == null)
            {
                LogError("Cannot create units - missing GameConfig or GameState!");
                return;
            }

            // Создаем тестовые юниты
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

            LogInfo($"Created {_gameState.Units.Count} units");
        }

        private void StartGame()
        {
            LogSection("Starting Game");

            if (_gameState == null)
            {
                LogError("Cannot start game - GameStateService is null!");
                return;
            }

            // Устанавливаем начальное состояние
            var gameState = GameState.Default;
            gameState.phase = GamePhase.Playing;
            gameState.currentPlayer = PlayerId.Player1;
            gameState.canMove = true;
            gameState.canAttack = true;
            gameState.turnTimeLeft = _gameConfig?.TurnDuration ?? 60f;

            _gameState.UpdateGameState(gameState);

            LogSuccess("Game started successfully");
        }

        #endregion

        #region Контекстные методы для отладки

        [ContextMenu("Validate All Services")]
        private void ValidateAllServices()
        {
            ValidateDependencyInjection();
        }

        [ContextMenu("Show Game State")]
        private void ShowGameState()
        {
            if (_gameState == null)
            {
                LogError("GameStateService is null!");
                return;
            }

            var state = _gameState.GameState.CurrentValue;
            LogInfo($"Game Phase: {state.phase}");
            LogInfo($"Current Player: {state.currentPlayer}");
            LogInfo($"Turn: {state.currentTurn}");
            LogInfo($"Time Left: {state.turnTimeLeft:F1}s");
            LogInfo($"Can Move: {state.canMove}");
            LogInfo($"Can Attack: {state.canAttack}");
            LogInfo($"Total Units: {_gameState.Units.Count}");
        }

        [ContextMenu("Check Network Status")]
        private void CheckNetworkStatus()
        {
            if (_networkGame == null)
            {
                LogError("NetworkGameService is null!");
                return;
            }

            LogInfo($"Network Service Type: {_networkGame.GetType().Name}");
            
            if (_networkGame is NetworkGameService networkService)
            {
                LogInfo($"Is Server: {networkService.IsServer}");
                LogInfo($"Is Client: {networkService.IsClient}");
                LogInfo($"Is Spawned: {networkService.IsSpawned}");
            }
        }

        [ContextMenu("Restart Game")]
        private void RestartGame()
        {
            LogHeader("RESTARTING GAME");
            
            CreateInitialUnits();
            StartGame();
        }

        #endregion

        #region Методы логирования

        private void LogHeader(string message)
        {
            Debug.Log($"<color=cyan>==================== {message} ====================</color>");
        }

        private void LogSection(string message)
        {
            Debug.Log($"<color=yellow>--- {message} ---</color>");
        }

        private void LogSuccess(string message)
        {
            Debug.Log($"<color=green>[SUCCESS] {message}</color>");
        }

        private void LogError(string message)
        {
            Debug.Log($"<color=red>[ERROR] {message}</color>");
        }

        private void LogInfo(string message)
        {
            Debug.Log($"<color=white>[INFO] {message}</color>");
        }

        #endregion
    }
}
