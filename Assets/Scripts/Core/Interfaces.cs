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

using System.Collections.Generic;
using UnityEngine;
using R3;
using ObservableCollections;
using TurnBasedGame.Core.Events;

namespace TurnBasedGame.Core.Interfaces
{
    /// <summary>
    /// Основной сервис состояния игры с реактивными свойствами
    /// </summary>
    public interface IGameStateService
    {
        // Реактивные свойства (только для чтения)
        ReadOnlyReactiveProperty<GameState> GameState { get; }
        IReadOnlyObservableList<UnitData> Units { get; }
        Observable<GameEvent> GameEvents { get; }

        // Методы изменения состояния
        void UpdateGameState(GameState newState);
        void UpdateUnit(UnitData unit);
        void RemoveUnit(int unitId);
        void AddUnit(UnitData unit);
        
        // Вспомогательные методы
        UnitData? GetUnit(int unitId);
        UnitData[] GetUnitsForPlayer(PlayerId playerId);
        int GetUnitCount(PlayerId playerId);
    }

    /// <summary>
    /// Сетевой сервис для мультиплеера
    /// </summary>
    public interface INetworkGameService
    {
        // Реактивные события от сети
        Observable<GameState> GameStateChanged { get; }
        Observable<UnitData> UnitChanged { get; }
        Observable<int> UnitRemoved { get; }

        // Сетевые команды (ServerRpc)
        void RequestMoveUnit(int unitId, Vector2Int targetPosition);
        void RequestAttackUnit(int attackerId, int targetId);
        void RequestEndTurn();
        
        // Информация о соединении
        bool IsServer { get; }
        bool IsClient { get; }
        PlayerId GetLocalPlayerId();
    }

    /// <summary>
    /// Сервис управления юнитами и их логикой
    /// </summary>
    public interface IUnitService
    {
        // Реактивные свойства
        ReadOnlyReactiveProperty<UnitData?> SelectedUnit { get; }
        ReadOnlyReactiveProperty<Vector2Int[]> MovementPath { get; }
        ReadOnlyReactiveProperty<int[]> AttackTargets { get; }

        // Управление выбором
        void SelectUnit(int unitId);
        void DeselectUnit();

        // Планирование действий
        void PlanMovement(Vector2Int targetPosition);
        void PlanAttack(int targetId);
        
        // Выполнение действий
        void ExecuteMove(Vector2Int targetPosition);
        void ExecuteAttack(int targetId);
        
        // Вспомогательные методы
        bool CanUnitMove(int unitId);
        bool CanUnitAttack(int unitId);
        Vector2Int[] CalculatePath(Vector2Int from, Vector2Int to, int maxDistance);
        int[] GetTargetsInRange(Vector2Int position, int range, PlayerId excludePlayer);
    }

    /// <summary>
    /// Сервис управления ходами
    /// </summary>
    public interface ITurnService
    {
        // Реактивные свойства
        ReadOnlyReactiveProperty<float> TurnTimer { get; }
        ReadOnlyReactiveProperty<bool> IsMyTurn { get; }
        ReadOnlyReactiveProperty<bool> CanMove { get; }
        ReadOnlyReactiveProperty<bool> CanAttack { get; }

        // Управление ходом
        void EndTurn();
        void StartTurn(PlayerId playerId);
        
        // Информация
        PlayerId GetCurrentPlayer();
        int GetCurrentTurnNumber();
        float GetRemainingTime();
    }

    /// <summary>
    /// Сервис ввода с реактивными событиями
    /// </summary>
    public interface IInputService
    {
        // Реактивные события мыши
        Observable<Vector2> MouseClick { get; }
        Observable<Vector2> MouseRightClick { get; }
        Observable<Vector2> MouseDoubleClick { get; }
        Observable<Vector2> MousePosition { get; }

        // Управление состоянием
        void SetEnabled(bool enabled);
        bool IsEnabled { get; }
        
        // Вспомогательные методы
        Vector2Int ScreenToWorldGrid(Vector2 screenPosition);
        Vector2 WorldGridToScreen(Vector2Int gridPosition);
    }

    /// <summary>
    /// Сервис событий игры (реактивная шина)
    /// </summary>
    public interface IGameEventsService
    {
        // События юнитов
        Observable<UnitSelectedEvent> UnitSelected { get; }
        Observable<UnitMovedEvent> UnitMoved { get; }
        Observable<UnitAttackedEvent> UnitAttacked { get; }
        
        // События игры
        Observable<TurnChangedEvent> TurnChanged { get; }
        Observable<GameStateChangedEvent> GameStateChanged { get; }
        Observable<GameEndedEvent> GameEnded { get; }
        Observable<InfiniteSpeedActivatedEvent> InfiniteSpeedActivated { get; }
        
        // События планирования
        Observable<PathPlannedEvent> PathPlanned { get; }
        Observable<AttackTargetsUpdatedEvent> AttackTargetsUpdated { get; }

        // Методы публикации событий
        void PublishUnitSelected(int unitId, PlayerId playerId);
        void PublishUnitMoved(int unitId, Vector2Int from, Vector2Int to, PlayerId playerId);
        void PublishUnitAttacked(int attackerId, int targetId, Vector2Int attackerPos, Vector2Int targetPos, PlayerId playerId);
        void PublishTurnChanged(PlayerId newPlayer, int turnNumber, float timeLeft);
        void PublishGameStateChanged(GameState state);
        void PublishGameEnded(PlayerId winner, EndReason reason, int finalTurn = 0);
        void PublishInfiniteSpeedActivated(int turnNumber);
        void PublishPathPlanned(int unitId, Vector2Int[] path, bool isValid);
        void PublishAttackTargetsUpdated(int unitId, int[] targets, Vector2Int fromPosition);
    }

    /// <summary>
    /// Сервис для работы с игровым полем
    /// </summary>
    public interface IGameFieldService
    {
        // Информация о поле
        GameField GetField();
        bool IsValidPosition(Vector2Int position);
        bool IsObstacleAt(Vector2Int position);
        
        // Генерация поля
        void GenerateField(int width, int height);
        void PlaceObstacles();
        
        // Навигация
        Vector2Int[] FindPath(Vector2Int from, Vector2Int to);
        bool HasLineOfSight(Vector2Int from, Vector2Int to, int maxRange);
        float GetDistance(Vector2Int from, Vector2Int to);
        
        // Позиции спавна
        Vector2Int[] GetSpawnPositions(PlayerId playerId);
    }

    /// <summary>
    /// Сервис UI с реактивными биндингами
    /// </summary>
    public interface IUIReactiveService
    {
        // Инициализация биндингов
        void InitializeBindings();
        void CleanupBindings();
        
        // Управление UI
        void ShowGameUI();
        void HideGameUI();
        void ShowGameEndScreen(PlayerId winner, EndReason reason);
        
        // Обновление состояния
        void UpdateTurnTimer(float timeLeft);
        void UpdateCurrentPlayer(PlayerId playerId);
        void UpdateActionAvailability(bool canMove, bool canAttack);
    }
}
