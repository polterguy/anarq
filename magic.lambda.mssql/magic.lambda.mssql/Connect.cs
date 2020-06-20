/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using magic.node;
using magic.node.extensions;
using magic.signals.contracts;
using magic.lambda.mssql.helpers;

namespace magic.lambda.mssql
{
    /// <summary>
    /// [mssql.connect] slot, for connecting to a MS SQL Server database instance.
    /// </summary>
    [Slot(Name = "mssql.connect")]
    [Slot(Name = "wait.mssql.connect")]
    public class Connect : ISlot, ISlotAsync
    {
        readonly IConfiguration _configuration;

        /// <summary>
        /// Creates a new instance of your type.
        /// </summary>
        /// <param name="configuration">Configuration for your application.</param>
        public Connect(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Implementation of your slot.
        /// </summary>
        /// <param name="signaler">Signaler used to raise the signal.</param>
        /// <param name="input">Arguments to your slot.</param>
        public void Signal(ISignaler signaler, Node input)
        {
            var connectionString = GetConnectionString(input);

            using (var connection = new SqlConnectionWrapper(GetConnectionString(input)))
            {
                signaler.Scope(
                    "mssql.connect",
                    connection, () => signaler.Signal("eval", input));
                input.Value = null;
            }
        }

        /// <summary>
        /// Implementation of your slot.
        /// </summary>
        /// <param name="signaler">Signaler used to raise the signal.</param>
        /// <param name="input">Arguments to your slot.</param>
        /// <returns>An awaitable task.</returns>
        public async Task SignalAsync(ISignaler signaler, Node input)
        {
            using (var connection = new SqlConnectionWrapper(GetConnectionString(input)))
            {
                await signaler.ScopeAsync(
                    "mssql.connect",
                    connection,
                    async () => await signaler.SignalAsync("wait.eval", input));
                input.Value = null;
            }
        }

        #region [ -- Private helper methods -- ]

        string GetConnectionString(Node input)
        {
            var connectionString = input.Value == null ? null : input.GetEx<string>();

            // Checking if this is a "generic connection string".
            if (string.IsNullOrEmpty(connectionString))
            {
                var generic = _configuration["magic:databases:mssql:generic"];
                connectionString = generic.Replace("{database}", "master");
            }
            else if (connectionString.StartsWith("[", StringComparison.InvariantCulture) &&
                connectionString.EndsWith("]", StringComparison.InvariantCulture))
            {
                var generic = _configuration["magic:databases:mssql:generic"];
                connectionString = generic.Replace("{database}", connectionString.Substring(1, connectionString.Length - 2));
            }
            else if (!connectionString.Contains(";"))
            {
                var generic = _configuration["magic:databases:mssql:generic"];
                connectionString = generic.Replace("{database}", connectionString);
            }
            return connectionString;
        }

        #endregion
    }
}
