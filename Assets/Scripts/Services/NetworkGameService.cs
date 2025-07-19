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
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Zenject;
using R3;
using TurnBasedGame.Core;
using TurnBasedGame.Core.Interfaces;

namespace TurnBasedGame.Services
{
    /// <summary>
    /// Полная реализация сетевого сервиса для мультиплеера
    /// Использует Netcode for GameObjects для синхронизации игры
    /// </summary>
    public class NetworkGameService : NetworkBehaviour, INetworkGameService
    {
        // Инжектируемые зависимости
        [Inject] private IGameStateService _gameState;
        [Inject] private IGameEventsService _gameEvents;
        [Inject] private GameConfig _gameConfig;

        // NetworkVariable для синхронизации состояния
        private NetworkVariable<GameState> _networkGameState = new(
            GameState.Default, 
            NetworkVariableReadPermission.Everyone, 
            NetworkVariableWritePermission.Server
        );

        private NetworkList<UnitData> _networkUnits;

        // Реактивные события
        private readonly Subject<GameState> _gameStateChanged = new();
        private readonly Subject<UnitData> _unitChanged = new();
        private readonly Subject<int> _unitRemoved = new();

        // Публичные реактивные свойства
        public Observable<GameState> GameStateChanged => _gameStateChanged;
        public Observable<UnitData> UnitChanged => _unitChanged;
        public Observable<int> UnitRemoved => _unitRemoved;

        // Информация о соединении
        public bool IsServer => NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer;
        public bool IsClient => NetworkManager.Singleton != null && NetworkManager.Singleton.IsClient;

        // Локальное состояние
        private readonly Dictionary<ulong, PlayerId> _clientPlayerMap = new();
        private bool _gameStarted = false;
        private int _connectedPlayers = 0;

        private void Awake()
        {
            _networkUnits = new NetworkList<UnitData>();
            Debug.Log("[NetworkGameService] Initialized with Netcode for GameObjects");
        }

        public override void OnNetworkSpawn()
        {
            Debug.Log($"[NetworkGameService] Network spawned. IsServer: {IsServer}, IsClient: {IsClient}");

            _networkGameState.OnValueChanged += OnNetworkGameStateChanged;
            _networkUnits.OnListChanged += OnNetworkUnitsChanged;

            if (IsServer)
            {
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
                InitializeServerGame();
            }

            base.OnNetworkSpawn();
        }

        public override void OnNetworkDespawn()
        {
            _networkGameState.OnValueChanged -= OnNetworkGameStateChanged;
            _networkUnits.OnListChanged -= OnNetworkUnitsChanged;

            if (IsServer && NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            }

            base.OnNetworkDespawn();
        }

        #region Public Interface

        public void RequestMoveUnit(int unitId, Vector2Int targetPosition)
        {
            if (!IsClient) return;
            RequestMoveUnitServerRpc(unitId, targetPosition);
        }

        public void RequestAttackUnit(int attackerId, int targetId)
        {
            if (!IsClient) return;
            RequestAttackUnitServerRpc(attackerId, targetId);
        }

        public void RequestEndTurn()
        {
            if (!IsClient) return;
            RequestEndTurnServerRpc();
        }

        public PlayerId GetLocalPlayerId()
        {
            if (!IsClient) return PlayerId.None;
            var clientId = NetworkManager.Singleton.LocalClientId;
            return _clientPlayerMap.TryGetValue(clientId, out var playerId) ? playerId : PlayerId.None;
        }

        #endregion

        #region Server RPCs

        [ServerRpc(RequireOwnership = false)]
        public void RequestMoveUnitServerRpc(int unitId, Vector2Int targetPosition, ServerRpcParams rpcParams = default)
        {
            var clientId = rpcParams.Receive.SenderClientId;
            var playerId = GetPlayerIdFromClientId(clientId);

            if (ValidateMoveOnServer(unitId, targetPosition, playerId))
            {
                ExecuteMoveOnServer(unitId, targetPosition, playerId);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestAttackUnitServerRpc(int attackerId, int targetId, ServerRpcParams rpcParams = default)
        {
            var clientId = rpcParams.Receive.SenderClientId;
            var playerId = GetPlayerIdFromClientId(clientId);

            if (ValidateAttackOnServer(attackerId, targetId, playerId))
            {
                ExecuteAttackOnServer(attackerId, targetId, playerId);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestEndTurnServerRpc(ServerRpcParams rpcParams = default)
        {
            var clientId = rpcParams.Receive.SenderClientId;
            var playerId = GetPlayerIdFromClientId(clientId);

            if (_networkGameState.Value.currentPlayer == playerId)
            {
                ExecuteEndTurnOnServer(playerId);
            }
        }

        #endregion

        #region Server Logic

        private void InitializeServerGame()
        {
            var gameState = GameState.Default;
            gameState.phase = GamePhase.WaitingForPlayers;
            _networkGameState.Value = gameState;
        }

        private void OnClientConnected(ulong clientId)
        {
            _connectedPlayers++;
            var playerId = _connectedPlayers == 1 ? PlayerId.Player1 : PlayerId.Player2;
            _clientPlayerMap[clientId] = playerId;

            if (_connectedPlayers >= 2)
            {
                StartGame();
            }
        }

        private void OnClientDisconnected(ulong clientId)
        {
            if (_clientPlayerMap.TryGetValue(clientId, out var playerId))
            {
                _clientPlayerMap.Remove(clientId);
                _connectedPlayers--;

                if (_gameStarted)
                {
                    var winner = playerId == PlayerId.Player1 ? PlayerId.Player2 : PlayerId.Player1;
                    EndGame(winner, EndReason.PlayerDisconnected);
                }
            }
        }

        private void StartGame()
        {
            var gameState = _networkGameState.Value;
            gameState.phase = GamePhase.Playing;
            gameState.currentPlayer = PlayerId.Player1;
            gameState.currentTurn = 1;
            gameState.turnTimeLeft = _gameConfig.TurnDuration;
            gameState.canMove = true;
            gameState.canAttack = true;

            _networkGameState.Value = gameState;
            _gameStarted = true;

            CreateInitialUnits();
        }

        private void CreateInitialUnits()
        {
            var unitId = 1;

            for (int i = 0; i < _gameConfig.StartingUnitsPerType; i++)
            {
                _networkUnits.Add(_gameConfig.CreateRangedUnit(unitId++, PlayerId.Player1, new Vector2Int(1, 1 + i)));
                _networkUnits.Add(_gameConfig.CreateMeleeUnit(unitId++, PlayerId.Player1, new Vector2Int(2, 1 + i)));
                _networkUnits.Add(_gameConfig.CreateRangedUnit(unitId++, PlayerId.Player2, new Vector2Int(_gameConfig.FieldWidth - 2, _gameConfig.FieldHeight - 2 - i)));
                _networkUnits.Add(_gameConfig.CreateMeleeUnit(unitId++, PlayerId.Player2, new Vector2Int(_gameConfig.FieldWidth - 3, _gameConfig.FieldHeight - 2 - i)));
            }
        }

        private bool ValidateMoveOnServer(int unitId, Vector2Int targetPosition, PlayerId playerId)
        {
            if (_networkGameState.Value.currentPlayer != playerId) return false;

            foreach (var unit in _networkUnits)
            {
                if (unit.id == unitId && unit.owner == playerId && !unit.hasMovedThisTurn)
                {
                    return Vector2Int.Distance(unit.position, targetPosition) <= unit.speed;
                }
            }
            return false;
        }

        private bool ValidateAttackOnServer(int attackerId, int targetId, PlayerId playerId)
        {
            if (_networkGameState.Value.currentPlayer != playerId) return false;

            UnitData? attacker = null, target = null;
            foreach (var unit in _networkUnits)
            {
                if (unit.id == attackerId) attacker = unit;
                if (unit.id == targetId) target = unit;
            }

            if (!attacker.HasValue || !target.HasValue) return false;
            if (attacker.Value.owner != playerId || target.Value.owner == playerId) return false;
            if (attacker.Value.hasAttackedThisTurn) return false;

            return Vector2Int.Distance(attacker.Value.position, target.Value.position) <= attacker.Value.attackRange;
        }

        private void ExecuteMoveOnServer(int unitId, Vector2Int targetPosition, PlayerId playerId)
        {
            for (int i = 0; i < _networkUnits.Count; i++)
            {
                var unit = _networkUnits[i];
                if (unit.id == unitId && unit.owner == playerId)
                {
                    unit.position = targetPosition;
                    unit.hasMovedThisTurn = true;
                    _networkUnits[i] = unit;
                    break;
                }
            }
        }

        private void ExecuteAttackOnServer(int attackerId, int targetId, PlayerId playerId)
        {
            // Удаляем цель
            for (int i = _networkUnits.Count - 1; i >= 0; i--)
            {
                if (_networkUnits[i].id == targetId)
                {
                    _networkUnits.RemoveAt(i);
                    break;
                }
            }

            // Отмечаем атакующего
            for (int i = 0; i < _networkUnits.Count; i++)
            {
                var unit = _networkUnits[i];
                if (unit.id == attackerId && unit.owner == playerId)
                {
                    unit.hasAttackedThisTurn = true;
                    _networkUnits[i] = unit;
                    break;
                }
            }

            CheckGameEndConditions();
        }

        private void ExecuteEndTurnOnServer(PlayerId playerId)
        {
            // Сбрасываем флаги действий
            for (int i = 0; i < _networkUnits.Count; i++)
            {
                var unit = _networkUnits[i];
                if (unit.owner == playerId)
                {
                    unit.hasMovedThisTurn = false;
                    unit.hasAttackedThisTurn = false;
                    _networkUnits[i] = unit;
                }
            }

            var gameState = _networkGameState.Value;
            var nextPlayer = gameState.currentPlayer == PlayerId.Player1 ? PlayerId.Player2 : PlayerId.Player1;
            var nextTurn = nextPlayer == PlayerId.Player1 ? gameState.currentTurn + 1 : gameState.currentTurn;

            gameState.currentPlayer = nextPlayer;
            gameState.currentTurn = nextTurn;
            gameState.turnTimeLeft = _gameConfig.TurnDuration;
            gameState.canMove = true;
            gameState.canAttack = true;

            _networkGameState.Value = gameState;
        }

        private void CheckGameEndConditions()
        {
            var player1Units = 0;
            var player2Units = 0;

            foreach (var unit in _networkUnits)
            {
                if (unit.owner == PlayerId.Player1) player1Units++;
                else if (unit.owner == PlayerId.Player2) player2Units++;
            }

            if (player1Units == 0)
                EndGame(PlayerId.Player2, EndReason.AllEnemyUnitsDestroyed);
            else if (player2Units == 0)
                EndGame(PlayerId.Player1, EndReason.AllEnemyUnitsDestroyed);
        }

        private void EndGame(PlayerId winner, EndReason reason)
        {
            var gameState = _networkGameState.Value;
            gameState.phase = GamePhase.GameEnded;
            _networkGameState.Value = gameState;
        }

        private PlayerId GetPlayerIdFromClientId(ulong clientId)
        {
            return _clientPlayerMap.TryGetValue(clientId, out var playerId) ? playerId : PlayerId.None;
        }

        #endregion

        #region Event Handlers

        private void OnNetworkGameStateChanged(GameState previousValue, GameState newValue)
        {
            _gameStateChanged.OnNext(newValue);
        }

        private void OnNetworkUnitsChanged(NetworkListEvent<UnitData> changeEvent)
        {
            switch (changeEvent.Type)
            {
                case NetworkListEvent<UnitData>.EventType.Add:
                case NetworkListEvent<UnitData>.EventType.Value:
                    _unitChanged.OnNext(changeEvent.Value);
                    break;
                case NetworkListEvent<UnitData>.EventType.Remove:
                    _unitRemoved.OnNext(changeEvent.Value.id);
                    break;
            }
        }

        #endregion
    }
}
