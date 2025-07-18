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
    /// Идентификатор игрока
    /// </summary>
    public enum PlayerId
    {
        None = 0,
        Player1 = 1,
        Player2 = 2
    }

    /// <summary>
    /// Тип юнита согласно ТЗ
    /// </summary>
    public enum UnitType
    {
        None = 0,
        /// <summary>
        /// Медленный но дальнобойный
        /// </summary>
        Ranged = 1,
        /// <summary>
        /// Быстрый но с малым радиусом атаки
        /// </summary>
        Melee = 2
    }

    /// <summary>
    /// Фаза игры
    /// </summary>
    public enum GamePhase
    {
        WaitingForPlayers = 0,
        Playing = 1,
        GameOver = 2
    }

    /// <summary>
    /// Тип действия игрока
    /// </summary>
    public enum ActionType
    {
        None = 0,
        Move = 1,
        Attack = 2,
        EndTurn = 3
    }

    /// <summary>
    /// Причина завершения игры
    /// </summary>
    public enum EndReason
    {
        None = 0,
        AllEnemyUnitsDestroyed = 1,
        UnitCountAdvantage = 2,
        PlayerDisconnected = 3,
        CheatDetected = 4
    }

    /// <summary>
    /// Тип препятствия на поле
    /// </summary>
    public enum ObstacleType
    {
        None = 0,
        Wall = 1,
        Rock = 2
    }

    /// <summary>
    /// Тип ошибки валидации
    /// </summary>
    public enum ValidationError
    {
        General = 0,
        UnitNotFound = 1,
        NotYourTurn = 2,
        ActionNotAvailable = 3,
        PathTooLong = 4,
        TargetOutOfRange = 5,
        ObstacleBlocking = 6,
        TimeExpired = 7,
        Spam = 8
    }

    /// <summary>
    /// Тип обнаруженного чита
    /// </summary>
    public enum CheatType
    {
        InvalidMove = 0,
        InvalidAttack = 1,
        ActionSpam = 2,
        TimeManipulation = 3
    }
}
