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

namespace TurnBasedGame
{
    /// <summary>
    /// ZENJECT BINDINGS REFERENCE - Справочник биндингов для разных типов классов
    /// </summary>
    public static class ZenjectBindingsReference
    {
        /*
         * === ПРАВИЛА БИНДИНГА В ZENJECT ===
         * 
         * 1. ОБЫЧНЫЕ КЛАССЫ (POCO):
         *    Container.Bind<IService>().To<Service>().AsSingle();
         *    ✅ Работает для: GameStateService, GameEventsService, UnitService, etc.
         * 
         * 2. MONOBEHAVIOUR КЛАССЫ:
         *    Container.Bind<IService>().To<Service>().FromNewComponentOnNewGameObject().AsSingle();
         *    ✅ Работает для: NetworkGameService, InputService, UIReactiveService
         *    ❌ НЕ ИСПОЛЬЗУЙТЕ обычный .To<>().AsSingle() для MonoBehaviour!
         * 
         * 3. SCRIPTABLEOBJECT:
         *    Container.Bind<GameConfig>().FromInstance(configAsset).AsSingle();
         *    ✅ Работает для: GameConfig и другие SO
         * 
         * 4. NETWORKBEHAVIOUR КЛАССЫ:
         *    Container.Bind<INetworkService>().To<NetworkService>().FromNewComponentOnNewGameObject().AsSingle();
         *    ✅ NetworkBehaviour наследуется от MonoBehaviour, поэтому правила те же
         * 
         * === ИСПРАВЛЕННЫЕ ОШИБКИ ===
         * 
         * ❌ БЫЛО:
         * Container.Bind<INetworkGameService>().To<NetworkGameService>().AsSingle();
         * ОШИБКА: ZenjectException - Cannot instantiate MonoBehaviour directly
         * 
         * ✅ СТАЛО:
         * Container.Bind<INetworkGameService>().To<NetworkGameService>()
         *     .FromNewComponentOnNewGameObject().AsSingle();
         * РЕЗУЛЬТАТ: Создается новый GameObject с компонентом NetworkGameService
         * 
         * === ДОПОЛНИТЕЛЬНЫЕ ВАРИАНТЫ ===
         * 
         * Для существующего GameObject:
         * Container.Bind<IService>().To<Service>().FromComponentInNewPrefab(prefab).AsSingle();
         * 
         * Для компонента в Hierarchy:
         * Container.Bind<IService>().FromComponentInHierarchy().AsSingle();
         * 
         * Для ленивой инициализации:
         * Container.Bind<IService>().To<Service>().FromNewComponentOnNewGameObject().AsTransient();
         */
    }
}
