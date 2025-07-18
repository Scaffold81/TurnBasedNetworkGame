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

namespace TurnBasedGame.Core
{
    /// <summary>
    /// Конфигурация игры через ScriptableObject
    /// Позволяет настраивать параметры игры из эдитора
    /// </summary>
    [CreateAssetMenu(fileName = "GameConfig", menuName = "TurnBasedGame/Game Config")]
    public class GameConfig : ScriptableObject
    {
        [Header("=== ОСНОВНЫЕ ПРАВИЛА ИГРЫ ===")]
        
        [SerializeField, Tooltip("Время на ход в секундах")]
        private float turnDuration = GameConstants.TURN_DURATION;
        
        [SerializeField, Tooltip("Номер хода для активации правил ничьей")]
        private int drawResolutionTurn = GameConstants.DRAW_RESOLUTION_TURN;
        
        [SerializeField, Tooltip("Включить правило бесконечной скорости после ничьей")]
        private bool enableInfiniteSpeed = true;

        [Header("=== РАЗМЕРЫ ПОЛЯ ===")]
        
        [SerializeField, Tooltip("Ширина игрового поля")]
        private int fieldWidth = GameConstants.DEFAULT_FIELD_WIDTH;
        
        [SerializeField, Tooltip("Высота игрового поля")]
        private int fieldHeight = GameConstants.DEFAULT_FIELD_HEIGHT;
        
        [SerializeField, Tooltip("Количество юнитов каждого типа в стартовом наборе")]
        private int startingUnitsPerType = GameConstants.STARTING_UNITS_PER_TYPE;

        [Header("=== ХАРАКТЕРИСТИКИ ЮНИТОВ ===")]
        
        [SerializeField, Tooltip("Скорость дальнобойного юнита")]
        private int rangedUnitSpeed = GameConstants.RANGED_UNIT_SPEED;
        
        [SerializeField, Tooltip("Дальность атаки дальнобойного юнита")]
        private int rangedUnitAttackRange = GameConstants.RANGED_UNIT_ATTACK_RANGE;
        
        [SerializeField, Tooltip("Скорость ближнего бойца")]
        private int meleeUnitSpeed = GameConstants.MELEE_UNIT_SPEED;
        
        [SerializeField, Tooltip("Дальность атаки ближнего бойца")]
        private int meleeUnitAttackRange = GameConstants.MELEE_UNIT_ATTACK_RANGE;

        [Header("=== ОПЦИОНАЛЬНЫЕ ФУНКЦИИ ===")]
        
        [SerializeField, Tooltip("Включить стриминг игрового поля (не передавать всю карту сразу)")]
        private bool enableFieldStreaming = true;
        
        [SerializeField, Tooltip("Включить Line of Sight для атак")]
        private bool enableLineOfSight = true;
        
        [SerializeField, Tooltip("Включить защиту от читов на сервере")]
        private bool enableAntiCheat = true;
        
        [SerializeField, Tooltip("Использовать геометрическое определение целей без коллайдеров")]
        private bool useGeometricTargeting = true;

        [Header("=== АНТИ-ЧИТ НАСТРОЙКИ ===")]
        
        [SerializeField, Tooltip("Максимальное количество действий в секунду")]
        private int maxActionsPerSecond = GameConstants.MAX_ACTIONS_PER_SECOND;
        
        [SerializeField, Tooltip("Допустимая погрешность времени движения")]
        private float movementTimeTolerance = GameConstants.MOVEMENT_TIME_TOLERANCE;

        [Header("=== ВИЗУАЛЬНЫЕ НАСТРОЙКИ ===")]
        
        [SerializeField, Tooltip("Показывать путь движения")]
        private bool showMovementPath = true;
        
        [SerializeField, Tooltip("Показывать радиус атаки")]
        private bool showAttackRange = true;
        
        [SerializeField, Tooltip("Показывать выделение юнита")]
        private bool showUnitSelection = true;

        [Header("=== СЕТЕВЫЕ НАСТРОЙКИ ===")]
        
        [SerializeField, Tooltip("Максимальное количество клеток для стриминга за запрос")]
        private int maxStreamingCellsPerRequest = GameConstants.MAX_STREAMING_CELLS_PER_REQUEST;
        
        [SerializeField, Tooltip("Частота обновления таймера хода")]
        private float turnTimerUpdateRate = GameConstants.TURN_TIMER_UPDATE_RATE;

        // === ПУБЛИЧНЫЕ СВОЙСТВА ===
        
        public float TurnDuration => turnDuration;
        public int DrawResolutionTurn => drawResolutionTurn;
        public bool EnableInfiniteSpeed => enableInfiniteSpeed;
        
        public int FieldWidth => fieldWidth;
        public int FieldHeight => fieldHeight;
        public int StartingUnitsPerType => startingUnitsPerType;
        
        public int RangedUnitSpeed => rangedUnitSpeed;
        public int RangedUnitAttackRange => rangedUnitAttackRange;
        public int MeleeUnitSpeed => meleeUnitSpeed;
        public int MeleeUnitAttackRange => meleeUnitAttackRange;
        
        public bool EnableFieldStreaming => enableFieldStreaming;
        public bool EnableLineOfSight => enableLineOfSight;
        public bool EnableAntiCheat => enableAntiCheat;
        public bool UseGeometricTargeting => useGeometricTargeting;
        
        public int MaxActionsPerSecond => maxActionsPerSecond;
        public float MovementTimeTolerance => movementTimeTolerance;
        
        public bool ShowMovementPath => showMovementPath;
        public bool ShowAttackRange => showAttackRange;
        public bool ShowUnitSelection => showUnitSelection;
        
        public int MaxStreamingCellsPerRequest => maxStreamingCellsPerRequest;
        public float TurnTimerUpdateRate => turnTimerUpdateRate;

        // === МЕТОДЫ ===
        
        /// <summary>
        /// Создает UnitData для дальнобойного юнита с настройками из конфига
        /// </summary>
        public UnitData CreateRangedUnit(int id, PlayerId owner, Vector2Int position)
        {
            return new UnitData
            {
                id = id,
                owner = owner,
                type = UnitType.Ranged,
                position = position,
                health = GameConstants.UNIT_HEALTH,
                speed = rangedUnitSpeed,
                attackRange = rangedUnitAttackRange,
                isSelected = false,
                size = GameConstants.UNIT_SIZE,
                visionRange = GameConstants.UNIT_VISION_RANGE,
                hasMovedThisTurn = false,
                hasAttackedThisTurn = false
            };
        }

        /// <summary>
        /// Создает UnitData для ближнего бойца с настройками из конфига
        /// </summary>
        public UnitData CreateMeleeUnit(int id, PlayerId owner, Vector2Int position)
        {
            return new UnitData
            {
                id = id,
                owner = owner,
                type = UnitType.Melee,
                position = position,
                health = GameConstants.UNIT_HEALTH,
                speed = meleeUnitSpeed,
                attackRange = meleeUnitAttackRange,
                isSelected = false,
                size = GameConstants.UNIT_SIZE,
                visionRange = GameConstants.UNIT_VISION_RANGE,
                hasMovedThisTurn = false,
                hasAttackedThisTurn = false
            };
        }

        /// <summary>
        /// Создает стандартное игровое поле согласно настройкам
        /// </summary>
        public GameField CreateGameField()
        {
            // Позиции спавна для игрока 1 (левый нижний угол)
            var player1Spawns = new Vector2Int[startingUnitsPerType * 2];
            for (int i = 0; i < startingUnitsPerType; i++)
            {
                player1Spawns[i] = new Vector2Int(1, 1 + i); // Дальнобойные
                player1Spawns[i + startingUnitsPerType] = new Vector2Int(2, 1 + i); // Ближние
            }

            // Позиции спавна для игрока 2 (правый верхний угол)
            var player2Spawns = new Vector2Int[startingUnitsPerType * 2];
            for (int i = 0; i < startingUnitsPerType; i++)
            {
                player2Spawns[i] = new Vector2Int(fieldWidth - 2, fieldHeight - 2 - i); // Дальнобойные
                player2Spawns[i + startingUnitsPerType] = new Vector2Int(fieldWidth - 3, fieldHeight - 2 - i); // Ближние
            }

            return new GameField
            {
                width = fieldWidth,
                height = fieldHeight,
                player1Spawns = player1Spawns,
                player2Spawns = player2Spawns
            };
        }

        /// <summary>
        /// Валидация настроек при изменении в инспекторе
        /// </summary>
        private void OnValidate()
        {
            // Убеждаемся, что значения в разумных пределах
            turnDuration = Mathf.Clamp(turnDuration, 10f, 300f);
            drawResolutionTurn = Mathf.Clamp(drawResolutionTurn, 5, 50);
            
            fieldWidth = Mathf.Clamp(fieldWidth, 10, 100);
            fieldHeight = Mathf.Clamp(fieldHeight, 10, 100);
            startingUnitsPerType = Mathf.Clamp(startingUnitsPerType, 1, 10);
            
            rangedUnitSpeed = Mathf.Clamp(rangedUnitSpeed, 1, 20);
            rangedUnitAttackRange = Mathf.Clamp(rangedUnitAttackRange, 1, 20);
            meleeUnitSpeed = Mathf.Clamp(meleeUnitSpeed, 1, 20);
            meleeUnitAttackRange = Mathf.Clamp(meleeUnitAttackRange, 1, 20);
            
            maxActionsPerSecond = Mathf.Clamp(maxActionsPerSecond, 1, 20);
            movementTimeTolerance = Mathf.Clamp(movementTimeTolerance, 0.1f, 5f);
            
            maxStreamingCellsPerRequest = Mathf.Clamp(maxStreamingCellsPerRequest, 10, 1000);
            turnTimerUpdateRate = Mathf.Clamp(turnTimerUpdateRate, 0.05f, 1f);
        }
    }
}
