﻿/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using magic.node;
using magic.node.extensions;
using magic.signals.contracts;
using magic.lambda.mssql.helpers;

namespace magic.lambda.mssql
{
    /// <summary>
    /// [mssql.execute-batch] slot for executing a batch type of SQL script.
    /// 
    /// A batch script is typically a create database schema SQL file, which contains "GO" commands within it.
    /// This SQL cannot be executed in Microsoft SQL server using a simple SqlCommand.
    /// </summary>
    [Slot(Name = "mssql.execute-batch")]
    [Slot(Name = "wait.mssql.execute-batch")]
    public class ExecuteBatch : ISlot, ISlotAsync
    {
        /// <summary>
        /// Implementation of your slot.
        /// </summary>
        /// <param name="signaler">Signaler used to raise the signal.</param>
        /// <param name="input">Arguments to your slot.</param>
        public void Signal(ISignaler signaler, Node input)
        {
            var connection = signaler.Peek<SqlConnectionWrapper>("mssql.connect").Connection;
            foreach (var idxBatch in GetBatches(input.GetEx<string>()))
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = idxBatch;
                    var result = cmd.ExecuteNonQuery();
                    var resultNode = new Node();
                    resultNode.Add(new Node("Records affected", result));
                    input.Add(resultNode);
                }
            }
            input.Value = null;
        }


        /// <summary>
        /// Implementation of your slot.
        /// </summary>
        /// <param name="signaler">Signaler used to raise the signal.</param>
        /// <param name="input">Arguments to your slot.</param>
        /// <returns>An awaitable task.</returns>
        public async Task SignalAsync(ISignaler signaler, Node input)
        {
            var connection = signaler.Peek<SqlConnectionWrapper>("mssql.connect").Connection;
            foreach (var idxBatch in GetBatches(input.GetEx<string>()))
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = idxBatch;
                    var result = await cmd.ExecuteNonQueryAsync();
                    var resultNode = new Node();
                    resultNode.Add(new Node("Records affected", result));
                    input.Add(resultNode);
                }
            }
            input.Value = null;
        }

        #region [ -- Private helper methods -- ]

        IEnumerable<string> GetBatches(string sql)
        {
            if (string.IsNullOrEmpty(sql))
                yield break; // Nothing to do here ...

            var lines = sql.Split('\r', '\n');
            var builder = new StringBuilder();
            foreach (var idxLine in lines)
            {
                if (idxLine == "GO")
                {
                    yield return builder.ToString();
                    builder.Clear();
                }
                else
                {
                    builder.Append(idxLine);
                    builder.Append("\r\n");
                }
            }
            if (builder.Length > 0)
                yield return builder.ToString();
        }

        #endregion
    }
}
