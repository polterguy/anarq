/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System.Linq;
using System.Threading.Tasks;
using magic.node;
using magic.node.extensions;
using com = magic.data.common;
using magic.signals.contracts;
using magic.lambda.mysql.helpers;
using magic.lambda.mysql.crud.builders;

namespace magic.lambda.mysql.crud
{
    /// <summary>
    /// The [mysql.create] slot class
    /// </summary>
    [Slot(Name = "mysql.create")]
    [Slot(Name = "wait.mysql.create")]
    public class Create : ISlot, ISlotAsync
    {
        /// <summary>
        /// Handles the signal for the class.
        /// </summary>
        /// <param name="signaler">Signaler used to signal the slot.</param>
        /// <param name="input">Root node for invocation.</param>
        public void Signal(ISignaler signaler, Node input)
        {
            /*
             * Checking if caller wants us to return the ID of the newly
             * create record.
             */
            var returnId = input.Children
                .FirstOrDefault(x => x.Name == "return-id")?.GetEx<bool>() ?? true;

            // Parsing and creating SQL.
            var exe = returnId ?
                com.SqlBuilder.Parse<SqlCreateBuilder>(signaler, input) :
                com.SqlBuilder.Parse<SqlCreateBuilderNoId>(signaler, input);

            /*
             * Notice, if the builder doesn't return a node, we are
             * not supposed to actually execute the SQL, but rather only
             * to generate it.
             */
            if (exe == null)
                return;

            // Executing SQL, now parametrized.
            com.Executor.Execute(
                exe,
                signaler.Peek<MySqlConnectionWrapper>("mysql.connect").Connection,
                signaler.Peek<com.Transaction>("mysql.transaction"),
                (cmd) =>
            {
                /*
                 * Checking if caller wants us to return the ID of the newly
                 * created record.
                 */
                if (returnId)
                {
                    input.Value = cmd.ExecuteScalar();
                }
                else
                {
                    cmd.ExecuteNonQuery();
                    input.Value = null;
                }
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
            /*
             * Checking if caller wants us to return the ID of the newly
             * create record.
             */
            var returnId = input.Children
                .FirstOrDefault(x => x.Name == "return-id")?.GetEx<bool>() ?? true;

            // Parsing and creating SQL.
            var exe = returnId ?
                com.SqlBuilder.Parse<SqlCreateBuilder>(signaler, input) :
                com.SqlBuilder.Parse<SqlCreateBuilderNoId>(signaler, input);

            /*
             * Notice, if the builder doesn't return a node, we are
             * not supposed to actually execute the SQL, but rather only
             * to generate it.
             */
            if (exe == null)
                return;

            // Executing SQL, now parametrized.
            await com.Executor.ExecuteAsync(
                exe,
                signaler.Peek<MySqlConnectionWrapper>("mysql.connect").Connection,
                signaler.Peek<com.Transaction>("mysql.transaction"),
                async (cmd) =>
            {
                /*
                 * Checking if caller wants us to return the ID of the newly
                 * created record.
                 */
                if (returnId)
                {
                    input.Value = await cmd.ExecuteScalarAsync();
                }
                else
                {
                    await cmd.ExecuteNonQueryAsync();
                    input.Value = null;
                }
                input.Clear();
            });
        }
    }
}
