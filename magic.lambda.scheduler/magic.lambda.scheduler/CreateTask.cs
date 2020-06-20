/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using magic.node;
using magic.signals.contracts;
using magic.lambda.scheduler.utilities;
using magic.lambda.scheduler.utilities.jobs;

namespace magic.lambda.scheduler
{
    /// <summary>
    /// [scheduler.tasks.create] slot that will create a new scheduled task.
    /// </summary>
    [Slot(Name = "scheduler.tasks.create")]
    public class CreateTask : ISlot
    {
        readonly Scheduler _scheduler;

        /// <summary>
        /// Creates a new instance of your slot.
        /// </summary>
        /// <param name="scheduler">Which background service to use.</param>
        public CreateTask(Scheduler scheduler)
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
            _scheduler.Create(input);
        }
    }
}
