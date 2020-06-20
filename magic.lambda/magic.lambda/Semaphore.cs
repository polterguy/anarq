/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.Threading.Tasks;
using magic.node;
using magic.node.extensions;
using magic.signals.contracts;
using threading = System.Threading;
using System.Collections.Concurrent;

namespace magic.lambda
{
    /// <summary>
    /// [semaphore] slot, allowing you to create a semaphore,
    /// only allowing one caller entry into some lambda object at the same time.
    /// </summary>
    [Slot(Name = "semaphore")]
    [Slot(Name = "wait.semaphore")]
    public class Semaphore : ISlot, ISlotAsync
    {
        static readonly ConcurrentDictionary<string, threading.SemaphoreSlim> _semaphores =
            new ConcurrentDictionary<string, threading.SemaphoreSlim>();

        /// <summary>
        /// Implementation of signal
        /// </summary>
        /// <param name="signaler">Signaler used to signal</param>
        /// <param name="input">Parameters passed from signaler</param>
        public void Signal(ISignaler signaler, Node input)
        {
            var key = input.GetEx<string>();
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("A semaphore must have a value, used to uniquely name the object");

            var semaphore = _semaphores.GetOrAdd(key, (name) =>
            {
                return new threading.SemaphoreSlim(1);
            });
            semaphore.Wait();
            try
            {
                signaler.Signal("eval", input);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <summary>
        /// Implementation of signal
        /// </summary>
        /// <param name="signaler">Signaler used to signal</param>
        /// <param name="input">Parameters passed from signaler</param>
        /// <returns>An awaiatble task.</returns>
        public async Task SignalAsync(ISignaler signaler, Node input)
        {
            var key = input.GetEx<string>();
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("A semaphore must have a value, used to uniquely name the object");

            var semaphore = _semaphores.GetOrAdd(key, (name) =>
            {
                return new threading.SemaphoreSlim(1);
            });
            await semaphore.WaitAsync();
            try
            {
                await signaler.SignalAsync("wait.eval", input);
            }
            finally
            {
                semaphore.Release();
            }
        }

        #region [ -- Private helper methods -- ]


        #endregion
    }
}
