/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System.Threading.Tasks;
using magic.node;
using magic.data.common;
using magic.signals.contracts;
using magic.lambda.mssql.helpers;

namespace magic.lambda.mssql
{
    /// <summary>
    /// [mssql.execute] slot for executing a non query type of SQL.
    /// </summary>
    [Slot(Name = "mssql.execute")]
    [Slot(Name = "wait.mssql.execute")]
    public class Execute : ISlot, ISlotAsync
    {
        /// <summary>
        /// Implementation of your slot.
        /// </summary>
        /// <param name="signaler">Signaler used to raise the signal.</param>
        /// <param name="input">Arguments to your slot.</param>
        public void Signal(ISignaler signaler, Node input)
        {
            Executor.Execute(
                input,
                signaler.Peek<SqlConnectionWrapper>("mssql.connect").Connection,
                signaler.Peek<Transaction>("mssql.transaction"),
                (cmd) =>
            {
                input.Value = cmd.ExecuteNonQuery();
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
            await Executor.ExecuteAsync(
                input,
                signaler.Peek<SqlConnectionWrapper>("mssql.connect").Connection,
                signaler.Peek<Transaction>("mssql.transaction"),
                async (cmd) =>
            {
                input.Value = await cmd.ExecuteNonQueryAsync();
            });
        }
    }
}
