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

namespace TurnBasedGame.Core.Events
{
    /// <summary>
    /// Базовое игровое событие
    /// </summary>
    public abstract class GameEvent
    {
        public float timestamp = Time.time;
    }

    /// <summary>
    /// Юнит был выбран
    /// </summary>
    public class UnitSelectedEvent : GameEvent
    {
        public int unitId;
        public PlayerId playerId;

        public UnitSelectedEvent(int unitId, PlayerId playerId)
        {
            this.unitId = unitId;
            this.playerId = playerId;
        }
    }

    /// <summary>
    /// Юнит переместился
    /// </summary>
    public class UnitMovedEvent : GameEvent
    {
        public int unitId;
        public Vector2Int fromPosition;
        public Vector2Int toPosition;
        public PlayerId playerId;

        public UnitMovedEvent(int unitId, Vector2Int from, Vector2Int to, PlayerId playerId)
        {
            this.unitId = unitId;
            this.fromPosition = from;
            this.toPosition = to;
            this.playerId = playerId;
        }
    }

    /// <summary>
    /// Юнит атаковал
    /// </summary>
    public class UnitAttackedEvent : GameEvent
    {
        public int attackerId;
        public int targetId;
        public Vector2Int attackerPosition;
        public Vector2Int targetPosition;
        public PlayerId playerId;

        public UnitAttackedEvent(int attackerId, int targetId, Vector2Int attackerPos, Vector2Int targetPos, PlayerId playerId)
        {
            this.attackerId = attackerId;
            this.targetId = targetId;
            this.attackerPosition = attackerPos;
            this.targetPosition = targetPos;
            this.playerId = playerId;
        }
    }

    /// <summary>
    /// Ход игрока изменился
    /// </summary>
    public class TurnChangedEvent : GameEvent
    {
        public PlayerId newCurrentPlayer;
        public int turnNumber;
        public float turnTimeLeft;

        public TurnChangedEvent(PlayerId newPlayer, int turn, float timeLeft)
        {
            this.newCurrentPlayer = newPlayer;
            this.turnNumber = turn;
            this.turnTimeLeft = timeLeft;
        }
    }

    /// <summary>
    /// Состояние игры изменилось
    /// </summary>
    public class GameStateChangedEvent : GameEvent
    {
        public GameState newState;

        public GameStateChangedEvent(GameState state)
        {
            this.newState = state;
        }
    }

    /// <summary>
    /// Игра завершилась
    /// </summary>
    public class GameEndedEvent : GameEvent
    {
        public PlayerId winner;
        public EndReason reason;
        public int finalTurn;

        public GameEndedEvent(PlayerId winner, EndReason reason, int finalTurn = 0)
        {
            this.winner = winner;
            this.reason = reason;
            this.finalTurn = finalTurn;
        }
    }

    /// <summary>
    /// Активировалась бесконечная скорость (согласно правилам ТЗ)
    /// </summary>
    public class InfiniteSpeedActivatedEvent : GameEvent
    {
        public int turnNumber;
        public string message;

        public InfiniteSpeedActivatedEvent(int turn)
        {
            this.turnNumber = turn;
            this.message = "Infinite speed activated! All units can now move unlimited distance.";
        }
    }

    /// <summary>
    /// Обнаружен чит
    /// </summary>
    public class CheatDetectedEvent : GameEvent
    {
        public PlayerId suspiciousPlayer;
        public CheatType type;
        public string details;

        public CheatDetectedEvent(PlayerId player, CheatType type, string details)
        {
            this.suspiciousPlayer = player;
            this.type = type;
            this.details = details;
        }
    }

    /// <summary>
    /// Данные поля переданы клиенту (стриминг)
    /// </summary>
    public class FieldDataStreamedEvent : GameEvent
    {
        public Vector2Int[] streamedCells;
        public int totalCellsKnown;

        public FieldDataStreamedEvent(Vector2Int[] cells, int totalKnown)
        {
            this.streamedCells = cells;
            this.totalCellsKnown = totalKnown;
        }
    }

    /// <summary>
    /// Путь для юнита запланирован
    /// </summary>
    public class PathPlannedEvent : GameEvent
    {
        public int unitId;
        public Vector2Int[] path;
        public bool isValidPath;

        public PathPlannedEvent(int unitId, Vector2Int[] path, bool isValid)
        {
            this.unitId = unitId;
            this.path = path;
            this.isValidPath = isValid;
        }
    }

    /// <summary>
    /// Цели для атаки обновлены
    /// </summary>
    public class AttackTargetsUpdatedEvent : GameEvent
    {
        public int unitId;
        public int[] availableTargets;
        public Vector2Int fromPosition;

        public AttackTargetsUpdatedEvent(int unitId, int[] targets, Vector2Int fromPos)
        {
            this.unitId = unitId;
            this.availableTargets = targets;
            this.fromPosition = fromPos;
        }
    }
}
