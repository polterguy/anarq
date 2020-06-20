/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System.Data.Common;
using magic.node;
using magic.data.common;
using magic.signals.contracts;
using System.Threading.Tasks;
using magic.lambda.mysql.helpers;

namespace magic.lambda.mysql
{
    /// <summary>
    /// [mysql.transaction.create] slot for creating a new MySQL database transaction.
    /// </summary>
    [Slot(Name = "mysql.transaction.create")]
    [Slot(Name = "wait.mysql.transaction.create")]
    public class CreateTransaction : ISlot, ISlotAsync
    {
        /// <summary>
        /// Handles the signal for the class.
        /// </summary>
        /// <param name="signaler">Signaler used to signal the slot.</param>
        /// <param name="input">Root node for invocation.</param>
        public void Signal(ISignaler signaler, Node input)
        {
            signaler.Scope(
                "mysql.transaction",
                new Transaction(signaler, signaler.Peek<MySqlConnectionWrapper>("mysql.connect").Connection),
                () => signaler.Signal("eval", input));
        }

        /// <summary>
        /// Handles the signal for the class.
        /// </summary>
        /// <param name="signaler">Signaler used to signal the slot.</param>
        /// <param name="input">Root node for invocation.</param>
        /// <returns>An awaitable task.</returns>
        public async Task SignalAsync(ISignaler signaler, Node input)
        {
            await signaler.ScopeAsync(
                "mysql.transaction",
                new Transaction(signaler, signaler.Peek<MySqlConnectionWrapper>("mysql.connect").Connection),
                async () => await signaler.SignalAsync("wait.eval", input));
        }
    }
}
