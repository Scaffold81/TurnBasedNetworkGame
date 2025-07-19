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
    /// Остальные игровые сервисы (заглушки)
    /// TODO: Реализовать полную функциональность
    /// </summary>

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
