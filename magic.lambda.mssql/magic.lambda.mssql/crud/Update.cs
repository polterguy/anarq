/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System.Data.SqlClient;
using System.Threading.Tasks;
using magic.node;
using com = magic.data.common;
using magic.signals.contracts;
using magic.lambda.mssql.helpers;
using magic.lambda.mssql.crud.builders;

namespace magic.lambda.mssql.crud
{
    /// <summary>
    /// [mssql.update] slot for updating a record in some table.
    /// </summary>
    [Slot(Name = "mssql.update")]
    [Slot(Name = "wait.mssql.update")]
    public class Update : ISlot, ISlotAsync
    {
        /// <summary>
        /// Implementation of your slot.
        /// </summary>
        /// <param name="signaler">Signaler used to raise the signal.</param>
        /// <param name="input">Arguments to your slot.</param>
        public void Signal(ISignaler signaler, Node input)
        {
            // Parsing and creating SQL.
            var exe = com.SqlBuilder.Parse<SqlUpdateBuilder>(signaler, input);
            if (exe == null)
                return;

            // Executing SQL, now parametrized.
            com.Executor.Execute(
                exe,
                signaler.Peek<SqlConnectionWrapper>("mssql.connect").Connection,
                signaler.Peek<com.Transaction>("mssql.transaction"),
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
            var exe = com.SqlBuilder.Parse<SqlUpdateBuilder>(signaler, input);
            if (exe == null)
                return;

            // Executing SQL, now parametrized.
            await com.Executor.ExecuteAsync(
                exe,
                signaler.Peek<SqlConnectionWrapper>("mssql.connect").Connection,
                signaler.Peek<com.Transaction>("mssql.transaction"),
                async (cmd) =>
            {
                input.Value = await cmd.ExecuteNonQueryAsync();
                input.Clear();
            });
        }
    }
}
