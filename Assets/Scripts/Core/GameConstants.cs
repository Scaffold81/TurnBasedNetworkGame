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

namespace TurnBasedGame.Core
{
    /// <summary>
    /// Игровые константы согласно ТЗ
    /// </summary>
    public static class GameConstants
    {
        // === ОСНОВНЫЕ ПРАВИЛА ИГРЫ ===
        
        /// <summary>
        /// Время на ход в секундах (согласно ТЗ - 60 секунд)
        /// </summary>
        public const float TURN_DURATION = 60f;
        
        /// <summary>
        /// Номер хода для активации правил ничьей (согласно ТЗ - ход 15)
        /// </summary>
        public const int DRAW_RESOLUTION_TURN = 15;
        
        /// <summary>
        /// Максимальное количество действий за ход (согласно ТЗ - движение + атака)
        /// </summary>
        public const int MAX_ACTIONS_PER_TURN = 2;

        // === ХАРАКТЕРИСТИКИ ЮНИТОВ ===
        
        /// <summary>
        /// Здоровье всех юнитов (согласно ТЗ - 1 очко здоровья)
        /// </summary>
        public const int UNIT_HEALTH = 1;
        
        /// <summary>
        /// Урон всех юнитов (согласно ТЗ - 1 единица урона)
        /// </summary>
        public const int UNIT_DAMAGE = 1;
        
        /// <summary>
        /// Скорость дальнобойного юнита (медленный)
        /// </summary>
        public const int RANGED_UNIT_SPEED = 2;
        
        /// <summary>
        /// Дальность атаки дальнобойного юнита
        /// </summary>
        public const int RANGED_UNIT_ATTACK_RANGE = 4;
        
        /// <summary>
        /// Скорость ближнего бойца (быстрый)
        /// </summary>
        public const int MELEE_UNIT_SPEED = 4;
        
        /// <summary>
        /// Дальность атаки ближнего бойца (малый радиус)
        /// </summary>
        public const int MELEE_UNIT_ATTACK_RANGE = 1;
        
        /// <summary>
        /// Размер юнита для геометрических расчетов
        /// </summary>
        public const float UNIT_SIZE = 1f;
        
        /// <summary>
        /// Дальность видимости юнитов для стриминга поля
        /// </summary>
        public const int UNIT_VISION_RANGE = 5;

        // === РАЗМЕРЫ ПОЛЯ ===
        
        /// <summary>
        /// Ширина игрового поля по умолчанию
        /// </summary>
        public const int DEFAULT_FIELD_WIDTH = 20;
        
        /// <summary>
        /// Высота игрового поля по умолчанию
        /// </summary>
        public const int DEFAULT_FIELD_HEIGHT = 20;
        
        /// <summary>
        /// Количество юнитов каждого типа в стартовом наборе
        /// </summary>
        public const int STARTING_UNITS_PER_TYPE = 2;

        // === АНТИ-ЧИТ НАСТРОЙКИ ===
        
        /// <summary>
        /// Максимальное количество действий в секунду (анти-спам)
        /// </summary>
        public const int MAX_ACTIONS_PER_SECOND = 5;
        
        /// <summary>
        /// Время для определения спама в секундах
        /// </summary>
        public const float SPAM_DETECTION_WINDOW = 1f;
        
        /// <summary>
        /// Допустимая погрешность времени движения в секундах
        /// </summary>
        public const float MOVEMENT_TIME_TOLERANCE = 0.5f;

        // === СЕТЕВЫЕ НАСТРОЙКИ ===
        
        /// <summary>
        /// Максимальное количество клеток для запроса стриминга за раз
        /// </summary>
        public const int MAX_STREAMING_CELLS_PER_REQUEST = 100;
        
        /// <summary>
        /// Частота обновления таймера хода в секундах
        /// </summary>
        public const float TURN_TIMER_UPDATE_RATE = 0.1f;

        // === UI НАСТРОЙКИ ===
        
        /// <summary>
        /// Время анимации движения юнита в секундах
        /// </summary>
        public const float UNIT_MOVE_ANIMATION_DURATION = 0.5f;
        
        /// <summary>
        /// Время анимации атаки юнита в секундах
        /// </summary>
        public const float UNIT_ATTACK_ANIMATION_DURATION = 0.3f;
        
        /// <summary>
        /// Время показа эффекта уничтожения в секундах
        /// </summary>
        public const float DESTRUCTION_EFFECT_DURATION = 1f;

        // === ЦВЕТА ===
        
        /// <summary>
        /// Цвет юнитов первого игрока
        /// </summary>
        public static readonly UnityEngine.Color PLAYER1_COLOR = UnityEngine.Color.blue;
        
        /// <summary>
        /// Цвет юнитов второго игрока
        /// </summary>
        public static readonly UnityEngine.Color PLAYER2_COLOR = UnityEngine.Color.red;
        
        /// <summary>
        /// Цвет выделения выбранного юнита
        /// </summary>
        public static readonly UnityEngine.Color SELECTION_COLOR = UnityEngine.Color.yellow;
        
        /// <summary>
        /// Цвет отображения пути движения
        /// </summary>
        public static readonly UnityEngine.Color PATH_COLOR = UnityEngine.Color.green;
        
        /// <summary>
        /// Цвет отображения радиуса атаки
        /// </summary>
        public static readonly UnityEngine.Color ATTACK_RANGE_COLOR = UnityEngine.Color.red;

        // === СЛОИ И ТЕГИ ===
        
        /// <summary>
        /// Имя слоя для игрового поля
        /// </summary>
        public const string FIELD_LAYER = "GameField";
        
        /// <summary>
        /// Имя слоя для юнитов
        /// </summary>
        public const string UNITS_LAYER = "Units";
        
        /// <summary>
        /// Имя слоя для препятствий
        /// </summary>
        public const string OBSTACLES_LAYER = "Obstacles";
        
        /// <summary>
        /// Тег для юнитов
        /// </summary>
        public const string UNIT_TAG = "Unit";
        
        /// <summary>
        /// Тег для препятствий
        /// </summary>
        public const string OBSTACLE_TAG = "Obstacle";

        // === ПУТИ К РЕСУРСАМ ===
        
        /// <summary>
        /// Путь к префабам юнитов
        /// </summary>
        public const string UNITS_PREFABS_PATH = "Prefabs/Units/";
        
        /// <summary>
        /// Путь к префабам препятствий
        /// </summary>
        public const string OBSTACLES_PREFABS_PATH = "Prefabs/Obstacles/";
        
        /// <summary>
        /// Путь к UI префабам
        /// </summary>
        public const string UI_PREFABS_PATH = "Prefabs/UI/";
        
        /// <summary>
        /// Путь к звуковым файлам
        /// </summary>
        public const string AUDIO_PATH = "Audio/";
    }
}
