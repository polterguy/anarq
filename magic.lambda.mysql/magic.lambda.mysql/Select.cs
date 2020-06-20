/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using magic.node;
using magic.data.common;
using magic.signals.contracts;
using magic.lambda.mysql.helpers;

namespace magic.lambda.mysql
{
    /// <summary>
    /// [mysql.select] slot for executing a select type of SQL command, that returns
    /// a row set.
    /// </summary>
    [Slot(Name = "mysql.select")]
    [Slot(Name = "wait.mysql.select")]
    public class Select : ISlot, ISlotAsync
    {
        /// <summary>
        /// Handles the signal for the class.
        /// </summary>
        /// <param name="signaler">Signaler used to signal the slot.</param>
        /// <param name="input">Root node for invocation.</param>
        public void Signal(ISignaler signaler, Node input)
        {
            Executor.Execute(
                input,
                signaler.Peek<MySqlConnectionWrapper>("mysql.connect").Connection,
                signaler.Peek<Transaction>("mysql.transaction"),
                (cmd) =>
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var rowNode = new Node();
                        for (var idxCol = 0; idxCol < reader.FieldCount; idxCol++)
                        {
                            var colNode = new Node(reader.GetName(idxCol), Converter.GetValue(reader[idxCol]));
                            rowNode.Add(colNode);
                        }
                        input.Add(rowNode);
                    }
                }
            });
        }

        /// <summary>
        /// Handles the signal for the class.
        /// </summary>
        /// <param name="signaler">Signaler used to signal the slot.</param>
        /// <param name="input">Root node for invocation.</param>
        /// <returns>An awaitable task.</returns>
        public async Task SignalAsync(ISignaler signaler, Node input)
        {
            await Executor.ExecuteAsync(
                input,
                signaler.Peek<MySqlConnectionWrapper>("mysql.connect").Connection,
                signaler.Peek<Transaction>("mysql.transaction"),
                async (cmd) =>
            {
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var rowNode = new Node();
                        for (var idxCol = 0; idxCol < reader.FieldCount; idxCol++)
                        {
                            var colNode = new Node(reader.GetName(idxCol), Converter.GetValue(reader[idxCol]));
                            rowNode.Add(colNode);
                        }
                        input.Add(rowNode);
                    }
                }
            });
        }
    }
}
