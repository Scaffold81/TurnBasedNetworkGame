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
using System.Linq;
using UnityEngine;
using Zenject;
using TurnBasedGame.Core;
using TurnBasedGame.Core.Interfaces;

namespace TurnBasedGame.Services
{
    /// <summary>
    /// Сервис управления игровым полем
    /// Генерирует поле, размещает препятствия, управляет навигацией
    /// </summary>
    public class GameFieldService : IGameFieldService
    {
        [Inject] private IPathfindingService _pathfindingService;
        [Inject] private GameConfig _gameConfig;

        // Данные игрового поля
        private GameField _currentField;
        private HashSet<Vector2Int> _obstacles = new HashSet<Vector2Int>();
        private bool _fieldGenerated = false;

        public GameFieldService()
        {
            Debug.Log("[GameFieldService] Initialized with field generation");
        }

        /// <summary>
        /// Получение текущего игрового поля
        /// </summary>
        public GameField GetField()
        {
            if (!_fieldGenerated)
            {
                GenerateField(_gameConfig.FieldWidth, _gameConfig.FieldHeight);
            }
            
            return _currentField;
        }

        /// <summary>
        /// Генерация игрового поля согласно конфигурации
        /// </summary>
        public void GenerateField(int width, int height)
        {
            Debug.Log($"[GameFieldService] Generating field {width}x{height}");

            // Создаем базовое поле
            _currentField = new GameField
            {
                width = width,
                height = height,
                player1Spawns = GeneratePlayerSpawns(PlayerId.Player1, width, height),
                player2Spawns = GeneratePlayerSpawns(PlayerId.Player2, width, height)
            };

            // Очищаем старые препятствия
            _obstacles.Clear();

            // Размещаем препятствия
            PlaceObstacles();

            _fieldGenerated = true;

            Debug.Log($"[GameFieldService] Field generated successfully with {_obstacles.Count} obstacles");
            Debug.Log($"[GameFieldService] Player1 spawns: {string.Join(", ", _currentField.player1Spawns)}");
            Debug.Log($"[GameFieldService] Player2 spawns: {string.Join(", ", _currentField.player2Spawns)}");
        }

        /// <summary>
        /// Генерация позиций спавна для игрока
        /// </summary>
        private Vector2Int[] GeneratePlayerSpawns(PlayerId playerId, int width, int height)
        {
            var spawns = new List<Vector2Int>();
            int unitsPerType = _gameConfig.StartingUnitsPerType;

            if (playerId == PlayerId.Player1)
            {
                // Игрок 1 спавнится в левом нижнем углу
                for (int i = 0; i < unitsPerType; i++)
                {
                    // Дальнобойные юниты
                    spawns.Add(new Vector2Int(1, 1 + i));
                }
                for (int i = 0; i < unitsPerType; i++)
                {
                    // Ближние бойцы
                    spawns.Add(new Vector2Int(2, 1 + i));
                }
            }
            else if (playerId == PlayerId.Player2)
            {
                // Игрок 2 спавнится в правом верхнем углу
                for (int i = 0; i < unitsPerType; i++)
                {
                    // Дальнобойные юниты
                    spawns.Add(new Vector2Int(width - 2, height - 2 - i));
                }
                for (int i = 0; i < unitsPerType; i++)
                {
                    // Ближние бойцы  
                    spawns.Add(new Vector2Int(width - 3, height - 2 - i));
                }
            }

            return spawns.ToArray();
        }

        /// <summary>
        /// Размещение препятствий на поле
        /// </summary>
        public void PlaceObstacles()
        {
            Debug.Log("[GameFieldService] Placing obstacles on field");

            var field = GetField();
            var random = new System.Random();

            // Определяем зоны, где можно размещать препятствия
            var forbiddenZones = GetForbiddenZones();

            // Генерируем случайные препятствия
            int targetObstacles = Mathf.RoundToInt((field.width * field.height) * 0.15f); // 15% поля
            int maxAttempts = targetObstacles * 3;
            int attempts = 0;

            while (_obstacles.Count < targetObstacles && attempts < maxAttempts)
            {
                attempts++;

                int x = random.Next(1, field.width - 1); // Не размещаем по краям
                int y = random.Next(1, field.height - 1);
                var position = new Vector2Int(x, y);

                // Проверяем, что позиция не в запрещенной зоне
                if (forbiddenZones.Contains(position))
                    continue;

                // Проверяем, что препятствие не блокирует критические пути
                if (WouldBlockCriticalPaths(position))
                    continue;

                _obstacles.Add(position);
            }

            Debug.Log($"[GameFieldService] Placed {_obstacles.Count} obstacles in {attempts} attempts");
        }

        /// <summary>
        /// Получение запрещенных зон для препятствий
        /// </summary>
        private HashSet<Vector2Int> GetForbiddenZones()
        {
            var forbidden = new HashSet<Vector2Int>();
            var field = GetField();

            // Зоны спавна игроков (с буферной зоной)
            foreach (var spawn in field.player1Spawns.Concat(field.player2Spawns))
            {
                for (int dx = -2; dx <= 2; dx++)
                {
                    for (int dy = -2; dy <= 2; dy++)
                    {
                        forbidden.Add(spawn + new Vector2Int(dx, dy));
                    }
                }
            }

            // Центральная область (для обеспечения связности)
            int centerX = field.width / 2;
            int centerY = field.height / 2;
            for (int dx = -2; dx <= 2; dx++)
            {
                for (int dy = -2; dy <= 2; dy++)
                {
                    forbidden.Add(new Vector2Int(centerX + dx, centerY + dy));
                }
            }

            return forbidden;
        }

        /// <summary>
        /// Проверка, блокирует ли препятствие критические пути
        /// </summary>
        private bool WouldBlockCriticalPaths(Vector2Int obstaclePosition)
        {
            // Временно добавляем препятствие
            _obstacles.Add(obstaclePosition);

            var field = GetField();
            bool wouldBlock = false;

            // Проверяем связность между зонами спавна
            var player1Center = GetSpawnCenter(field.player1Spawns);
            var player2Center = GetSpawnCenter(field.player2Spawns);

            var path = _pathfindingService.FindPath(player1Center, player2Center);
            if (path == null || path.Length == 0)
            {
                wouldBlock = true;
            }
            else
            {
                // Проверяем, что путь не слишком длинный (детектируем блокировку)
                float directDistance = Vector2Int.Distance(player1Center, player2Center);
                float pathDistance = _pathfindingService.CalculatePathLength(path);
                
                if (pathDistance > directDistance * 2.5f) // Путь более чем в 2.5 раза длиннее прямого
                {
                    wouldBlock = true;
                }
            }

            // Убираем временное препятствие
            _obstacles.Remove(obstaclePosition);

            return wouldBlock;
        }

        /// <summary>
        /// Получение центра зоны спавна
        /// </summary>
        private Vector2Int GetSpawnCenter(Vector2Int[] spawns)
        {
            if (spawns.Length == 0)
                return Vector2Int.zero;

            int avgX = Mathf.RoundToInt((float)spawns.Average(s => s.x));
            int avgY = Mathf.RoundToInt((float)spawns.Average(s => s.y));
            return new Vector2Int(avgX, avgY);
        }

        /// <summary>
        /// Проверка валидности позиции
        /// </summary>
        public bool IsValidPosition(Vector2Int position)
        {
            var field = GetField();
            return position.x >= 0 && position.x < field.width &&
                   position.y >= 0 && position.y < field.height;
        }

        /// <summary>
        /// Проверка наличия препятствия в позиции
        /// </summary>
        public bool IsObstacleAt(Vector2Int position)
        {
            return _obstacles.Contains(position);
        }

        /// <summary>
        /// Поиск пути через PathfindingService
        /// </summary>
        public Vector2Int[] FindPath(Vector2Int from, Vector2Int to)
        {
            return _pathfindingService.FindPath(from, to);
        }

        /// <summary>
        /// Проверка линии видимости (для атак)
        /// </summary>
        public bool HasLineOfSight(Vector2Int from, Vector2Int to, int maxRange)
        {
            // Проверяем дистанцию
            float distance = Vector2Int.Distance(from, to);
            if (distance > maxRange)
                return false;

            // Проверяем препятствия между точками
            return !HasObstacleBetween(from, to);
        }

        /// <summary>
        /// Проверка препятствий между двумя точками
        /// </summary>
        private bool HasObstacleBetween(Vector2Int from, Vector2Int to)
        {
            var linePoints = GetLinePoints(from, to);

            foreach (var point in linePoints)
            {
                // Пропускаем начальную и конечную точки
                if (point == from || point == to)
                    continue;

                if (IsObstacleAt(point))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Получение точек линии (алгоритм Брезенхема)
        /// </summary>
        private List<Vector2Int> GetLinePoints(Vector2Int from, Vector2Int to)
        {
            var points = new List<Vector2Int>();

            int dx = Mathf.Abs(to.x - from.x);
            int dy = Mathf.Abs(to.y - from.y);
            int sx = from.x < to.x ? 1 : -1;
            int sy = from.y < to.y ? 1 : -1;
            int err = dx - dy;

            int x = from.x;
            int y = from.y;

            while (true)
            {
                points.Add(new Vector2Int(x, y));

                if (x == to.x && y == to.y)
                    break;

                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    x += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    y += sy;
                }
            }

            return points;
        }

        /// <summary>
        /// Вычисление расстояния между точками
        /// </summary>
        public float GetDistance(Vector2Int from, Vector2Int to)
        {
            return Vector2Int.Distance(from, to);
        }

        /// <summary>
        /// Получение позиций спавна для игрока
        /// </summary>
        public Vector2Int[] GetSpawnPositions(PlayerId playerId)
        {
            var field = GetField();
            
            return playerId switch
            {
                PlayerId.Player1 => field.player1Spawns,
                PlayerId.Player2 => field.player2Spawns,
                _ => new Vector2Int[0]
            };
        }

        /// <summary>
        /// Получение всех препятствий (для отладки)
        /// </summary>
        public Vector2Int[] GetAllObstacles()
        {
            return _obstacles.ToArray();
        }

        /// <summary>
        /// Добавление препятствия (для тестирования)
        /// </summary>
        public void AddObstacle(Vector2Int position)
        {
            if (IsValidPosition(position))
            {
                _obstacles.Add(position);
                Debug.Log($"[GameFieldService] Added obstacle at {position}");
            }
        }

        /// <summary>
        /// Удаление препятствия (для тестирования)
        /// </summary>
        public void RemoveObstacle(Vector2Int position)
        {
            if (_obstacles.Remove(position))
            {
                Debug.Log($"[GameFieldService] Removed obstacle at {position}");
            }
        }

        /// <summary>
        /// Очистка всех препятствий
        /// </summary>
        public void ClearObstacles()
        {
            _obstacles.Clear();
            Debug.Log("[GameFieldService] All obstacles cleared");
        }

        /// <summary>
        /// Получение статистики поля
        /// </summary>
        public void PrintFieldStatistics()
        {
            var field = GetField();
            Debug.Log($"=== FIELD STATISTICS ===");
            Debug.Log($"Field size: {field.width}x{field.height} ({field.width * field.height} total cells)");
            Debug.Log($"Obstacles: {_obstacles.Count} ({(_obstacles.Count / (float)(field.width * field.height) * 100):F1}% coverage)");
            Debug.Log($"Player 1 spawns: {field.player1Spawns.Length} positions");
            Debug.Log($"Player 2 spawns: {field.player2Spawns.Length} positions");
            
            // Проверяем связность
            var player1Center = GetSpawnCenter(field.player1Spawns);
            var player2Center = GetSpawnCenter(field.player2Spawns);
            var path = FindPath(player1Center, player2Center);
            
            Debug.Log($"Connectivity: {(path != null ? $"Connected (path length: {_pathfindingService.CalculatePathLength(path)})" : "DISCONNECTED!")}");
        }
    }
}
