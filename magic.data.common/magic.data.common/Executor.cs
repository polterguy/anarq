/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.Data.Common;
using System.Threading.Tasks;
using magic.node;
using magic.node.extensions;

namespace magic.data.common
{
    /// <summary>
    /// Helper class for creating and parametrizing an SQL command of some type.
    /// </summary>
    public static class Executor
    {
        /// <summary>
        /// Creates a new SQL command of some type, and parametrizes it with each
        /// child node specified in the invocation node as a key/value DB parameter -
        /// For then to invoke the specified functor lambda callback.
        /// </summary>
        /// <param name="input">Node containing SQL and parameters as children.</param>
        /// <param name="connection">Database connection.</param>
        /// <param name="transaction">Database transaction, or null if there are none.</param>
        /// <param name="functor">Lambda function responsible for executing the command somehow.</param>
        public static void Execute(
            Node input,
            DbConnection connection,
            Transaction transaction,
            Action<DbCommand> functor)
        {
            // Making sure we dispose our command after execution.
            using (var cmd = connection.CreateCommand())
            {
                // Parametrizing and decorating command.
                PrepareCommand(cmd, transaction, input);

                // Invoking lambda callback supplied by caller.
                functor(cmd);
            }
        }

        /// <summary>
        /// Creates a new SQL command of some type, and parametrizes it with each
        /// child node specified in the invocation node as a key/value DB parameter -
        /// For then to invoke the specified functor lambda callback.
        /// </summary>
        /// <param name="input">Node containing SQL and parameters as children.</param>
        /// <param name="connection">Database connection.</param>
        /// <param name="transaction">Database transaction, or null if there are none.</param>
        /// <param name="functor">Lambda function responsible for executing the command somehow.</param>
        /// <returns>An awaitable task.</returns>
        public static async Task ExecuteAsync(
            Node input,
            DbConnection connection,
            Transaction transaction,
            Func<DbCommand, Task> functor)
        {
            using (var cmd = connection.CreateCommand())
            {
                // Parametrizing and decorating command.
                PrepareCommand(cmd, transaction, input);

                // Invoking lambda callback supplied by caller.
                await functor(cmd);
            }
        }

        #region [ -- Private helper methods -- ]

        /*
         * Helper method to parametrize command with SQL parameters, in addition to
         * decorating command with the specified transaction, if any.
         */
        static void PrepareCommand(
            DbCommand cmd, 
            Transaction transaction, 
            Node input)
        {
            // Associating transaction with command.
            if (transaction != null)
                cmd.Transaction = transaction.Value;

            // Retrieves the command text.
            cmd.CommandText = input.GetEx<string>();

            // Applies the parameters, if any.
            foreach (var idxPar in input.Children)
            {
                var par = cmd.CreateParameter();
                par.ParameterName = idxPar.Name;
                par.Value = idxPar.GetEx<object>();
                cmd.Parameters.Add(par);
            }

            // Making sure we clean nodes before invoking lambda callback.
            input.Value = null;
            input.Clear();
        }

        #endregion
    }
}
