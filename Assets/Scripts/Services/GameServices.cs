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
using R3;
using TurnBasedGame.Core;
using TurnBasedGame.Core.Interfaces;

namespace TurnBasedGame.Services
{
    /// <summary>
    /// Заглушки игровых сервисов
    /// TODO: Реализовать полную функциональность
    /// </summary>

    public class UnitService : IUnitService
    {
        private readonly ReactiveProperty<UnitData?> _selectedUnit = new();
        private readonly ReactiveProperty<Vector2Int[]> _movementPath = new();
        private readonly ReactiveProperty<int[]> _attackTargets = new();

        public ReadOnlyReactiveProperty<UnitData?> SelectedUnit { get; private set; }
        public ReadOnlyReactiveProperty<Vector2Int[]> MovementPath { get; private set; }
        public ReadOnlyReactiveProperty<int[]> AttackTargets { get; private set; }

        public UnitService()
        {
            SelectedUnit = _selectedUnit.ToReadOnlyReactiveProperty();
            MovementPath = _movementPath.ToReadOnlyReactiveProperty();
            AttackTargets = _attackTargets.ToReadOnlyReactiveProperty();
            Debug.Log("[UnitService] Initialized (Stub)");
        }

        public void SelectUnit(int unitId) => Debug.Log($"[UnitService] SelectUnit: {unitId} (STUB)");
        public void DeselectUnit() => Debug.Log("[UnitService] DeselectUnit (STUB)");
        public void PlanMovement(Vector2Int targetPosition) => Debug.Log($"[UnitService] PlanMovement: {targetPosition} (STUB)");
        public void PlanAttack(int targetId) => Debug.Log($"[UnitService] PlanAttack: {targetId} (STUB)");
        public void ExecuteMove(Vector2Int targetPosition) => Debug.Log($"[UnitService] ExecuteMove: {targetPosition} (STUB)");
        public void ExecuteAttack(int targetId) => Debug.Log($"[UnitService] ExecuteAttack: {targetId} (STUB)");
        public bool CanUnitMove(int unitId) { Debug.Log($"[UnitService] CanUnitMove: {unitId} -> true (STUB)"); return true; }
        public bool CanUnitAttack(int unitId) { Debug.Log($"[UnitService] CanUnitAttack: {unitId} -> true (STUB)"); return true; }
        
        public Vector2Int[] CalculatePath(Vector2Int from, Vector2Int to, int maxDistance)
        {
            Debug.Log($"[UnitService] CalculatePath: {from} -> {to}, max: {maxDistance} (STUB)");
            return new Vector2Int[] { from, to };
        }
        
        public int[] GetTargetsInRange(Vector2Int position, int range, PlayerId excludePlayer)
        {
            Debug.Log($"[UnitService] GetTargetsInRange: pos {position}, range {range} (STUB)");
            return new int[0];
        }
    }

    public class TurnService : ITurnService
    {
        private readonly ReactiveProperty<float> _turnTimer = new(GameConstants.TURN_DURATION);
        private readonly ReactiveProperty<bool> _isMyTurn = new(true);
        private readonly ReactiveProperty<bool> _canMove = new(true);
        private readonly ReactiveProperty<bool> _canAttack = new(true);

        public ReadOnlyReactiveProperty<float> TurnTimer { get; private set; }
        public ReadOnlyReactiveProperty<bool> IsMyTurn { get; private set; }
        public ReadOnlyReactiveProperty<bool> CanMove { get; private set; }
        public ReadOnlyReactiveProperty<bool> CanAttack { get; private set; }

        public TurnService()
        {
            TurnTimer = _turnTimer.ToReadOnlyReactiveProperty();
            IsMyTurn = _isMyTurn.ToReadOnlyReactiveProperty();
            CanMove = _canMove.ToReadOnlyReactiveProperty();
            CanAttack = _canAttack.ToReadOnlyReactiveProperty();
            Debug.Log("[TurnService] Initialized (Stub)");
        }

        public void EndTurn() => Debug.Log("[TurnService] EndTurn (STUB)");
        public void StartTurn(PlayerId playerId) => Debug.Log($"[TurnService] StartTurn: {playerId} (STUB)");
        public PlayerId GetCurrentPlayer() { Debug.Log("[TurnService] GetCurrentPlayer -> Player1 (STUB)"); return PlayerId.Player1; }
        public int GetCurrentTurnNumber() { Debug.Log("[TurnService] GetCurrentTurnNumber -> 1 (STUB)"); return 1; }
        public float GetRemainingTime() { Debug.Log("[TurnService] GetRemainingTime -> 60 (STUB)"); return 60f; }
    }

    public class GameFieldService : IGameFieldService
    {
        public GameFieldService() => Debug.Log("[GameFieldService] Initialized (Stub)");
        public GameField GetField() { Debug.Log("[GameFieldService] GetField (STUB)"); return GameField.Default; }
        public bool IsValidPosition(Vector2Int position) { Debug.Log($"[GameFieldService] IsValidPosition: {position} -> true (STUB)"); return true; }
        public bool IsObstacleAt(Vector2Int position) { Debug.Log($"[GameFieldService] IsObstacleAt: {position} -> false (STUB)"); return false; }
        public void GenerateField(int width, int height) => Debug.Log($"[GameFieldService] GenerateField: {width}x{height} (STUB)");
        public void PlaceObstacles() => Debug.Log("[GameFieldService] PlaceObstacles (STUB)");
        
        public Vector2Int[] FindPath(Vector2Int from, Vector2Int to)
        {
            Debug.Log($"[GameFieldService] FindPath: {from} -> {to} (STUB)");
            return new Vector2Int[] { from, to };
        }
        
        public bool HasLineOfSight(Vector2Int from, Vector2Int to, int maxRange)
        {
            Debug.Log($"[GameFieldService] HasLineOfSight: {from} -> {to} -> true (STUB)");
            return true;
        }
        
        public float GetDistance(Vector2Int from, Vector2Int to)
        {
            var dist = Vector2Int.Distance(from, to);
            Debug.Log($"[GameFieldService] GetDistance: {from} -> {to} = {dist} (STUB)");
            return dist;
        }
        
        public Vector2Int[] GetSpawnPositions(PlayerId playerId)
        {
            Debug.Log($"[GameFieldService] GetSpawnPositions: {playerId} (STUB)");
            return playerId == PlayerId.Player1 ? 
                new Vector2Int[] { new Vector2Int(1, 1), new Vector2Int(1, 2) } : 
                new Vector2Int[] { new Vector2Int(18, 18), new Vector2Int(18, 17) };
        }
    }

    public class InputService : MonoBehaviour, IInputService
    {
        private readonly Subject<Vector2> _mouseClick = new();
        private readonly Subject<Vector2> _mouseRightClick = new();
        private readonly Subject<Vector2> _mouseDoubleClick = new();
        private readonly Subject<Vector2> _mousePosition = new();

        public Observable<Vector2> MouseClick => _mouseClick;
        public Observable<Vector2> MouseRightClick => _mouseRightClick;
        public Observable<Vector2> MouseDoubleClick => _mouseDoubleClick;
        public Observable<Vector2> MousePosition => _mousePosition;

        public bool IsEnabled { get; private set; } = true;

        private void Start() => Debug.Log("[InputService] Initialized (Stub)");
        public void SetEnabled(bool enabled) => Debug.Log($"[InputService] SetEnabled: {enabled} (STUB)");
        
        public Vector2Int ScreenToWorldGrid(Vector2 screenPosition)
        {
            Debug.Log($"[InputService] ScreenToWorldGrid: {screenPosition} (STUB)");
            return Vector2Int.zero;
        }
        
        public Vector2 WorldGridToScreen(Vector2Int gridPosition)
        {
            Debug.Log($"[InputService] WorldGridToScreen: {gridPosition} (STUB)");
            return Vector2.zero;
        }
    }

    public class PathfindingService : IPathfindingService
    {
        public PathfindingService() => Debug.Log("[PathfindingService] Initialized (Stub)");
        
        public Vector2Int[] FindPath(Vector2Int start, Vector2Int goal)
        {
            Debug.Log($"[PathfindingService] FindPath: {start} -> {goal} (STUB)");
            return new Vector2Int[] { start, goal };
        }
        
        public Vector2Int[] FindPath(Vector2Int start, Vector2Int goal, int maxDistance)
        {
            Debug.Log($"[PathfindingService] FindPath with max distance: {start} -> {goal}, max: {maxDistance} (STUB)");
            return new Vector2Int[] { start, goal };
        }
        
        public bool IsPathValid(Vector2Int[] path, int maxDistance) { Debug.Log("[PathfindingService] IsPathValid -> true (STUB)"); return true; }
        public bool IsPositionWalkable(Vector2Int position) { Debug.Log($"[PathfindingService] IsPositionWalkable: {position} -> true (STUB)"); return true; }
        public int CalculatePathLength(Vector2Int[] path) { Debug.Log($"[PathfindingService] CalculatePathLength: {path?.Length ?? 0} (STUB)"); return path?.Length ?? 0; }
        public Vector2Int[] OptimizePath(Vector2Int[] path) { Debug.Log("[PathfindingService] OptimizePath (STUB)"); return path; }
        
        public Vector2Int[] FindPathAvoidingUnits(Vector2Int start, Vector2Int goal, Vector2Int[] unitPositions)
        {
            Debug.Log($"[PathfindingService] FindPathAvoidingUnits: {start} -> {goal}, units: {unitPositions?.Length ?? 0} (STUB)");
            return new Vector2Int[] { start, goal };
        }
    }

    public class GameRulesService : IGameRulesService
    {
        public GameRulesService() => Debug.Log("[GameRulesService] Initialized (Stub)");
        public void CheckGameEndConditions() => Debug.Log("[GameRulesService] CheckGameEndConditions (STUB)");
        public void CheckDrawResolution() => Debug.Log("[GameRulesService] CheckDrawResolution (STUB)");
        public void ActivateInfiniteSpeed() => Debug.Log("[GameRulesService] ActivateInfiniteSpeed (STUB)");
        
        public bool ShouldActivateInfiniteSpeed(int currentTurn, int player1Units, int player2Units)
        {
            Debug.Log($"[GameRulesService] ShouldActivateInfiniteSpeed: turn {currentTurn}, p1: {player1Units}, p2: {player2Units} -> false (STUB)");
            return false;
        }
        
        public PlayerId DetermineWinner()
        {
            Debug.Log("[GameRulesService] DetermineWinner -> None (STUB)");
            return PlayerId.None;
        }
        
        public EndReason GetEndReason()
        {
            Debug.Log("[GameRulesService] GetEndReason -> None (STUB)");
            return EndReason.None;
        }
    }
}
