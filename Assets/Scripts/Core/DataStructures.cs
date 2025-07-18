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
using UnityEngine;
using Unity.Netcode;

namespace TurnBasedGame.Core
{
    /// <summary>
    /// Основное состояние игры
    /// </summary>
    [Serializable]
    public struct GameState : INetworkSerializable
    {
        public int currentTurn;
        public PlayerId currentPlayer;
        public bool canMove;
        public bool canAttack;
        public float turnTimeLeft;
        public GamePhase phase;
        public bool infiniteSpeedEnabled;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref currentTurn);
            serializer.SerializeValue(ref currentPlayer);
            serializer.SerializeValue(ref canMove);
            serializer.SerializeValue(ref canAttack);
            serializer.SerializeValue(ref turnTimeLeft);
            serializer.SerializeValue(ref phase);
            serializer.SerializeValue(ref infiniteSpeedEnabled);
        }

        public static GameState Default => new GameState
        {
            currentTurn = 1,
            currentPlayer = PlayerId.Player1,
            canMove = true,
            canAttack = true,
            turnTimeLeft = 60f,
            phase = GamePhase.WaitingForPlayers,
            infiniteSpeedEnabled = false
        };
    }

    /// <summary>
    /// Данные юнита
    /// </summary>
    [Serializable]
    public struct UnitData : INetworkSerializable
    {
        public int id;
        public PlayerId owner;
        public UnitType type;
        public Vector2Int position;
        public int health;
        public int speed;
        public int attackRange;
        public bool isSelected;
        public float size;
        public int visionRange;
        public bool hasMovedThisTurn;
        public bool hasAttackedThisTurn;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref id);
            serializer.SerializeValue(ref owner);
            serializer.SerializeValue(ref type);
            serializer.SerializeValue(ref position);
            serializer.SerializeValue(ref health);
            serializer.SerializeValue(ref speed);
            serializer.SerializeValue(ref attackRange);
            serializer.SerializeValue(ref isSelected);
            serializer.SerializeValue(ref size);
            serializer.SerializeValue(ref visionRange);
            serializer.SerializeValue(ref hasMovedThisTurn);
            serializer.SerializeValue(ref hasAttackedThisTurn);
        }

        /// <summary>
        /// Создает дальнобойного юнита согласно ТЗ
        /// </summary>
        public static UnitData CreateRanged(int id, PlayerId owner, Vector2Int position)
        {
            return new UnitData
            {
                id = id,
                owner = owner,
                type = UnitType.Ranged,
                position = position,
                health = 1, // Согласно ТЗ - у всех одно очко здоровья
                speed = 2, // Медленный
                attackRange = 4, // Дальнобойный
                isSelected = false,
                size = 1f,
                visionRange = 5,
                hasMovedThisTurn = false,
                hasAttackedThisTurn = false
            };
        }

        /// <summary>
        /// Создает ближнего бойца согласно ТЗ
        /// </summary>
        public static UnitData CreateMelee(int id, PlayerId owner, Vector2Int position)
        {
            return new UnitData
            {
                id = id,
                owner = owner,
                type = UnitType.Melee,
                position = position,
                health = 1, // Согласно ТЗ - у всех одно очко здоровья
                speed = 4, // Быстрый
                attackRange = 1, // Малый радиус атаки
                isSelected = false,
                size = 1f,
                visionRange = 3,
                hasMovedThisTurn = false,
                hasAttackedThisTurn = false
            };
        }
    }

    /// <summary>
    /// Данные игрового поля
    /// </summary>
    [Serializable]
    public struct GameField : INetworkSerializable
    {
        public int width;
        public int height;
        public Vector2Int[] player1Spawns;
        public Vector2Int[] player2Spawns;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref width);
            serializer.SerializeValue(ref height);
            serializer.SerializeValue(ref player1Spawns);
            serializer.SerializeValue(ref player2Spawns);
        }

        public static GameField Default => new GameField
        {
            width = 20,
            height = 20,
            player1Spawns = new Vector2Int[] 
            { 
                new Vector2Int(1, 1), 
                new Vector2Int(1, 2), 
                new Vector2Int(2, 1), 
                new Vector2Int(2, 2) 
            },
            player2Spawns = new Vector2Int[] 
            { 
                new Vector2Int(18, 18), 
                new Vector2Int(18, 17), 
                new Vector2Int(17, 18), 
                new Vector2Int(17, 17) 
            }
        };
    }

    /// <summary>
    /// Данные клетки поля (для стриминга)
    /// </summary>
    [Serializable]
    public struct CellData : INetworkSerializable
    {
        public Vector2Int position;
        public bool hasObstacle;
        public ObstacleType obstacleType;
        public float height;
        public bool isVisible;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref position);
            serializer.SerializeValue(ref hasObstacle);
            serializer.SerializeValue(ref obstacleType);
            serializer.SerializeValue(ref height);
            serializer.SerializeValue(ref isVisible);
        }
    }

    /// <summary>
    /// Результат валидации
    /// </summary>
    [Serializable]
    public struct ValidationResult
    {
        public bool isValid;
        public string errorMessage;
        public ValidationError errorType;

        public static ValidationResult Success() => new ValidationResult { isValid = true };
        public static ValidationResult Fail(string message, ValidationError type = ValidationError.General) => 
            new ValidationResult { isValid = false, errorMessage = message, errorType = type };
    }

    /// <summary>
    /// Запись действия игрока для анти-чит системы
    /// </summary>
    [Serializable]
    public struct ActionRecord
    {
        public ActionType type;
        public float timestamp;
        public Vector2Int position;
        public int unitId;
        public int targetId;
    }

    /// <summary>
    /// История действий игрока
    /// </summary>
    [Serializable]
    public struct PlayerActionHistory
    {
        public ActionRecord[] actions;
        public float lastActionTime;
        public int actionsThisTurn;
        public int actionsThisSecond;
    }
}
