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
using Zenject;
using TurnBasedGame.Core;
using TurnBasedGame.Core.Interfaces;
using TurnBasedGame.Services;

namespace TurnBasedGame.Installers
{
    /// <summary>
    /// Основной инсталлер зависимостей для игры
    /// Настраивает все сервисы через Zenject DI
    /// </summary>
    public class GameInstaller : MonoInstaller
    {
        [Header("=== КОНФИГУРАЦИЯ ===")]
        [SerializeField] 
        private GameConfig gameConfig;

        public override void InstallBindings()
        {
            Debug.Log("[GameInstaller] Installing dependencies...");

            // === КОНФИГУРАЦИЯ ===
            InstallConfiguration();

            // === ОСНОВНЫЕ СЕРВИСЫ ===
            InstallCoreServices();

            // === ИГРОВЫЕ СЕРВИСЫ ===
            InstallGameServices();

            // === UI СЕРВИСЫ ===
            InstallUIServices();

            // === ОПЦИОНАЛЬНЫЕ ПРОДВИНУТЫЕ СЕРВИСЫ ===
            if (gameConfig != null)
            {
                InstallAdvancedServices();
            }

            Debug.Log("[GameInstaller] All dependencies installed successfully!");
        }

        private void InstallConfiguration()
        {
            // Биндим конфигурацию как синглтон
            if (gameConfig != null)
            {
                Container.Bind<GameConfig>().FromInstance(gameConfig).AsSingle();
                Debug.Log("[GameInstaller] GameConfig bound");
            }
            else
            {
                Debug.LogWarning("[GameInstaller] GameConfig is null, using default settings");
                // Создаем дефолтную конфигурацию
                var defaultConfig = ScriptableObject.CreateInstance<GameConfig>();
                Container.Bind<GameConfig>().FromInstance(defaultConfig).AsSingle();
            }
        }

        private void InstallCoreServices()
        {
            // Основные сервисы состояния и событий
            Container.Bind<IGameStateService>().To<GameStateService>().AsSingle();
            Container.Bind<IGameEventsService>().To<GameEventsService>().AsSingle();
            
            Debug.Log("[GameInstaller] Core services installed");
        }

        private void InstallGameServices()
        {
            // Сетевые сервисы - NetworkGameService это MonoBehaviour, поэтому используем FromNewComponentOnNewGameObject
            Container.Bind<INetworkGameService>().To<NetworkGameService>().FromNewComponentOnNewGameObject().AsSingle();
            
            // Игровая логика
            Container.Bind<IUnitService>().To<UnitService>().AsSingle();
            Container.Bind<ITurnService>().To<TurnService>().AsSingle();
            Container.Bind<IGameFieldService>().To<GameFieldService>().AsSingle();
            Container.Bind<IInputService>().To<InputService>().FromNewComponentOnNewGameObject().AsSingle();
            Container.Bind<IPathfindingService>().To<PathfindingService>().AsSingle();
            Container.Bind<IGameRulesService>().To<GameRulesService>().AsSingle();
            
            Debug.Log("[GameInstaller] Game services installed");
        }

        private void InstallUIServices()
        {
            // UI и визуализация - UIReactiveService это MonoBehaviour
            Container.Bind<IUIReactiveService>().To<UIReactiveService>().FromNewComponentOnNewGameObject().AsSingle();
            Container.Bind<IVisualizationService>().To<VisualizationService>().AsSingle();
            Container.Bind<IAudioService>().To<AudioService>().AsSingle();
            Container.Bind<IGameStatisticsService>().To<GameStatisticsService>().AsSingle();
            Container.Bind<IGameSettingsService>().To<GameSettingsService>().AsSingle();
            
            Debug.Log("[GameInstaller] UI services installed");
        }

        private void InstallAdvancedServices()
        {
            // Продвинутые опциональные функции
            if (gameConfig.EnableFieldStreaming)
            {
                Container.Bind<IFieldStreamingService>().To<FieldStreamingService>().AsSingle();
                Debug.Log("[GameInstaller] Field streaming service installed");
            }

            if (gameConfig.EnableAntiCheat)
            {
                Container.Bind<IServerValidationService>().To<ServerValidationService>().AsSingle();
                Debug.Log("[GameInstaller] Server validation service installed");
            }

            if (gameConfig.EnableLineOfSight)
            {
                Container.Bind<ILineOfSightService>().To<LineOfSightService>().AsSingle();
                Debug.Log("[GameInstaller] Line of sight service installed");
            }

            if (gameConfig.UseGeometricTargeting)
            {
                Container.Bind<IAdvancedTargetingService>().To<AdvancedTargetingService>().AsSingle();
                Debug.Log("[GameInstaller] Advanced targeting service installed");
            }
        }

        /// <summary>
        /// Валидация в эдиторе
        /// </summary>
        private void OnValidate()
        {
            if (gameConfig == null)
            {
                Debug.LogWarning("[GameInstaller] GameConfig is not assigned! Please assign a GameConfig asset.");
            }
        }
    }
}
