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

using System.Collections.Generic;
using UnityEngine;
using R3;

namespace TurnBasedGame.Core.Interfaces
{
    /// <summary>
    /// Сервис стриминга игрового поля (опциональная продвинутая функция)
    /// Не передает клиенту полную карту, а только видимые части
    /// </summary>
    public interface IFieldStreamingService
    {
        // Реактивные события
        Observable<CellData[]> FieldDataReceived { get; }
        
        // Запрос данных поля
        void RequestFieldData(Vector2Int[] positions);
        
        // Локальный кэш
        CellData? GetCellData(Vector2Int position);
        bool IsCellKnown(Vector2Int position);
        
        // Видимость
        HashSet<Vector2Int> GetVisibleCells();
        void UpdateVisibility(Vector2Int[] unitPositions, int[] visionRanges);
    }

    /// <summary>
    /// Сервис валидации на сервере (защита от читов)
    /// </summary>
    public interface IServerValidationService
    {
        // Валидация действий
        ValidationResult ValidateMove(int unitId, Vector2Int targetPosition, PlayerId playerId);
        ValidationResult ValidateAttack(int attackerId, int targetId, PlayerId playerId);
        ValidationResult ValidateEndTurn(PlayerId playerId);
        
        // Анти-спам защита
        bool IsSpamming(PlayerId playerId);
        void RecordAction(PlayerId playerId, ActionType actionType, Vector2Int position);
        
        // Анти-чит проверки
        bool DetectSpeedHack(PlayerId playerId, Vector2Int[] path, float timeTaken);
        bool DetectTeleportHack(PlayerId playerId, Vector2Int from, Vector2Int to);
        void ReportSuspiciousActivity(PlayerId playerId, CheatType cheatType, string details);
    }

    /// <summary>
    /// Сервис Line of Sight для атак (опциональная функция)
    /// </summary>
    public interface ILineOfSightService
    {
        // Основная проверка
        bool HasLineOfSight(Vector2Int from, Vector2Int to, int maxRange);
        bool HasObstacleBetween(Vector2Int from, Vector2Int to);
        
        // Детальная геометрия
        Vector2Int[] GetLinePoints(Vector2Int from, Vector2Int to);
        bool IsPointBlocked(Vector2Int point);
        
        // Работа с размерами юнитов
        bool HasLineOfSightWithSizes(Vector2Int from, Vector2Int to, float fromSize, float toSize, int maxRange);
    }

    /// <summary>
    /// Продвинутый сервис таргетинга (геометрическое определение целей)
    /// </summary>
    public interface IAdvancedTargetingService
    {
        // Поиск целей с учетом геометрии
        int[] GetTargetsInRange(Vector2Int attackerPosition, int attackRange, PlayerId excludePlayer, float attackerSize = 1f);
        int[] GetTargetsInArea(Vector2Int center, int radius, PlayerId excludePlayer);
        
        // Геометрические проверки без коллайдеров (согласно ТЗ)
        bool IsUnitInRange(Vector2Int attackerPos, UnitData target, int range, float attackerSize = 1f);
        bool IsCircleIntersectingUnit(Vector2Int circleCenter, int radius, UnitData unit);
        
        // Вычисление дистанции с учетом размеров
        float CalculateDistanceWithSizes(Vector2Int pos1, Vector2Int pos2, float size1, float size2);
    }

    /// <summary>
    /// Сервис правил игры (механика разрешения ничьей)
    /// </summary>
    public interface IGameRulesService
    {
        // Проверка условий завершения игры
        void CheckGameEndConditions();
        void CheckDrawResolution();
        
        // Специальные правила согласно ТЗ
        void ActivateInfiniteSpeed();
        bool ShouldActivateInfiniteSpeed(int currentTurn, int player1Units, int player2Units);
        
        // Подсчет победителя
        PlayerId DetermineWinner();
        EndReason GetEndReason();
    }

    /// <summary>
    /// Сервис поиска пути (A* или другой алгоритм)
    /// </summary>
    public interface IPathfindingService
    {
        // Основной поиск пути
        Vector2Int[] FindPath(Vector2Int start, Vector2Int goal);
        Vector2Int[] FindPath(Vector2Int start, Vector2Int goal, int maxDistance);
        
        // Проверка возможности движения
        bool IsPathValid(Vector2Int[] path, int maxDistance);
        bool IsPositionWalkable(Vector2Int position);
        
        // Вспомогательные методы
        int CalculatePathLength(Vector2Int[] path);
        Vector2Int[] OptimizePath(Vector2Int[] path);
        
        // Учет других юнитов
        Vector2Int[] FindPathAvoidingUnits(Vector2Int start, Vector2Int goal, Vector2Int[] unitPositions);
    }

    /// <summary>
    /// Сервис анимаций и визуальных эффектов
    /// </summary>
    public interface IVisualizationService
    {
        // Анимации юнитов
        Observable<bool> AnimateUnitMovement(int unitId, Vector2Int[] path);
        Observable<bool> AnimateUnitAttack(int attackerId, int targetId);
        Observable<bool> AnimateUnitDestruction(int unitId);
        
        // Визуальные индикаторы
        void ShowMovementPath(Vector2Int[] path);
        void HideMovementPath();
        void ShowAttackRange(Vector2Int center, int range);
        void HideAttackRange();
        void ShowUnitSelection(int unitId);
        void HideUnitSelection();
        
        // Эффекты
        void PlayAttackEffect(Vector2Int position);
        void PlayDestructionEffect(Vector2Int position);
        void PlayTurnChangeEffect(PlayerId newPlayer);
    }

    /// <summary>
    /// Сервис аудио (звуки и музыка)
    /// </summary>
    public interface IAudioService
    {
        // Игровые звуки
        void PlayUnitMoveSound();
        void PlayUnitAttackSound();
        void PlayUnitDestroySound();
        void PlayTurnChangeSound();
        void PlayGameEndSound(PlayerId winner);
        
        // Музыка
        void PlayBackgroundMusic();
        void StopBackgroundMusic();
        
        // Управление громкостью
        void SetMasterVolume(float volume);
        void SetSfxVolume(float volume);
        void SetMusicVolume(float volume);
    }

    /// <summary>
    /// Сервис настроек игры
    /// </summary>
    public interface IGameSettingsService
    {
        // Игровые настройки
        float TurnDuration { get; set; }
        bool InfiniteSpeedEnabled { get; set; }
        bool LineOfSightEnabled { get; set; }
        bool AntiCheatEnabled { get; set; }
        
        // Визуальные настройки
        bool ShowMovementPath { get; set; }
        bool ShowAttackRange { get; set; }
        bool ShowUnitSelection { get; set; }
        
        // Сохранение/загрузка
        void SaveSettings();
        void LoadSettings();
        void ResetToDefaults();
        
        // Реактивные уведомления об изменениях
        Observable<string> SettingChanged { get; }
    }

    /// <summary>
    /// Сервис статистики игры
    /// </summary>
    public interface IGameStatisticsService
    {
        // Статистика текущей игры
        int GetUnitsDestroyed(PlayerId playerId);
        int GetUnitsLost(PlayerId playerId);
        int GetTotalMoves(PlayerId playerId);
        int GetTotalAttacks(PlayerId playerId);
        float GetAverageActionTime(PlayerId playerId);
        
        // Общая статистика
        void RecordGameResult(PlayerId winner, EndReason reason, int totalTurns);
        int GetGamesPlayed();
        int GetGamesWon(PlayerId playerId);
        float GetWinRate(PlayerId playerId);
        
        // Экспорт статистики
        string ExportStatistics();
        void ResetStatistics();
    }
}
