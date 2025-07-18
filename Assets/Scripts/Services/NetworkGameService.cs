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
using Unity.Netcode;
using R3;
using TurnBasedGame.Core;
using TurnBasedGame.Core.Interfaces;

namespace TurnBasedGame.Services
{
    /// <summary>
    /// Заглушка сетевого сервиса для мультиплеера
    /// TODO: Реализовать полную функциональность с Netcode for GameObjects
    /// </summary>
    public class NetworkGameService : NetworkBehaviour, INetworkGameService
    {
        // Заглушки для реактивных событий
        private readonly Subject<GameState> _gameStateChanged = new();
        private readonly Subject<UnitData> _unitChanged = new();
        private readonly Subject<int> _unitRemoved = new();

        public Observable<GameState> GameStateChanged => _gameStateChanged;
        public Observable<UnitData> UnitChanged => _unitChanged;
        public Observable<int> UnitRemoved => _unitRemoved;

        public bool IsServer => NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer;
        public bool IsClient => NetworkManager.Singleton != null && NetworkManager.Singleton.IsClient;

        private void Start()
        {
            Debug.Log("[NetworkGameService] Started (Stub implementation)");
        }

        public void RequestMoveUnit(int unitId, Vector2Int targetPosition)
        {
            Debug.Log($"[NetworkGameService] RequestMoveUnit: Unit {unitId} to {targetPosition} (STUB)");
            // TODO: Реализовать ServerRpc
        }

        public void RequestAttackUnit(int attackerId, int targetId)
        {
            Debug.Log($"[NetworkGameService] RequestAttackUnit: Attacker {attackerId} -> Target {targetId} (STUB)");
            // TODO: Реализовать ServerRpc
        }

        public void RequestEndTurn()
        {
            Debug.Log("[NetworkGameService] RequestEndTurn (STUB)");
            // TODO: Реализовать ServerRpc
        }

        public PlayerId GetLocalPlayerId()
        {
            // Заглушка - возвращаем Player1 для тестирования
            Debug.Log("[NetworkGameService] GetLocalPlayerId -> Player1 (STUB)");
            return PlayerId.Player1;
        }
    }
}
