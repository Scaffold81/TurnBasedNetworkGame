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
using TurnBasedGame.Core;
using TurnBasedGame.Core.Interfaces;

namespace TurnBasedGame.Services
{
    /// <summary>
    /// Заглушки продвинутых опциональных сервисов
    /// TODO: Реализовать полную функциональность
    /// </summary>

    public class FieldStreamingService : IFieldStreamingService
    {
        private readonly Subject<CellData[]> _fieldDataReceived = new();

        public Observable<CellData[]> FieldDataReceived => _fieldDataReceived;

        public FieldStreamingService() => Debug.Log("[FieldStreamingService] Initialized (Stub)");

        public void RequestFieldData(Vector2Int[] positions)
        {
            Debug.Log($"[FieldStreamingService] RequestFieldData: {positions?.Length ?? 0} positions (STUB)");
        }

        public CellData? GetCellData(Vector2Int position)
        {
            Debug.Log($"[FieldStreamingService] GetCellData: {position} -> null (STUB)");
            return null;
        }

        public bool IsCellKnown(Vector2Int position)
        {
            Debug.Log($"[FieldStreamingService] IsCellKnown: {position} -> false (STUB)");
            return false;
        }

        public HashSet<Vector2Int> GetVisibleCells()
        {
            Debug.Log("[FieldStreamingService] GetVisibleCells -> empty set (STUB)");
            return new HashSet<Vector2Int>();
        }

        public void UpdateVisibility(Vector2Int[] unitPositions, int[] visionRanges)
        {
            Debug.Log($"[FieldStreamingService] UpdateVisibility: {unitPositions?.Length ?? 0} units (STUB)");
        }
    }

    public class ServerValidationService : IServerValidationService
    {
        public ServerValidationService() => Debug.Log("[ServerValidationService] Initialized (Stub)");

        public ValidationResult ValidateMove(int unitId, Vector2Int targetPosition, PlayerId playerId)
        {
            Debug.Log($"[ServerValidationService] ValidateMove: unit {unitId} to {targetPosition} by {playerId} -> Valid (STUB)");
            return ValidationResult.Success();
        }

        public ValidationResult ValidateAttack(int attackerId, int targetId, PlayerId playerId)
        {
            Debug.Log($"[ServerValidationService] ValidateAttack: {attackerId} -> {targetId} by {playerId} -> Valid (STUB)");
            return ValidationResult.Success();
        }

        public ValidationResult ValidateEndTurn(PlayerId playerId)
        {
            Debug.Log($"[ServerValidationService] ValidateEndTurn: {playerId} -> Valid (STUB)");
            return ValidationResult.Success();
        }

        public bool IsSpamming(PlayerId playerId)
        {
            Debug.Log($"[ServerValidationService] IsSpamming: {playerId} -> false (STUB)");
            return false;
        }

        public void RecordAction(PlayerId playerId, ActionType actionType, Vector2Int position)
        {
            Debug.Log($"[ServerValidationService] RecordAction: {playerId}, {actionType} at {position} (STUB)");
        }

        public bool DetectSpeedHack(PlayerId playerId, Vector2Int[] path, float timeTaken)
        {
            Debug.Log($"[ServerValidationService] DetectSpeedHack: {playerId}, path length {path?.Length ?? 0}, time {timeTaken:F2}s -> false (STUB)");
            return false;
        }

        public bool DetectTeleportHack(PlayerId playerId, Vector2Int from, Vector2Int to)
        {
            Debug.Log($"[ServerValidationService] DetectTeleportHack: {playerId}, {from} -> {to} -> false (STUB)");
            return false;
        }

        public void ReportSuspiciousActivity(PlayerId playerId, CheatType cheatType, string details)
        {
            Debug.Log($"[ServerValidationService] ReportSuspiciousActivity: {playerId}, {cheatType}, {details} (STUB)");
        }
    }

    public class LineOfSightService : ILineOfSightService
    {
        public LineOfSightService() => Debug.Log("[LineOfSightService] Initialized (Stub)");

        public bool HasLineOfSight(Vector2Int from, Vector2Int to, int maxRange)
        {
            Debug.Log($"[LineOfSightService] HasLineOfSight: {from} -> {to}, range {maxRange} -> true (STUB)");
            return true;
        }

        public bool HasObstacleBetween(Vector2Int from, Vector2Int to)
        {
            Debug.Log($"[LineOfSightService] HasObstacleBetween: {from} -> {to} -> false (STUB)");
            return false;
        }

        public Vector2Int[] GetLinePoints(Vector2Int from, Vector2Int to)
        {
            Debug.Log($"[LineOfSightService] GetLinePoints: {from} -> {to} (STUB)");
            return new Vector2Int[] { from, to };
        }

        public bool IsPointBlocked(Vector2Int point)
        {
            Debug.Log($"[LineOfSightService] IsPointBlocked: {point} -> false (STUB)");
            return false;
        }

        public bool HasLineOfSightWithSizes(Vector2Int from, Vector2Int to, float fromSize, float toSize, int maxRange)
        {
            Debug.Log($"[LineOfSightService] HasLineOfSightWithSizes: {from} -> {to}, sizes {fromSize}->{toSize}, range {maxRange} -> true (STUB)");
            return true;
        }
    }

    public class AdvancedTargetingService : IAdvancedTargetingService
    {
        public AdvancedTargetingService() => Debug.Log("[AdvancedTargetingService] Initialized (Stub)");

        public int[] GetTargetsInRange(Vector2Int attackerPosition, int attackRange, PlayerId excludePlayer, float attackerSize = 1f)
        {
            Debug.Log($"[AdvancedTargetingService] GetTargetsInRange: pos {attackerPosition}, range {attackRange}, exclude {excludePlayer} -> empty (STUB)");
            return new int[0];
        }

        public int[] GetTargetsInArea(Vector2Int center, int radius, PlayerId excludePlayer)
        {
            Debug.Log($"[AdvancedTargetingService] GetTargetsInArea: center {center}, radius {radius}, exclude {excludePlayer} -> empty (STUB)");
            return new int[0];
        }

        public bool IsUnitInRange(Vector2Int attackerPos, UnitData target, int range, float attackerSize = 1f)
        {
            Debug.Log($"[AdvancedTargetingService] IsUnitInRange: {attackerPos} -> unit {target.id} at {target.position}, range {range} -> true (STUB)");
            return true;
        }

        public bool IsCircleIntersectingUnit(Vector2Int circleCenter, int radius, UnitData unit)
        {
            Debug.Log($"[AdvancedTargetingService] IsCircleIntersectingUnit: circle at {circleCenter} r{radius} vs unit {unit.id} -> true (STUB)");
            return true;
        }

        public float CalculateDistanceWithSizes(Vector2Int pos1, Vector2Int pos2, float size1, float size2)
        {
            var distance = Vector2Int.Distance(pos1, pos2);
            Debug.Log($"[AdvancedTargetingService] CalculateDistanceWithSizes: {pos1} -> {pos2}, sizes {size1}->{size2} = {distance:F2} (STUB)");
            return distance;
        }
    }
}
