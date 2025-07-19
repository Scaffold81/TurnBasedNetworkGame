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
    /// Сервис поиска пути с A* алгоритмом
    /// Поддерживает препятствия, других юнитов и ограничения дистанции
    /// </summary>
    public class PathfindingService : IPathfindingService
    {
        [Inject] private IGameFieldService _gameField;
        [Inject] private IGameStateService _gameState;
        [Inject] private GameConfig _gameConfig;

        public PathfindingService()
        {
            Debug.Log("[PathfindingService] Initialized with A* algorithm");
        }

        /// <summary>
        /// Основной поиск пути с A* алгоритмом
        /// </summary>
        public Vector2Int[] FindPath(Vector2Int start, Vector2Int goal)
        {
            return FindPath(start, goal, int.MaxValue);
        }

        /// <summary>
        /// Поиск пути с ограничением максимальной дистанции
        /// </summary>
        public Vector2Int[] FindPath(Vector2Int start, Vector2Int goal, int maxDistance)
        {
            Debug.Log($"[PathfindingService] Finding path from {start} to {goal}, max distance: {maxDistance}");

            // Проверяем базовые условия
            if (start == goal)
            {
                Debug.Log("[PathfindingService] Start equals goal, returning single point");
                return new Vector2Int[] { start };
            }

            if (!IsPositionWalkable(start))
            {
                Debug.LogWarning($"[PathfindingService] Start position {start} is not walkable!");
                return null;
            }

            if (!IsPositionWalkable(goal))
            {
                Debug.LogWarning($"[PathfindingService] Goal position {goal} is not walkable!");
                return null;
            }

            // Выполняем A* поиск
            var path = AStar(start, goal, maxDistance);
            
            if (path != null)
            {
                Debug.Log($"[PathfindingService] Path found with {path.Length} points, length: {CalculatePathLength(path)}");
                return OptimizePath(path);
            }

            Debug.Log("[PathfindingService] No path found");
            return null;
        }

        /// <summary>
        /// A* алгоритм поиска пути
        /// </summary>
        private Vector2Int[] AStar(Vector2Int start, Vector2Int goal, int maxDistance)
        {
            var openSet = new PriorityQueue<AStarNode>();
            var closedSet = new HashSet<Vector2Int>();
            var allNodes = new Dictionary<Vector2Int, AStarNode>();

            // Начальная нода
            var startNode = new AStarNode(start, 0, GetHeuristic(start, goal), null);
            openSet.Enqueue(startNode, startNode.F);
            allNodes[start] = startNode;

            while (openSet.Count > 0)
            {
                var currentNode = openSet.Dequeue();
                var currentPos = currentNode.Position;

                // Достигли цели
                if (currentPos == goal)
                {
                    return ReconstructPath(currentNode);
                }

                closedSet.Add(currentPos);

                // Проверяем всех соседей
                foreach (var neighbor in GetNeighbors(currentPos))
                {
                    if (closedSet.Contains(neighbor))
                        continue;

                    if (!IsPositionWalkable(neighbor))
                        continue;

                    var tentativeG = currentNode.G + GetMovementCost(currentPos, neighbor);

                    // Проверяем ограничение дистанции
                    if (tentativeG > maxDistance)
                        continue;

                    var neighborNode = allNodes.GetValueOrDefault(neighbor);
                    
                    if (neighborNode == null)
                    {
                        // Новая нода
                        neighborNode = new AStarNode(neighbor, tentativeG, GetHeuristic(neighbor, goal), currentNode);
                        allNodes[neighbor] = neighborNode;
                        openSet.Enqueue(neighborNode, neighborNode.F);
                    }
                    else if (tentativeG < neighborNode.G)
                    {
                        // Нашли лучший путь к этой ноде
                        neighborNode.G = tentativeG;
                        neighborNode.F = neighborNode.G + neighborNode.H;
                        neighborNode.Parent = currentNode;
                        
                        // Обновляем приоритет в очереди
                        openSet.UpdatePriority(neighborNode, neighborNode.F);
                    }
                }
            }

            return null; // Путь не найден
        }

        /// <summary>
        /// Восстановление пути из цепочки нод
        /// </summary>
        private Vector2Int[] ReconstructPath(AStarNode goalNode)
        {
            var path = new List<Vector2Int>();
            var current = goalNode;

            while (current != null)
            {
                path.Add(current.Position);
                current = current.Parent;
            }

            path.Reverse();
            return path.ToArray();
        }

        /// <summary>
        /// Получение соседних клеток (8 направлений)
        /// </summary>
        private IEnumerable<Vector2Int> GetNeighbors(Vector2Int position)
        {
            // 8 направлений (включая диагонали)
            var directions = new Vector2Int[]
            {
                new Vector2Int(0, 1),   // Север
                new Vector2Int(1, 1),   // Северо-восток
                new Vector2Int(1, 0),   // Восток
                new Vector2Int(1, -1),  // Юго-восток
                new Vector2Int(0, -1),  // Юг
                new Vector2Int(-1, -1), // Юго-запад
                new Vector2Int(-1, 0),  // Запад
                new Vector2Int(-1, 1)   // Северо-запад
            };

            foreach (var direction in directions)
            {
                var neighbor = position + direction;
                
                // Проверяем границы поля
                if (IsPositionInBounds(neighbor))
                {
                    yield return neighbor;
                }
            }
        }

        /// <summary>
        /// Проверка, что позиция находится в границах поля
        /// </summary>
        private bool IsPositionInBounds(Vector2Int position)
        {
            var field = _gameField.GetField();
            return position.x >= 0 && position.x < field.width &&
                   position.y >= 0 && position.y < field.height;
        }

        /// <summary>
        /// Эвристическая функция (Manhattan distance с учетом диагоналей)
        /// </summary>
        private float GetHeuristic(Vector2Int from, Vector2Int to)
        {
            var dx = Mathf.Abs(to.x - from.x);
            var dy = Mathf.Abs(to.y - from.y);
            
            // Octile distance (оптимизированная для 8 направлений)
            var diagonal = Mathf.Min(dx, dy);
            var straight = Mathf.Max(dx, dy) - diagonal;
            
            return diagonal * 1.414f + straight * 1.0f;
        }

        /// <summary>
        /// Стоимость перемещения между соседними клетками
        /// </summary>
        private float GetMovementCost(Vector2Int from, Vector2Int to)
        {
            // Диагональное движение стоит больше
            var dx = Mathf.Abs(to.x - from.x);
            var dy = Mathf.Abs(to.y - from.y);
            
            if (dx == 1 && dy == 1)
                return 1.414f; // Диагональ
            else
                return 1.0f;   // Прямое направление
        }

        /// <summary>
        /// Проверка, что позиция проходима
        /// </summary>
        public bool IsPositionWalkable(Vector2Int position)
        {
            if (!IsPositionInBounds(position))
                return false;

            // Проверяем препятствия
            if (_gameField.IsObstacleAt(position))
                return false;

            // Проверяем, не занято ли другим юнитом
            if (IsPositionOccupiedByUnit(position))
                return false;

            return true;
        }

        /// <summary>
        /// Проверка, занята ли позиция другим юнитом
        /// </summary>
        private bool IsPositionOccupiedByUnit(Vector2Int position)
        {
            return _gameState.Units.Any(unit => unit.position == position);
        }

        /// <summary>
        /// Поиск пути с учетом избегания конкретных юнитов
        /// </summary>
        public Vector2Int[] FindPathAvoidingUnits(Vector2Int start, Vector2Int goal, Vector2Int[] unitPositions)
        {
            Debug.Log($"[PathfindingService] Finding path avoiding {unitPositions?.Length ?? 0} units");

            // Временно добавляем позиции юнитов как препятствия
            var originalUnits = _gameState.Units.ToList();
            var avoidPositions = new HashSet<Vector2Int>(unitPositions ?? new Vector2Int[0]);

            // Ищем путь с учетом дополнительных препятствий
            var path = FindPathWithAdditionalObstacles(start, goal, avoidPositions);

            return path;
        }

        /// <summary>
        /// Поиск пути с дополнительными препятствиями
        /// </summary>
        private Vector2Int[] FindPathWithAdditionalObstacles(Vector2Int start, Vector2Int goal, HashSet<Vector2Int> additionalObstacles)
        {
            var openSet = new PriorityQueue<AStarNode>();
            var closedSet = new HashSet<Vector2Int>();
            var allNodes = new Dictionary<Vector2Int, AStarNode>();

            var startNode = new AStarNode(start, 0, GetHeuristic(start, goal), null);
            openSet.Enqueue(startNode, startNode.F);
            allNodes[start] = startNode;

            while (openSet.Count > 0)
            {
                var currentNode = openSet.Dequeue();
                var currentPos = currentNode.Position;

                if (currentPos == goal)
                {
                    return ReconstructPath(currentNode);
                }

                closedSet.Add(currentPos);

                foreach (var neighbor in GetNeighbors(currentPos))
                {
                    if (closedSet.Contains(neighbor))
                        continue;

                    // Проверяем основные препятствия
                    if (!IsPositionInBounds(neighbor) || _gameField.IsObstacleAt(neighbor))
                        continue;

                    // Проверяем дополнительные препятствия (позиции юнитов)
                    if (additionalObstacles.Contains(neighbor))
                        continue;

                    var tentativeG = currentNode.G + GetMovementCost(currentPos, neighbor);
                    var neighborNode = allNodes.GetValueOrDefault(neighbor);
                    
                    if (neighborNode == null)
                    {
                        neighborNode = new AStarNode(neighbor, tentativeG, GetHeuristic(neighbor, goal), currentNode);
                        allNodes[neighbor] = neighborNode;
                        openSet.Enqueue(neighborNode, neighborNode.F);
                    }
                    else if (tentativeG < neighborNode.G)
                    {
                        neighborNode.G = tentativeG;
                        neighborNode.F = neighborNode.G + neighborNode.H;
                        neighborNode.Parent = currentNode;
                        openSet.UpdatePriority(neighborNode, neighborNode.F);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Проверка валидности пути
        /// </summary>
        public bool IsPathValid(Vector2Int[] path, int maxDistance)
        {
            if (path == null || path.Length == 0)
                return false;

            if (path.Length == 1)
                return true;

            // Проверяем длину пути
            if (CalculatePathLength(path) > maxDistance)
                return false;

            // Проверяем каждую точку пути
            for (int i = 0; i < path.Length; i++)
            {
                if (!IsPositionWalkable(path[i]))
                    return false;
            }

            // Проверяем связность пути
            for (int i = 0; i < path.Length - 1; i++)
            {
                var distance = Vector2Int.Distance(path[i], path[i + 1]);
                if (distance > 1.5f) // Максимальная дистанция для диагонали
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Вычисление длины пути
        /// </summary>
        public int CalculatePathLength(Vector2Int[] path)
        {
            if (path == null || path.Length <= 1)
                return 0;

            float totalLength = 0;
            for (int i = 0; i < path.Length - 1; i++)
            {
                totalLength += GetMovementCost(path[i], path[i + 1]);
            }

            return Mathf.RoundToInt(totalLength);
        }

        /// <summary>
        /// Оптимизация пути (удаление лишних точек)
        /// </summary>
        public Vector2Int[] OptimizePath(Vector2Int[] path)
        {
            if (path == null || path.Length <= 2)
                return path;

            var optimized = new List<Vector2Int> { path[0] };

            for (int i = 1; i < path.Length - 1; i++)
            {
                var prev = path[i - 1];
                var current = path[i];
                var next = path[i + 1];

                // Проверяем, можно ли пропустить текущую точку
                if (!CanSkipPoint(prev, current, next))
                {
                    optimized.Add(current);
                }
            }

            optimized.Add(path[path.Length - 1]);

            Debug.Log($"[PathfindingService] Path optimized from {path.Length} to {optimized.Count} points");
            return optimized.ToArray();
        }

        /// <summary>
        /// Проверка, можно ли пропустить точку пути
        /// </summary>
        private bool CanSkipPoint(Vector2Int prev, Vector2Int current, Vector2Int next)
        {
            // Проверяем, лежат ли три точки на одной прямой
            var dir1 = current - prev;
            var dir2 = next - current;

            // Проверяем, имеют ли векторы одинаковое направление
            // Используем нормализацию через Vector2 для получения направления
            var normalizedDir1 = ((Vector2)dir1).normalized;
            var normalizedDir2 = ((Vector2)dir2).normalized;
            
            // Сравниваем направления с небольшой погрешностью
            return Vector2.Distance(normalizedDir1, normalizedDir2) < 0.01f;
        }
    }

    /// <summary>
    /// Нода для A* алгоритма
    /// </summary>
    public class AStarNode
    {
        public Vector2Int Position { get; }
        public float G { get; set; }  // Расстояние от старта
        public float H { get; }       // Эвристика до цели
        public float F { get; set; }  // G + H
        public AStarNode Parent { get; set; }

        public AStarNode(Vector2Int position, float g, float h, AStarNode parent)
        {
            Position = position;
            G = g;
            H = h;
            F = G + H;
            Parent = parent;
        }
    }

    /// <summary>
    /// Простая приоритетная очередь для A*
    /// </summary>
    public class PriorityQueue<T>
    {
        private readonly List<(T item, float priority)> _items = new();

        public int Count => _items.Count;

        public void Enqueue(T item, float priority)
        {
            _items.Add((item, priority));
            _items.Sort((a, b) => a.priority.CompareTo(b.priority));
        }

        public T Dequeue()
        {
            if (_items.Count == 0)
                throw new InvalidOperationException("Queue is empty");

            var item = _items[0].item;
            _items.RemoveAt(0);
            return item;
        }

        public void UpdatePriority(T item, float newPriority)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                if (EqualityComparer<T>.Default.Equals(_items[i].item, item))
                {
                    _items[i] = (item, newPriority);
                    _items.Sort((a, b) => a.priority.CompareTo(b.priority));
                    return;
                }
            }
        }
    }
}
