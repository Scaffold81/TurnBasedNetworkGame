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
using R3;
using ObservableCollections;
using TurnBasedGame.Core;
using TurnBasedGame.Core.Interfaces;
using TurnBasedGame.Core.Events;

namespace TurnBasedGame.Services
{
    /// <summary>
    /// Основной сервис состояния игры с реактивными свойствами на R3
    /// </summary>
    public class GameStateService : IGameStateService
    {
        // Реактивные свойства
        private readonly ReactiveProperty<GameState> _gameState;
        private readonly ObservableList<UnitData> _units;
        private readonly Subject<GameEvent> _gameEvents;

        // Публичные реактивные свойства (только для чтения)
        public ReadOnlyReactiveProperty<GameState> GameState { get; private set; }
        public IReadOnlyObservableList<UnitData> Units => _units;
        public Observable<GameEvent> GameEvents => _gameEvents;

        public GameStateService()
        {
            // Инициализируем поля в конструкторе
            _gameState = new ReactiveProperty<GameState>(Core.GameState.Default);
            _units = new ObservableList<UnitData>();
            _gameEvents = new Subject<GameEvent>();
            
            // Создаем ReadOnly обертки
            GameState = _gameState.ToReadOnlyReactiveProperty();
            
            Debug.Log("[GameStateService] Initialized with reactive properties");
        }

        public void UpdateGameState(GameState newState)
        {
            Debug.Log($"[GameStateService] Updating game state: Turn {newState.currentTurn}, Player {newState.currentPlayer}, Phase {newState.phase}");
            
            _gameState.Value = newState;
            _gameEvents.OnNext(new GameStateChangedEvent(newState));
        }

        public void UpdateUnit(UnitData unit)
        {
            Debug.Log($"[GameStateService] Updating unit {unit.id} at position {unit.position}");
            
            var index = -1;
            for (int i = 0; i < _units.Count; i++)
            {
                if (_units[i].id == unit.id)
                {
                    index = i;
                    break;
                }
            }
            if (index >= 0)
            {
                _units[index] = unit;
            }
            else
            {
                Debug.LogWarning($"[GameStateService] Unit {unit.id} not found for update");
            }
        }

        public void RemoveUnit(int unitId)
        {
            Debug.Log($"[GameStateService] Removing unit {unitId}");
            
            var index = -1;
            for (int i = 0; i < _units.Count; i++)
            {
                if (_units[i].id == unitId)
                {
                    index = i;
                    break;
                }
            }
            if (index >= 0)
            {
                _units.RemoveAt(index);
            }
            else
            {
                Debug.LogWarning($"[GameStateService] Unit {unitId} not found for removal");
            }
        }

        public void AddUnit(UnitData unit)
        {
            Debug.Log($"[GameStateService] Adding unit {unit.id} (Type: {unit.type}, Owner: {unit.owner})");
            
            _units.Add(unit);
        }

        public UnitData? GetUnit(int unitId)
        {
            var unit = _units.FirstOrDefault(u => u.id == unitId);
            return unit.id != 0 ? unit : null;
        }

        public UnitData[] GetUnitsForPlayer(PlayerId playerId)
        {
            return _units.Where(u => u.owner == playerId).ToArray();
        }

        public int GetUnitCount(PlayerId playerId)
        {
            return _units.Count(u => u.owner == playerId);
        }
    }
}
