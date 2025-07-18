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
using R3;
using TurnBasedGame.Core;
using TurnBasedGame.Core.Interfaces;

namespace TurnBasedGame.Services
{
    /// <summary>
    /// Заглушки UI и вспомогательных сервисов
    /// TODO: Реализовать полную функциональность
    /// </summary>

    public class UIReactiveService : MonoBehaviour, IUIReactiveService
    {
        private void Start() => Debug.Log("[UIReactiveService] Initialized (Stub)");
        public void InitializeBindings() => Debug.Log("[UIReactiveService] InitializeBindings (STUB)");
        public void CleanupBindings() => Debug.Log("[UIReactiveService] CleanupBindings (STUB)");
        public void ShowGameUI() => Debug.Log("[UIReactiveService] ShowGameUI (STUB)");
        public void HideGameUI() => Debug.Log("[UIReactiveService] HideGameUI (STUB)");
        public void ShowGameEndScreen(PlayerId winner, EndReason reason) => Debug.Log($"[UIReactiveService] ShowGameEndScreen: {winner}, {reason} (STUB)");
        public void UpdateTurnTimer(float timeLeft) => Debug.Log($"[UIReactiveService] UpdateTurnTimer: {timeLeft:F1}s (STUB)");
        public void UpdateCurrentPlayer(PlayerId playerId) => Debug.Log($"[UIReactiveService] UpdateCurrentPlayer: {playerId} (STUB)");
        public void UpdateActionAvailability(bool canMove, bool canAttack) => Debug.Log($"[UIReactiveService] UpdateActionAvailability: move={canMove}, attack={canAttack} (STUB)");
    }

    public class VisualizationService : IVisualizationService
    {
        public VisualizationService() => Debug.Log("[VisualizationService] Initialized (Stub)");
        
        public Observable<bool> AnimateUnitMovement(int unitId, Vector2Int[] path)
        {
            Debug.Log($"[VisualizationService] AnimateUnitMovement: unit {unitId}, path length {path?.Length ?? 0} (STUB)");
            return Observable.Return(true);
        }
        
        public Observable<bool> AnimateUnitAttack(int attackerId, int targetId)
        {
            Debug.Log($"[VisualizationService] AnimateUnitAttack: {attackerId} -> {targetId} (STUB)");
            return Observable.Return(true);
        }
        
        public Observable<bool> AnimateUnitDestruction(int unitId)
        {
            Debug.Log($"[VisualizationService] AnimateUnitDestruction: unit {unitId} (STUB)");
            return Observable.Return(true);
        }

        public void ShowMovementPath(Vector2Int[] path) => Debug.Log($"[VisualizationService] ShowMovementPath: length {path?.Length ?? 0} (STUB)");
        public void HideMovementPath() => Debug.Log("[VisualizationService] HideMovementPath (STUB)");
        public void ShowAttackRange(Vector2Int center, int range) => Debug.Log($"[VisualizationService] ShowAttackRange: {center}, range {range} (STUB)");
        public void HideAttackRange() => Debug.Log("[VisualizationService] HideAttackRange (STUB)");
        public void ShowUnitSelection(int unitId) => Debug.Log($"[VisualizationService] ShowUnitSelection: unit {unitId} (STUB)");
        public void HideUnitSelection() => Debug.Log("[VisualizationService] HideUnitSelection (STUB)");
        public void PlayAttackEffect(Vector2Int position) => Debug.Log($"[VisualizationService] PlayAttackEffect at {position} (STUB)");
        public void PlayDestructionEffect(Vector2Int position) => Debug.Log($"[VisualizationService] PlayDestructionEffect at {position} (STUB)");
        public void PlayTurnChangeEffect(PlayerId newPlayer) => Debug.Log($"[VisualizationService] PlayTurnChangeEffect: {newPlayer} (STUB)");
    }

    public class AudioService : IAudioService
    {
        public AudioService() => Debug.Log("[AudioService] Initialized (Stub)");
        public void PlayUnitMoveSound() => Debug.Log("[AudioService] PlayUnitMoveSound (STUB)");
        public void PlayUnitAttackSound() => Debug.Log("[AudioService] PlayUnitAttackSound (STUB)");
        public void PlayUnitDestroySound() => Debug.Log("[AudioService] PlayUnitDestroySound (STUB)");
        public void PlayTurnChangeSound() => Debug.Log("[AudioService] PlayTurnChangeSound (STUB)");
        public void PlayGameEndSound(PlayerId winner) => Debug.Log($"[AudioService] PlayGameEndSound: {winner} (STUB)");
        public void PlayBackgroundMusic() => Debug.Log("[AudioService] PlayBackgroundMusic (STUB)");
        public void StopBackgroundMusic() => Debug.Log("[AudioService] StopBackgroundMusic (STUB)");
        public void SetMasterVolume(float volume) => Debug.Log($"[AudioService] SetMasterVolume: {volume} (STUB)");
        public void SetSfxVolume(float volume) => Debug.Log($"[AudioService] SetSfxVolume: {volume} (STUB)");
        public void SetMusicVolume(float volume) => Debug.Log($"[AudioService] SetMusicVolume: {volume} (STUB)");
    }

    public class GameStatisticsService : IGameStatisticsService
    {
        public GameStatisticsService() => Debug.Log("[GameStatisticsService] Initialized (Stub)");
        public int GetUnitsDestroyed(PlayerId playerId) { Debug.Log($"[GameStatisticsService] GetUnitsDestroyed: {playerId} -> 0 (STUB)"); return 0; }
        public int GetUnitsLost(PlayerId playerId) { Debug.Log($"[GameStatisticsService] GetUnitsLost: {playerId} -> 0 (STUB)"); return 0; }
        public int GetTotalMoves(PlayerId playerId) { Debug.Log($"[GameStatisticsService] GetTotalMoves: {playerId} -> 0 (STUB)"); return 0; }
        public int GetTotalAttacks(PlayerId playerId) { Debug.Log($"[GameStatisticsService] GetTotalAttacks: {playerId} -> 0 (STUB)"); return 0; }
        public float GetAverageActionTime(PlayerId playerId) { Debug.Log($"[GameStatisticsService] GetAverageActionTime: {playerId} -> 0 (STUB)"); return 0f; }
        public void RecordGameResult(PlayerId winner, EndReason reason, int totalTurns) => Debug.Log($"[GameStatisticsService] RecordGameResult: {winner}, {reason}, {totalTurns} turns (STUB)");
        public int GetGamesPlayed() { Debug.Log("[GameStatisticsService] GetGamesPlayed -> 0 (STUB)"); return 0; }
        public int GetGamesWon(PlayerId playerId) { Debug.Log($"[GameStatisticsService] GetGamesWon: {playerId} -> 0 (STUB)"); return 0; }
        public float GetWinRate(PlayerId playerId) { Debug.Log($"[GameStatisticsService] GetWinRate: {playerId} -> 0 (STUB)"); return 0f; }
        public string ExportStatistics() { Debug.Log("[GameStatisticsService] ExportStatistics (STUB)"); return "No statistics available (Stub)"; }
        public void ResetStatistics() => Debug.Log("[GameStatisticsService] ResetStatistics (STUB)");
    }

    public class GameSettingsService : IGameSettingsService
    {
        private readonly Subject<string> _settingChanged = new();

        public float TurnDuration { get; set; } = GameConstants.TURN_DURATION;
        public bool InfiniteSpeedEnabled { get; set; } = true;
        public bool LineOfSightEnabled { get; set; } = true;
        public bool AntiCheatEnabled { get; set; } = true;
        public bool ShowMovementPath { get; set; } = true;
        public bool ShowAttackRange { get; set; } = true;
        public bool ShowUnitSelection { get; set; } = true;

        public Observable<string> SettingChanged => _settingChanged;

        public GameSettingsService()
        {
            Debug.Log("[GameSettingsService] Initialized (Stub)");
        }

        public void SaveSettings() => Debug.Log("[GameSettingsService] SaveSettings (STUB)");
        public void LoadSettings() => Debug.Log("[GameSettingsService] LoadSettings (STUB)");
        public void ResetToDefaults() => Debug.Log("[GameSettingsService] ResetToDefaults (STUB)");
    }
}
