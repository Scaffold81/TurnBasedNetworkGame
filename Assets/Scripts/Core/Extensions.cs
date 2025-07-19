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
using R3;

namespace TurnBasedGame.Core
{
    /// <summary>
    /// Расширения для работы с R3 и Disposables
    /// </summary>
    public static class DisposableExtensions
    {
        private static readonly Dictionary<object, CompositeDisposable> _disposables = new();

        /// <summary>
        /// Добавляет IDisposable к объекту для автоматической очистки
        /// </summary>
        public static T AddTo<T>(this T disposable, object target) where T : IDisposable
        {
            if (target == null) return disposable;

            if (!_disposables.TryGetValue(target, out var composite))
            {
                composite = new CompositeDisposable();
                _disposables[target] = composite;
            }

            composite.Add(disposable);
            return disposable;
        }

        /// <summary>
        /// Очищает все Disposables для объекта
        /// </summary>
        public static void ClearDisposables(this object target)
        {
            if (_disposables.TryGetValue(target, out var composite))
            {
                composite.Dispose();
                _disposables.Remove(target);
            }
        }
    }
}
