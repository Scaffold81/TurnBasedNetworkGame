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

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using R3;
using Cysharp.Threading.Tasks;
using TurnBasedGame.Core;
using TurnBasedGame.Core.Interfaces;
using TurnBasedGame.Core.Events;

namespace TurnBasedGame.Services
{
    /// <summary>
    /// Полная реализация сервиса управления ходами
    /// Поддерживает таймер хода, смену игроков, ограничения действий
    /// </summary>
    public class TurnService : MonoBehaviour, ITurnService
    {
        // Инжектируемые зависимости
        [Inject] private IGameStateService _gameState;
        [Inject] private IGameEventsService _gameEvents;
        [Inject] private INetworkGameService _networkGame;
        [Inject] private IGameRulesService _gameRules;
        [Inject] private GameConfig _gameConfig;

        // Реактивные свойства
        private readonly ReactiveProperty<float> _turnTimer = new();
        private readonly ReactiveProperty<bool> _isMyTurn = new();
        private readonly ReactiveProperty<bool> _canMove = new();
        private readonly ReactiveProperty<bool> _canAttack = new();

        // Публичные реактивные свойства (только для чтения)
        public ReadOnlyReactiveProperty<float> TurnTimer { get; private set; }
        public ReadOnlyReactiveProperty<bool> IsMyTurn { get; private set; }
        public ReadOnlyReactiveProperty<bool> CanMove { get; private set; }
        public ReadOnlyReactiveProperty<bool> CanAttack { get; private set; }

        // Внутреннее состояние
        private bool _isInitialized = false;
        private readonly List<IDisposable> _disposables = new();
        private bool _turnTimerRunning = false;
        private PlayerId _currentPlayer = PlayerId.None;
        private int _currentTurnNumber = 1;
        private float _turnStartTime;
        private bool _moveActionUsed = false;
        private bool _attackActionUsed = false;

        // События Unity
        private void Awake()
        {
            TurnTimer = _turnTimer.ToReadOnlyReactiveProperty();
            IsMyTurn = _isMyTurn.ToReadOnlyReactiveProperty();
            CanMove = _canMove.ToReadOnlyReactiveProperty();
            CanAttack = _canAttack.ToReadOnlyReactiveProperty();
            
            Debug.Log("[TurnService] Initialized with full implementation");
        }

        [Inject]
        public void Initialize()
        {
            if (_isInitialized) return;

            // Подписываемся на изменения состояния игры
            _disposables.Add(_gameState.GameState.Subscribe(OnGameStateChanged));
            
            // Подписываемся на игровые события
            _disposables.Add(_gameEvents.UnitMoved.Subscribe(OnUnitMoved));
            _disposables.Add(_gameEvents.UnitAttacked.Subscribe(OnUnitAttacked));

            // Инициализируем начальное состояние
            InitializeDefaultState();

            _isInitialized = true;
            Debug.Log("[TurnService] Fully initialized with reactive subscriptions");
        }

        private void Update()
        {
            if (_turnTimerRunning && _isMyTurn.CurrentValue)
            {
                UpdateTurnTimer();
            }
        }

        private void OnDestroy()
        {
            StopTurnTimer();
            
            foreach (var disposable in _disposables)
            {
                disposable?.Dispose();
            }
            _disposables.Clear();
        }

        #region Управление ходами

        public void StartTurn(PlayerId playerId)
        {
            Debug.Log($"[TurnService] Starting turn for {playerId}, turn #{_currentTurnNumber}");

            _currentPlayer = playerId;
            var localPlayerId = _networkGame.GetLocalPlayerId();
            var isMyTurn = (playerId == localPlayerId);

            // Обновляем состояния
            _isMyTurn.Value = isMyTurn;
            _moveActionUsed = false;
            _attackActionUsed = false;
            
            UpdateActionAvailability();

            // Запускаем таймер
            var turnDuration = _gameConfig.TurnDuration;
            if (_gameState.GameState.CurrentValue.infiniteSpeedEnabled)
            {
                turnDuration *= 2f; // Увеличиваем время при бесконечной скорости
            }

            StartTurnTimer(turnDuration);

            // Обновляем состояние игры
            var gameState = _gameState.GameState.CurrentValue;
            gameState.currentPlayer = playerId;
            gameState.currentTurn = _currentTurnNumber;
            gameState.turnTimeLeft = turnDuration;
            gameState.canMove = true;
            gameState.canAttack = true;
            _gameState.UpdateGameState(gameState);

            // Публикуем событие
            _gameEvents.PublishTurnChanged(playerId, _currentTurnNumber, turnDuration);

            // Проверяем правила игры
            _gameRules.CheckDrawResolution();

            Debug.Log($"[TurnService] Turn started: Player {playerId}, Time: {turnDuration}s, MyTurn: {isMyTurn}");
        }

        public void EndTurn()
        {
            Debug.Log($"[TurnService] Ending turn for {_currentPlayer}");

            if (!_isMyTurn.CurrentValue)
            {
                Debug.LogWarning("[TurnService] Cannot end turn - not my turn!");
                return;
            }

            // Останавливаем таймер
            StopTurnTimer();

            // Отправляем команду через сеть
            _networkGame.RequestEndTurn();

            // Обновляем локальное состояние
            _isMyTurn.Value = false;
            _canMove.Value = false;
            _canAttack.Value = false;

            Debug.Log("[TurnService] Turn end requested");
        }

        public PlayerId GetCurrentPlayer()
        {
            return _currentPlayer;
        }

        public int GetCurrentTurnNumber()
        {
            return _currentTurnNumber;
        }

        public float GetRemainingTime()
        {
            return _turnTimer.CurrentValue;
        }

        #endregion

        #region Управление таймером

        private void StartTurnTimer(float duration)
        {
            Debug.Log($"[TurnService] Starting turn timer: {duration} seconds");

            _turnStartTime = Time.time;
            _turnTimer.Value = duration;
            _turnTimerRunning = true;
        }

        private void StopTurnTimer()
        {
            Debug.Log("[TurnService] Stopping turn timer");
            _turnTimerRunning = false;
        }

        private void UpdateTurnTimer()
        {
            var elapsed = Time.time - _turnStartTime;
            var remaining = Mathf.Max(0f, _gameConfig.TurnDuration - elapsed);
            
            _turnTimer.Value = remaining;

            // Обновляем состояние игры
            var gameState = _gameState.GameState.CurrentValue;
            gameState.turnTimeLeft = remaining;
            _gameState.UpdateGameState(gameState);

            // Проверяем истечение времени
            if (remaining <= 0f && _isMyTurn.CurrentValue)
            {
                Debug.LogWarning("[TurnService] Turn time expired! Auto-ending turn");
                EndTurn();
            }
        }

        #endregion

        #region Управление действиями

        public void OnMoveActionUsed()
        {
            Debug.Log("[TurnService] Move action used");
            
            _moveActionUsed = true;
            UpdateActionAvailability();
            
            // Проверяем автозавершение хода
            CheckAutoEndTurn();
        }

        public void OnAttackActionUsed()
        {
            Debug.Log("[TurnService] Attack action used");
            
            _attackActionUsed = true;
            UpdateActionAvailability();
            
            // Проверяем автозавершение хода
            CheckAutoEndTurn();
        }

        private void UpdateActionAvailability()
        {
            var canMove = _isMyTurn.CurrentValue && !_moveActionUsed && _turnTimer.CurrentValue > 0f;
            var canAttack = _isMyTurn.CurrentValue && !_attackActionUsed && _turnTimer.CurrentValue > 0f;

            _canMove.Value = canMove;
            _canAttack.Value = canAttack;

            // Обновляем состояние игры
            var gameState = _gameState.GameState.CurrentValue;
            gameState.canMove = canMove;
            gameState.canAttack = canAttack;
            _gameState.UpdateGameState(gameState);

            Debug.Log($"[TurnService] Action availability updated: Move={canMove}, Attack={canAttack}");
        }

        private void CheckAutoEndTurn()
        {
            // Если использованы оба действия, автоматически заканчиваем ход
            if (_moveActionUsed && _attackActionUsed && _isMyTurn.CurrentValue)
            {
                Debug.Log("[TurnService] Both actions used, auto-ending turn");
                
                // Небольшая задержка для визуального эффекта
                AutoEndTurnAsync().Forget();
            }
        }

        private async UniTaskVoid AutoEndTurnAsync()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(1f)); // 1 секунда задержки
            
            if (_isMyTurn.CurrentValue) // Проверяем, что ход еще наш
            {
                EndTurn();
            }
        }

        #endregion

        #region Обработчики событий

        private void OnGameStateChanged(GameState newState)
        {
            Debug.Log($"[TurnService] Game state changed: Phase={newState.phase}, Turn={newState.currentTurn}");

            // Обновляем локальное состояние
            if (newState.currentTurn != _currentTurnNumber)
            {
                _currentTurnNumber = newState.currentTurn;
            }

            if (newState.currentPlayer != _currentPlayer)
            {
                ProcessTurnChange(newState.currentPlayer);
            }

            // Обновляем таймер если нужно
            if (Mathf.Abs(newState.turnTimeLeft - _turnTimer.CurrentValue) > 0.1f)
            {
                _turnTimer.Value = newState.turnTimeLeft;
            }
        }

        private void OnUnitMoved(UnitMovedEvent evt)
        {
            var localPlayerId = _networkGame.GetLocalPlayerId();
            
            if (evt.playerId == localPlayerId && _isMyTurn.CurrentValue)
            {
                Debug.Log($"[TurnService] Local player moved unit {evt.unitId}");
                OnMoveActionUsed();
            }
        }

        private void OnUnitAttacked(UnitAttackedEvent evt)
        {
            var localPlayerId = _networkGame.GetLocalPlayerId();
            
            if (evt.playerId == localPlayerId && _isMyTurn.CurrentValue)
            {
                Debug.Log($"[TurnService] Local player attacked with unit {evt.attackerId}");
                OnAttackActionUsed();
            }
        }

        private void ProcessTurnChange(PlayerId newPlayer)
        {
            Debug.Log($"[TurnService] Processing turn change from {_currentPlayer} to {newPlayer}");

            var wasMyTurn = _isMyTurn.CurrentValue;
            _currentPlayer = newPlayer;
            
            var localPlayerId = _networkGame.GetLocalPlayerId();
            var isMyTurn = (newPlayer == localPlayerId);

            if (wasMyTurn && !isMyTurn)
            {
                // Наш ход закончился
                Debug.Log("[TurnService] Our turn ended");
                StopTurnTimer();
                
                _isMyTurn.Value = false;
                _canMove.Value = false;
                _canAttack.Value = false;
            }
            else if (!wasMyTurn && isMyTurn)
            {
                // Начался наш ход
                Debug.Log("[TurnService] Our turn started");
                StartTurn(newPlayer);
            }
        }

        #endregion

        #region Вспомогательные методы

        private void InitializeDefaultState()
        {
            Debug.Log("[TurnService] Initializing default state");

            var gameState = _gameState.GameState.CurrentValue;
            
            _currentPlayer = gameState.currentPlayer;
            _currentTurnNumber = gameState.currentTurn;
            
            var localPlayerId = _networkGame.GetLocalPlayerId();
            var isMyTurn = (_currentPlayer == localPlayerId);
            
            _isMyTurn.Value = isMyTurn;
            _turnTimer.Value = gameState.turnTimeLeft;
            
            UpdateActionAvailability();

            if (isMyTurn && gameState.phase == GamePhase.Playing)
            {
                _turnTimerRunning = true;
                _turnStartTime = Time.time - (_gameConfig.TurnDuration - gameState.turnTimeLeft);
            }

            Debug.Log($"[TurnService] Default state initialized: Player={_currentPlayer}, Turn={_currentTurnNumber}, MyTurn={isMyTurn}");
        }

        /// <summary>
        /// Принудительная смена хода (для тестирования)
        /// </summary>
        [ContextMenu("Force Next Turn")]
        public void ForceNextTurn()
        {
            if (Application.isEditor)
            {
                Debug.Log("[TurnService] Force next turn (Editor only)");
                
                var nextPlayer = _currentPlayer == PlayerId.Player1 ? PlayerId.Player2 : PlayerId.Player1;
                _currentTurnNumber++;
                
                StartTurn(nextPlayer);
            }
        }

        /// <summary>
        /// Получение статистики хода
        /// </summary>
        public TurnStatistics GetTurnStatistics()
        {
            return new TurnStatistics
            {
                currentPlayer = _currentPlayer,
                turnNumber = _currentTurnNumber,
                remainingTime = _turnTimer.CurrentValue,
                moveActionUsed = _moveActionUsed,
                attackActionUsed = _attackActionUsed,
                isMyTurn = _isMyTurn.CurrentValue,
                canMove = _canMove.CurrentValue,
                canAttack = _canAttack.CurrentValue
            };
        }

        #endregion
    }

    /// <summary>
    /// Статистика текущего хода
    /// </summary>
    public struct TurnStatistics
    {
        public PlayerId currentPlayer;
        public int turnNumber;
        public float remainingTime;
        public bool moveActionUsed;
        public bool attackActionUsed;
        public bool isMyTurn;
        public bool canMove;
        public bool canAttack;
    }
}
