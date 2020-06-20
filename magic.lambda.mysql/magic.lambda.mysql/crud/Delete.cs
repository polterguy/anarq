/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System.Threading.Tasks;
using magic.node;
using com = magic.data.common;
using magic.signals.contracts;
using magic.lambda.mysql.helpers;
using magic.lambda.mysql.crud.builders;

namespace magic.lambda.mysql.crud
{
    /// <summary>
    /// The [mysql.delete] slot class
    /// </summary>
    [Slot(Name = "mysql.delete")]
    [Slot(Name = "wait.mysql.delete")]
    public class Delete : ISlot, ISlotAsync
    {
        /// <summary>
        /// Handles the signal for the class.
        /// </summary>
        /// <param name="signaler">Signaler used to signal the slot.</param>
        /// <param name="input">Root node for invocation.</param>
        public void Signal(ISignaler signaler, Node input)
        {
            // Parsing and creating SQL.
            var exe = com.SqlBuilder.Parse<SqlDeleteBuilder>(signaler, input);
            if (exe == null)
                return;

            // Executing SQL, now parametrized.
            com.Executor.Execute(
                exe,
                signaler.Peek<MySqlConnectionWrapper>("mysql.connect").Connection,
                signaler.Peek<com.Transaction>("mysql.transaction"),
                (cmd) =>
            {
                input.Value = cmd.ExecuteNonQuery();
                input.Clear();
            });
        }

        /// <summary>
        /// Implementation of your slot.
        /// </summary>
        /// <param name="signaler">Signaler used to raise the signal.</param>
        /// <param name="input">Arguments to your slot.</param>
        /// <returns>An awaitable task.</returns>
        public async Task SignalAsync(ISignaler signaler, Node input)
        {
            // Parsing and creating SQL.
            var exe = com.SqlBuilder.Parse<SqlDeleteBuilder>(signaler, input);
            if (exe == null)
                return;

            // Executing SQL, now parametrized.
            await com.Executor.ExecuteAsync(
                exe,
                signaler.Peek<MySqlConnectionWrapper>("mysql.connect").Connection,
                signaler.Peek<com.Transaction>("mysql.transaction"),
                async (cmd) =>
            {
                input.Value = await cmd.ExecuteNonQueryAsync();
                input.Clear();
            });
        }
    }
}
