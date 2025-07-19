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
    /// Главный инсталлер зависимостей для игры
    /// Устанавливает все сервисы в правильном порядке
    /// Оптимизирован для тестирования NetworkGameService
    /// </summary>
    public class GameInstaller : MonoInstaller
    {
        [Header("=== КОНФИГУРАЦИЯ ===")]
        [SerializeField] 
        private GameConfig gameConfig;

        [Header("=== НАСТРОЙКИ ===")]
        [SerializeField] 
        private bool enableAdvancedFeatures = true;

        [SerializeField] 
        private bool showInstallationLog = true;

        public override void InstallBindings()
        {
            if (showInstallationLog)
            {
                Debug.Log("[GameInstaller] ==================== STARTING DEPENDENCY INSTALLATION ====================");
            }

            // Устанавливаем все зависимости в правильном порядке
            InstallConfiguration();
            InstallCoreServices();
            InstallNetworkServices();
            InstallGameplayServices();
            InstallUIServices();

            if (enableAdvancedFeatures)
            {
                InstallAdvancedServices();
            }

            if (showInstallationLog)
            {
                Debug.Log("[GameInstaller] ==================== DEPENDENCY INSTALLATION COMPLETE ====================");
            }
        }

        #region Установка зависимостей

        private void InstallConfiguration()
        {
            // Биндим конфигурацию как синглтон
            if (gameConfig != null)
            {
                Container.Bind<GameConfig>().FromInstance(gameConfig).AsSingle();
                Debug.Log($"[GameInstaller] GameConfig bound: {gameConfig.name}");
            }
            else
            {
                Debug.LogWarning("[GameInstaller] GameConfig is null, creating default configuration");
                
                // Создаем дефолтную конфигурацию
                var defaultConfig = ScriptableObject.CreateInstance<GameConfig>();
                Container.Bind<GameConfig>().FromInstance(defaultConfig).AsSingle();
            }
        }

        private void InstallCoreServices()
        {
            // Основные сервисы состояния и событий - порядок важен!
            Container.Bind<IGameStateService>().To<GameStateService>().AsSingle();
            
            Container.Bind<IGameEventsService>().To<GameEventsService>().AsSingle()
                .NonLazy(); // Инициализируем сразу для обработки событий
            
            Debug.Log("[GameInstaller] Core services installed");
        }

        private void InstallNetworkServices()
        {
            // NetworkGameService - важно правильно настроить как NetworkBehaviour
            Container.Bind<INetworkGameService>()
                .To<NetworkGameService>()
                .FromNewComponentOnNewGameObject()
                .AsSingle()
                .NonLazy(); // Принудительная инициализация для сетевого сервиса

            Debug.Log("[GameInstaller] Network services installed");
        }

        private void InstallGameplayServices()
        {
            // Основные игровые сервисы
            Container.Bind<IUnitService>().To<UnitService>().AsSingle()
                .NonLazy(); // Важно для правильной инициализации

            Container.Bind<ITurnService>().To<TurnService>().AsSingle();

            Container.Bind<IGameFieldService>().To<GameFieldService>().AsSingle();
            
            Container.Bind<IPathfindingService>().To<PathfindingService>().AsSingle();

            Container.Bind<IGameRulesService>().To<GameRulesService>().AsSingle();

            // Input Service как MonoBehaviour компонент
            Container.Bind<IInputService>().To<InputService>()
                .FromNewComponentOnNewGameObject()
                .AsSingle();

            Debug.Log("[GameInstaller] Gameplay services installed");
        }

        private void InstallUIServices()
        {
            // Основной UI сервис как MonoBehaviour компонент
            Container.Bind<IUIReactiveService>().To<UIReactiveService>()
                .FromNewComponentOnNewGameObject()
                .AsSingle();

            // Настройки игры - всегда нужны
            Container.Bind<IGameSettingsService>().To<GameSettingsService>().AsSingle();

            // Опциональные UI сервисы
            Container.Bind<IVisualizationService>().To<VisualizationService>().AsSingle();
            Container.Bind<IAudioService>().To<AudioService>().AsSingle();
            Container.Bind<IGameStatisticsService>().To<GameStatisticsService>().AsSingle();

            Debug.Log("[GameInstaller] UI services installed");
        }

        private void InstallAdvancedServices()
        {
            // Получаем конфигурацию для проверки настроек
            var config = gameConfig;

            // Продвинутые функции
            if (config != null && config.EnableAntiCheat)
            {
                Container.Bind<IServerValidationService>().To<ServerValidationService>()
                    .AsSingle();

                Debug.Log("[GameInstaller] Server validation service installed");
            }

            if (config != null && config.EnableFieldStreaming)
            {
                Container.Bind<IFieldStreamingService>().To<FieldStreamingService>()
                    .AsSingle();

                Debug.Log("[GameInstaller] Field streaming service installed");
            }

            if (config != null && config.EnableLineOfSight)
            {
                Container.Bind<ILineOfSightService>().To<LineOfSightService>().AsSingle();
                Debug.Log("[GameInstaller] Line of sight service installed");
            }

            if (config != null && config.UseGeometricTargeting)
            {
                Container.Bind<IAdvancedTargetingService>().To<AdvancedTargetingService>().AsSingle();
                Debug.Log("[GameInstaller] Advanced targeting service installed");
            }

            Debug.Log($"[GameInstaller] Advanced services configured");
        }

        #endregion

        #region Валидация и отладка

        /// <summary>
        /// Проверка корректности установки всех зависимостей
        /// Особое внимание к NetworkGameService для тестирования
        /// </summary>
        [ContextMenu("Validate Installation")]
        private bool ValidateInstallation()
        {
            if (Container == null)
            {
                Debug.LogWarning("[GameInstaller] Container not initialized");
                return false;
            }

            Debug.Log("[GameInstaller] === VALIDATING INSTALLATION ===");

            bool isValid = true;

            // Проверяем критически важные сервисы
            if (!Container.HasBinding<GameConfig>())
            {
                Debug.LogError("[GameInstaller] ❌ GameConfig not bound!");
                isValid = false;
            }
            else
            {
                Debug.Log("[GameInstaller] ✅ GameConfig bound");
            }

            if (!Container.HasBinding<IGameStateService>())
            {
                Debug.LogError("[GameInstaller] ❌ IGameStateService not bound!");
                isValid = false;
            }
            else
            {
                Debug.Log("[GameInstaller] ✅ IGameStateService bound");
            }

            if (!Container.HasBinding<IGameEventsService>())
            {
                Debug.LogError("[GameInstaller] ❌ IGameEventsService not bound!");
                isValid = false;
            }
            else
            {
                Debug.Log("[GameInstaller] ✅ IGameEventsService bound");
            }

            // КРИТИЧЕСКИ ВАЖНО для тестирования: NetworkGameService
            if (!Container.HasBinding<INetworkGameService>())
            {
                Debug.LogError("[GameInstaller] ❌ INetworkGameService not bound! NetworkGameService tests will fail!");
                isValid = false;
            }
            else
            {
                Debug.Log("[GameInstaller] ✅ INetworkGameService bound - ready for testing");
            }

            // Проверяем игровые сервисы
            if (!Container.HasBinding<IUnitService>())
            {
                Debug.LogError("[GameInstaller] ❌ IUnitService not bound!");
                isValid = false;
            }
            else
            {
                Debug.Log("[GameInstaller] ✅ IUnitService bound");
            }

            if (!Container.HasBinding<ITurnService>())
            {
                Debug.LogError("[GameInstaller] ❌ ITurnService not bound!");
                isValid = false;
            }
            else
            {
                Debug.Log("[GameInstaller] ✅ ITurnService bound");
            }

            if (isValid)
            {
                Debug.Log("[GameInstaller] 🎉 Installation validation PASSED - all critical services bound");
            }
            else
            {
                Debug.LogError("[GameInstaller] 💥 Installation validation FAILED - missing critical services");
            }

            return isValid;
        }

        /// <summary>
        /// Быстрая проверка готовности к тестированию NetworkGameService
        /// </summary>
        [ContextMenu("Check NetworkGameService Test Readiness")]
        private void CheckNetworkTestReadiness()
        {
            Debug.Log("[GameInstaller] === NETWORK TEST READINESS CHECK ===");

            bool ready = true;
            
            if (!Container.HasBinding<INetworkGameService>())
            {
                Debug.LogError("❌ INetworkGameService not bound - tests will fail!");
                ready = false;
            }

            if (!Container.HasBinding<IGameStateService>())
            {
                Debug.LogError("❌ IGameStateService not bound - needed for tests!");
                ready = false;
            }

            if (!Container.HasBinding<IGameEventsService>())
            {
                Debug.LogError("❌ IGameEventsService not bound - needed for tests!");
                ready = false;
            }

            if (!Container.HasBinding<GameConfig>())
            {
                Debug.LogError("❌ GameConfig not bound - needed for tests!");
                ready = false;
            }

            if (ready)
            {
                Debug.Log("🎉 NetworkGameService testing environment is READY!");
                Debug.Log("✅ All required dependencies are bound");
                Debug.Log("✅ You can run NetworkGameService tests in GameInitializer");
            }
            else
            {
                Debug.LogError("💥 NetworkGameService testing environment is NOT READY!");
                Debug.LogError("❌ Fix missing dependencies before running tests");
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
                
                // Попытка найти GameConfig в проекте
                #if UNITY_EDITOR
                var configs = UnityEditor.AssetDatabase.FindAssets("t:GameConfig");
                if (configs.Length > 0)
                {
                    var path = UnityEditor.AssetDatabase.GUIDToAssetPath(configs[0]);
                    Debug.Log($"[GameInstaller] Found GameConfig at: {path}");
                }
                #endif
            }
        }

        #endregion
    }
}
