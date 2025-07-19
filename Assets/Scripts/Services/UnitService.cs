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

using System.Linq;
using UnityEngine;
using Zenject;
using R3;
using TurnBasedGame.Core;
using TurnBasedGame.Core.Interfaces;
using TurnBasedGame.Core.Events;

namespace TurnBasedGame.Services
{
    /// <summary>
    /// Полная реализация сервиса управления юнитами
    /// Поддерживает выбор, планирование и выполнение действий
    /// </summary>
    public class UnitService : IUnitService
    {
        // Инжектируемые зависимости
        [Inject] private IGameStateService _gameState;
        [Inject] private IGameEventsService _gameEvents;
        [Inject] private ITurnService _turnService;
        [Inject] private INetworkGameService _networkGame;
        [Inject] private IPathfindingService _pathfinding;
        [Inject] private IGameFieldService _gameField;
        [Inject] private GameConfig _gameConfig;

        // Реактивные свойства
        private readonly ReactiveProperty<UnitData?> _selectedUnit = new();
        private readonly ReactiveProperty<Vector2Int[]> _movementPath = new();
        private readonly ReactiveProperty<int[]> _attackTargets = new();

        // Публичные реактивные свойства (только для чтения)
        public ReadOnlyReactiveProperty<UnitData?> SelectedUnit { get; private set; }
        public ReadOnlyReactiveProperty<Vector2Int[]> MovementPath { get; private set; }
        public ReadOnlyReactiveProperty<int[]> AttackTargets { get; private set; }

        // Внутреннее состояние
        private bool _isInitialized = false;
        private readonly System.Collections.Generic.List<System.IDisposable> _disposables = new();

        public UnitService()
        {
            SelectedUnit = _selectedUnit.ToReadOnlyReactiveProperty();
            MovementPath = _movementPath.ToReadOnlyReactiveProperty();
            AttackTargets = _attackTargets.ToReadOnlyReactiveProperty();
            
            Debug.Log("[UnitService] Initialized with full implementation");
        }

        [Inject]
        public void Initialize()
        {
            if (_isInitialized) return;

            // Подписываемся на изменения состояния игры
            _disposables.Add(_gameState.GameState.Subscribe(OnGameStateChanged));
            
            // Подписываемся на события игры для отслеживания изменений юнитов
            _disposables.Add(_gameEvents.UnitMoved.Subscribe(evt => OnUnitMoved(evt)));
            _disposables.Add(_gameEvents.UnitAttacked.Subscribe(evt => OnUnitAttacked(evt)));

            // Подписываемся на события смены хода
            _disposables.Add(_gameEvents.TurnChanged.Subscribe(OnTurnChanged));

            _isInitialized = true;
            Debug.Log("[UnitService] Fully initialized with reactive subscriptions");
        }

        #region Выбор и управление юнитами

        public void SelectUnit(int unitId)
        {
            Debug.Log($"[UnitService] Attempting to select unit {unitId}");

            var unit = _gameState.GetUnit(unitId);
            if (!unit.HasValue)
            {
                Debug.LogWarning($"[UnitService] Unit {unitId} not found!");
                return;
            }

            var unitData = unit.Value;
            var localPlayerId = _networkGame.GetLocalPlayerId();

            // Проверяем, принадлежит ли юнит текущему игроку
            if (unitData.owner != localPlayerId)
            {
                Debug.LogWarning($"[UnitService] Cannot select unit {unitId} - belongs to {unitData.owner}, local player is {localPlayerId}");
                return;
            }

            // Проверяем, текущий ли это ход игрока
            if (!_turnService.IsMyTurn.CurrentValue)
            {
                Debug.LogWarning($"[UnitService] Cannot select unit {unitId} - not your turn");
                return;
            }

            // Обновляем выбранный юнит
            _selectedUnit.Value = unitData;
            
            // Очищаем предыдущие планы
            _movementPath.Value = null;
            _attackTargets.Value = null;

            // Вычисляем возможные цели для атаки
            UpdateAttackTargets(unitData);

            // Публикуем событие
            _gameEvents.PublishUnitSelected(unitId, localPlayerId);

            Debug.Log($"[UnitService] Unit {unitId} selected successfully");
        }

        public void DeselectUnit()
        {
            Debug.Log("[UnitService] Deselecting unit");

            var previousUnit = _selectedUnit.Value;
            
            _selectedUnit.Value = null;
            _movementPath.Value = null;
            _attackTargets.Value = null;

            if (previousUnit.HasValue)
            {
                Debug.Log($"[UnitService] Unit {previousUnit.Value.id} deselected");
            }
        }

        #endregion

        #region Планирование действий

        public void PlanMovement(Vector2Int targetPosition)
        {
            Debug.Log($"[UnitService] Planning movement to {targetPosition}");

            if (!_selectedUnit.Value.HasValue)
            {
                Debug.LogWarning("[UnitService] No unit selected for movement planning");
                return;
            }

            var unit = _selectedUnit.Value.Value;

            // Проверяем, может ли юнит двигаться
            if (!CanUnitMove(unit.id))
            {
                Debug.LogWarning($"[UnitService] Unit {unit.id} cannot move this turn");
                return;
            }

            // Вычисляем путь
            var path = CalculatePath(unit.position, targetPosition, unit.speed);
            
            if (path == null || path.Length == 0)
            {
                Debug.LogWarning($"[UnitService] Cannot find valid path to {targetPosition}");
                _movementPath.Value = null;
                _gameEvents.PublishPathPlanned(unit.id, null, false);
                return;
            }

            // Проверяем длину пути
            var pathLength = _pathfinding.CalculatePathLength(path);
            if (pathLength > unit.speed)
            {
                Debug.LogWarning($"[UnitService] Path too long: {pathLength} > {unit.speed}");
                _movementPath.Value = null;
                _gameEvents.PublishPathPlanned(unit.id, path, false);
                return;
            }

            // Сохраняем планируемый путь
            _movementPath.Value = path;

            // Обновляем цели атаки с учетом новой позиции
            UpdateAttackTargetsFromPosition(targetPosition);

            // Публикуем событие
            _gameEvents.PublishPathPlanned(unit.id, path, true);

            Debug.Log($"[UnitService] Movement planned: {path.Length} steps, total length: {pathLength}");
        }

        public void PlanAttack(int targetId)
        {
            Debug.Log($"[UnitService] Planning attack on unit {targetId}");

            if (!_selectedUnit.Value.HasValue)
            {
                Debug.LogWarning("[UnitService] No unit selected for attack planning");
                return;
            }

            var attacker = _selectedUnit.Value.Value;

            // Проверяем, может ли юнит атаковать
            if (!CanUnitAttack(attacker.id))
            {
                Debug.LogWarning($"[UnitService] Unit {attacker.id} cannot attack this turn");
                return;
            }

            // Проверяем, что цель существует
            var target = _gameState.GetUnit(targetId);
            if (!target.HasValue)
            {
                Debug.LogWarning($"[UnitService] Target unit {targetId} not found");
                return;
            }

            var targetData = target.Value;

            // Проверяем, что цель - противник
            if (targetData.owner == attacker.owner)
            {
                Debug.LogWarning($"[UnitService] Cannot attack own unit {targetId}");
                return;
            }

            // Определяем позицию атакующего (с учетом планируемого движения)
            var attackerPosition = _movementPath.Value != null && _movementPath.Value.Length > 0 
                ? _movementPath.Value[_movementPath.Value.Length - 1] 
                : attacker.position;

            // Проверяем дальность
            var distance = Vector2Int.Distance(attackerPosition, targetData.position);
            if (distance > attacker.attackRange)
            {
                Debug.LogWarning($"[UnitService] Target {targetId} out of range: {distance} > {attacker.attackRange}");
                return;
            }

            // Проверяем line of sight (если включено)
            if (_gameConfig.EnableLineOfSight && !_gameField.HasLineOfSight(attackerPosition, targetData.position, attacker.attackRange))
            {
                Debug.LogWarning($"[UnitService] No line of sight to target {targetId}");
                return;
            }

            Debug.Log($"[UnitService] Attack on unit {targetId} planned successfully");
        }

        #endregion

        #region Выполнение действий

        public void ExecuteMove(Vector2Int targetPosition)
        {
            Debug.Log($"[UnitService] Executing move to {targetPosition}");

            if (!_selectedUnit.Value.HasValue)
            {
                Debug.LogWarning("[UnitService] No unit selected for movement");
                return;
            }

            var unit = _selectedUnit.Value.Value;

            // Проверяем финальные условия
            if (!CanUnitMove(unit.id))
            {
                Debug.LogWarning($"[UnitService] Unit {unit.id} cannot move");
                return;
            }

            if (!_turnService.CanMove.CurrentValue)
            {
                Debug.LogWarning("[UnitService] Move action not available this turn");
                return;
            }

            // Используем запланированный путь или вычисляем новый
            var path = _movementPath.Value;
            if (path == null || path.Length == 0 || path[path.Length - 1] != targetPosition)
            {
                Debug.Log("[UnitService] Recalculating path for execution");
                path = CalculatePath(unit.position, targetPosition, unit.speed);
            }

            if (path == null || path.Length == 0)
            {
                Debug.LogError($"[UnitService] Cannot execute move - no valid path to {targetPosition}");
                return;
            }

            // Отправляем команду через сеть
            _networkGame.RequestMoveUnit(unit.id, targetPosition);

            Debug.Log($"[UnitService] Move command sent for unit {unit.id} to {targetPosition}");
        }

        public void ExecuteAttack(int targetId)
        {
            Debug.Log($"[UnitService] Executing attack on unit {targetId}");

            if (!_selectedUnit.Value.HasValue)
            {
                Debug.LogWarning("[UnitService] No unit selected for attack");
                return;
            }

            var attacker = _selectedUnit.Value.Value;

            // Проверяем финальные условия
            if (!CanUnitAttack(attacker.id))
            {
                Debug.LogWarning($"[UnitService] Unit {attacker.id} cannot attack");
                return;
            }

            if (!_turnService.CanAttack.CurrentValue)
            {
                Debug.LogWarning("[UnitService] Attack action not available this turn");
                return;
            }

            // Отправляем команду через сеть
            _networkGame.RequestAttackUnit(attacker.id, targetId);

            Debug.Log($"[UnitService] Attack command sent: unit {attacker.id} -> unit {targetId}");
        }

        #endregion

        #region Вспомогательные методы

        public bool CanUnitMove(int unitId)
        {
            var unit = _gameState.GetUnit(unitId);
            if (!unit.HasValue) return false;

            var unitData = unit.Value;
            var gameState = _gameState.GameState.CurrentValue;

            // Проверяем базовые условия
            if (unitData.owner != _networkGame.GetLocalPlayerId()) return false;
            if (gameState.currentPlayer != unitData.owner) return false;
            if (unitData.hasMovedThisTurn) return false;
            if (!gameState.canMove) return false;

            return true;
        }

        public bool CanUnitAttack(int unitId)
        {
            var unit = _gameState.GetUnit(unitId);
            if (!unit.HasValue) return false;

            var unitData = unit.Value;
            var gameState = _gameState.GameState.CurrentValue;

            // Проверяем базовые условия
            if (unitData.owner != _networkGame.GetLocalPlayerId()) return false;
            if (gameState.currentPlayer != unitData.owner) return false;
            if (unitData.hasAttackedThisTurn) return false;
            if (!gameState.canAttack) return false;

            return true;
        }

        public Vector2Int[] CalculatePath(Vector2Int from, Vector2Int to, int maxDistance)
        {
            Debug.Log($"[UnitService] Calculating path from {from} to {to}, max distance: {maxDistance}");

            // Используем PathfindingService для расчета пути
            var path = _pathfinding.FindPath(from, to, maxDistance);
            
            if (path != null)
            {
                var pathLength = _pathfinding.CalculatePathLength(path);
                Debug.Log($"[UnitService] Path calculated: {path.Length} points, length: {pathLength}");
            }

            return path;
        }

        public int[] GetTargetsInRange(Vector2Int position, int range, PlayerId excludePlayer)
        {
            Debug.Log($"[UnitService] Finding targets in range from {position}, range: {range}, exclude: {excludePlayer}");

            var targets = _gameState.Units
                .Where(unit => unit.owner != excludePlayer)
                .Where(unit => Vector2Int.Distance(position, unit.position) <= range)
                .Where(unit => !_gameConfig.EnableLineOfSight || _gameField.HasLineOfSight(position, unit.position, range))
                .Select(unit => unit.id)
                .ToArray();

            Debug.Log($"[UnitService] Found {targets.Length} targets in range");
            return targets;
        }

        #endregion

        #region Обновление целей атаки

        private void UpdateAttackTargets(UnitData unit)
        {
            var position = unit.position;
            UpdateAttackTargetsFromPosition(position);
        }

        private void UpdateAttackTargetsFromPosition(Vector2Int position)
        {
            if (!_selectedUnit.Value.HasValue) return;

            var unit = _selectedUnit.Value.Value;
            var targets = GetTargetsInRange(position, unit.attackRange, unit.owner);
            
            _attackTargets.Value = targets;
            _gameEvents.PublishAttackTargetsUpdated(unit.id, targets, position);

            Debug.Log($"[UnitService] Attack targets updated: {targets.Length} targets from position {position}");
        }

        #endregion

        #region Обработчики событий

        private void OnGameStateChanged(GameState newState)
        {
            Debug.Log($"[UnitService] Game state changed: Phase={newState.phase}, Turn={newState.currentTurn}");

            // Сбрасываем выбор юнита при смене хода
            if (newState.currentPlayer != _networkGame.GetLocalPlayerId())
            {
                DeselectUnit();
            }
        }

        private void OnTurnChanged(TurnChangedEvent evt)
        {
            Debug.Log($"[UnitService] Turn changed to {evt.newCurrentPlayer}");

            // Сбрасываем состояние юнитов на новый ход
            var localPlayerId = _networkGame.GetLocalPlayerId();
            if (evt.newCurrentPlayer == localPlayerId)
            {
                ResetUnitsForNewTurn(localPlayerId);
            }
        }

        private void OnUnitMoved(UnitMovedEvent evt)
        {
            Debug.Log($"[UnitService] Unit {evt.unitId} moved from {evt.fromPosition} to {evt.toPosition}");
            
            // Обновляем выбранный юнит, если он двигался
            if (_selectedUnit.Value.HasValue && _selectedUnit.Value.Value.id == evt.unitId)
            {
                var updatedUnit = _gameState.GetUnit(evt.unitId);
                if (updatedUnit.HasValue)
                {
                    _selectedUnit.Value = updatedUnit.Value;
                    UpdateAttackTargets(updatedUnit.Value);
                }
            }
        }

        private void OnUnitAttacked(UnitAttackedEvent evt)
        {
            Debug.Log($"[UnitService] Unit {evt.attackerId} attacked unit {evt.targetId}");
            
            // Обновляем выбранный юнит
            if (_selectedUnit.Value.HasValue)
            {
                var selectedId = _selectedUnit.Value.Value.id;
                if (selectedId == evt.attackerId || selectedId == evt.targetId)
                {
                    var updatedUnit = _gameState.GetUnit(selectedId);
                    if (updatedUnit.HasValue)
                    {
                        _selectedUnit.Value = updatedUnit.Value;
                        UpdateAttackTargets(updatedUnit.Value);
                    }
                    else
                    {
                        // Юнит был уничтожен
                        DeselectUnit();
                    }
                }
            }
        }

        private void ResetUnitsForNewTurn(PlayerId playerId)
        {
            Debug.Log($"[UnitService] Resetting units for new turn: {playerId}");

            // Сбрасываем флаги действий для юнитов игрока
            var playerUnits = _gameState.GetUnitsForPlayer(playerId);
            foreach (var unit in playerUnits)
            {
                var updatedUnit = unit;
                updatedUnit.hasMovedThisTurn = false;
                updatedUnit.hasAttackedThisTurn = false;
                _gameState.UpdateUnit(updatedUnit);
            }
        }

        #endregion

        #region Очистка ресурсов

        /// <summary>
        /// Очистка подписок
        /// </summary>
        public void Dispose()
        {
            Debug.Log("[UnitService] Disposing subscriptions");
            
            foreach (var disposable in _disposables)
            {
                disposable?.Dispose();
            }
            _disposables.Clear();
        }

        #endregion
    }
}
