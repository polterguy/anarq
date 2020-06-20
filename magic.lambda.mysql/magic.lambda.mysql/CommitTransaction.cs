/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using magic.node;
using magic.data.common;
using magic.signals.contracts;

namespace magic.lambda.mysql
{
    /// <summary>
    /// [mysql.transaction.commit] slot for committing the top level MySQL
    /// database transaction.
    /// </summary>
    [Slot(Name = "mysql.transaction.commit")]
    public class CommitTransaction : ISlot
    {
        /// <summary>
        /// Handles the signal for the class.
        /// </summary>
        /// <param name="signaler">Signaler used to signal the slot.</param>
        /// <param name="input">Root node for invocation.</param>
        public void Signal(ISignaler signaler, Node input)
        {
            signaler.Peek<Transaction>("mysql.transaction").Commit();
        }
    }
}
