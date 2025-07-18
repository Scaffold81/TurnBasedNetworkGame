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
using TurnBasedGame.Core.Events;

namespace TurnBasedGame.Services
{
    /// <summary>
    /// Сервис событий игры - реактивная шина для всех игровых событий
    /// </summary>
    public class GameEventsService : IGameEventsService
    {
        // Subjects для публикации событий
        private readonly Subject<UnitSelectedEvent> _unitSelected = new();
        private readonly Subject<UnitMovedEvent> _unitMoved = new();
        private readonly Subject<UnitAttackedEvent> _unitAttacked = new();
        private readonly Subject<TurnChangedEvent> _turnChanged = new();
        private readonly Subject<GameStateChangedEvent> _gameStateChanged = new();
        private readonly Subject<GameEndedEvent> _gameEnded = new();
        private readonly Subject<InfiniteSpeedActivatedEvent> _infiniteSpeedActivated = new();
        private readonly Subject<PathPlannedEvent> _pathPlanned = new();
        private readonly Subject<AttackTargetsUpdatedEvent> _attackTargetsUpdated = new();

        // Публичные Observable (только для чтения)
        public Observable<UnitSelectedEvent> UnitSelected => _unitSelected;
        public Observable<UnitMovedEvent> UnitMoved => _unitMoved;
        public Observable<UnitAttackedEvent> UnitAttacked => _unitAttacked;
        public Observable<TurnChangedEvent> TurnChanged => _turnChanged;
        public Observable<GameStateChangedEvent> GameStateChanged => _gameStateChanged;
        public Observable<GameEndedEvent> GameEnded => _gameEnded;
        public Observable<InfiniteSpeedActivatedEvent> InfiniteSpeedActivated => _infiniteSpeedActivated;
        public Observable<PathPlannedEvent> PathPlanned => _pathPlanned;
        public Observable<AttackTargetsUpdatedEvent> AttackTargetsUpdated => _attackTargetsUpdated;

        public GameEventsService()
        {
            Debug.Log("[GameEventsService] Initialized reactive events system");
        }

        public void PublishUnitSelected(int unitId, PlayerId playerId)
        {
            Debug.Log($"[GameEventsService] Unit {unitId} selected by player {playerId}");
            _unitSelected.OnNext(new UnitSelectedEvent(unitId, playerId));
        }

        public void PublishUnitMoved(int unitId, Vector2Int from, Vector2Int to, PlayerId playerId)
        {
            Debug.Log($"[GameEventsService] Unit {unitId} moved from {from} to {to} by player {playerId}");
            _unitMoved.OnNext(new UnitMovedEvent(unitId, from, to, playerId));
        }

        public void PublishUnitAttacked(int attackerId, int targetId, Vector2Int attackerPos, Vector2Int targetPos, PlayerId playerId)
        {
            Debug.Log($"[GameEventsService] Unit {attackerId} attacked unit {targetId} by player {playerId}");
            _unitAttacked.OnNext(new UnitAttackedEvent(attackerId, targetId, attackerPos, targetPos, playerId));
        }

        public void PublishTurnChanged(PlayerId newPlayer, int turnNumber, float timeLeft)
        {
            Debug.Log($"[GameEventsService] Turn changed to player {newPlayer}, turn {turnNumber}, time left: {timeLeft:F1}s");
            _turnChanged.OnNext(new TurnChangedEvent(newPlayer, turnNumber, timeLeft));
        }

        public void PublishGameStateChanged(GameState state)
        {
            Debug.Log($"[GameEventsService] Game state changed: Phase {state.phase}, Turn {state.currentTurn}");
            _gameStateChanged.OnNext(new GameStateChangedEvent(state));
        }

        public void PublishGameEnded(PlayerId winner, EndReason reason, int finalTurn = 0)
        {
            Debug.Log($"[GameEventsService] Game ended! Winner: {winner}, Reason: {reason}, Final turn: {finalTurn}");
            _gameEnded.OnNext(new GameEndedEvent(winner, reason, finalTurn));
        }

        public void PublishInfiniteSpeedActivated(int turnNumber)
        {
            Debug.Log($"[GameEventsService] Infinite speed activated on turn {turnNumber}!");
            _infiniteSpeedActivated.OnNext(new InfiniteSpeedActivatedEvent(turnNumber));
        }

        public void PublishPathPlanned(int unitId, Vector2Int[] path, bool isValid)
        {
            Debug.Log($"[GameEventsService] Path planned for unit {unitId}, length: {path?.Length ?? 0}, valid: {isValid}");
            _pathPlanned.OnNext(new PathPlannedEvent(unitId, path, isValid));
        }

        public void PublishAttackTargetsUpdated(int unitId, int[] targets, Vector2Int fromPosition)
        {
            Debug.Log($"[GameEventsService] Attack targets updated for unit {unitId}, targets count: {targets?.Length ?? 0}");
            _attackTargetsUpdated.OnNext(new AttackTargetsUpdatedEvent(unitId, targets, fromPosition));
        }
    }
}
