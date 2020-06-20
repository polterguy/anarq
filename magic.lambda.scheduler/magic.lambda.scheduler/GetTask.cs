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
    /// [scheduler.tasks.get] slot that will return an existing task with the specified name,
    /// including its next due date.
    /// </summary>
    [Slot(Name = "scheduler.tasks.get")]
    public class GetTask : ISlot
    {
        readonly Scheduler _scheduler;

        /// <summary>
        /// Creates a new instance of your slot.
        /// </summary>
        /// <param name="scheduler">Which background service to use.</param>
        public GetTask(Scheduler scheduler)
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
            var job = _scheduler.Get(input.GetEx<string>());
            var node = job.GetNode();
            node.Add(new Node("due", job.Due));
            input.Add(node);
        }
    }
}
