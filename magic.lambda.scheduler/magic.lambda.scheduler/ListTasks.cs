/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.Linq;
using magic.node;
using magic.signals.contracts;
using magic.lambda.scheduler.utilities;

namespace magic.lambda.scheduler
{
    /// <summary>
    /// [scheduler.tasks.list] slot that will return the names of all tasks in the system.
    /// </summary>
    [Slot(Name = "scheduler.tasks.list")]
    public class ListTasks : ISlot
    {
        readonly Scheduler _scheduler;

        /// <summary>
        /// Creates a new instance of your slot.
        /// </summary>
        /// <param name="scheduler">Which background service to use.</param>
        public ListTasks(Scheduler scheduler)
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
            var jobs = _scheduler.List();
            jobs.Sort((lhs, rhs) => lhs.Due.CompareTo(rhs.Due));
            input.AddRange(jobs.Select(x =>
            {
                return new Node("", null, new Node[]
                {
                    new Node("name", x.Name),
                    new Node("due", x.Due),
                    new Node("description", x.Description)
                });
            }));
        }
    }
}
