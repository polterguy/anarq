/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using magic.node;
using magic.node.extensions;
using magic.signals.contracts;
using magic.lambda.scheduler.utilities;

namespace magic.lambda.scheduler
{
    /// <summary>
    /// [scheduler.tasks.delete] slot that will delete a named task.
    /// </summary>
    [Slot(Name = "scheduler.tasks.delete")]
    public class DeleteTask : ISlot
    {
        readonly Scheduler _scheduler;

        /// <summary>
        /// Creates a new instance of your slot.
        /// </summary>
        /// <param name="scheduler">Which background service to use.</param>
        public DeleteTask(Scheduler scheduler)
        {
            _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
        }

        /// <summary>
        /// Slot implementation.
        /// </summary>
        /// <param name="signaler">Signaler that raised signal.</param>
        /// <param name="input">Arguments to slot.</param>
        public void Signal(ISignaler signaler, Node input)
        {
            _scheduler.Delete(input.GetEx<string>());
        }
    }
}
