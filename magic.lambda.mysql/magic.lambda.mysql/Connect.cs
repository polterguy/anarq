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
using magic.lambda.mysql.helpers;

namespace magic.lambda.mysql
{
    /// <summary>
    /// [mysql.connect] slot for connecting to a MySQL server instance.
    /// </summary>
    [Slot(Name = "mysql.connect")]
    [Slot(Name = "wait.mysql.connect")]
    public class Connect : ISlot, ISlotAsync
    {
        readonly IConfiguration _configuration;

        /// <summary>
        /// Creates a new instance of your class.
        /// </summary>
        /// <param name="configuration">Configuration for your application.</param>
        public Connect(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Handles the signal for the class.
        /// </summary>
        /// <param name="signaler">Signaler used to signal the slot.</param>
        /// <param name="input">Root node for invocation.</param>
        public void Signal(ISignaler signaler, Node input)
        {
            using (var connection = new MySqlConnectionWrapper(GetConnectionString(input)))
            {
                signaler.Scope(
                    "mysql.connect",
                    connection,
                    () => signaler.Signal("eval", input));
                input.Value = null;
            }
        }

        /// <summary>
        /// Handles the signal for the class.
        /// </summary>
        /// <param name="signaler">Signaler used to signal the slot.</param>
        /// <param name="input">Root node for invocation.</param>
        /// <returns>An awaitable task.</returns>
        public async Task SignalAsync(ISignaler signaler, Node input)
        {
            using (var connection = new MySqlConnectionWrapper(GetConnectionString(input)))
            {
                await signaler.ScopeAsync(
                    "mysql.connect",
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
                var generic = _configuration["magic:databases:mysql:generic"];
                connectionString = generic.Replace("{database}", "information_schema");
            }
            else if (connectionString.StartsWith("[", StringComparison.InvariantCulture) &&
                connectionString.EndsWith("]", StringComparison.InvariantCulture))
            {
                var generic = _configuration["magic:databases:mysql:generic"];
                connectionString = generic.Replace("{database}", connectionString.Substring(1, connectionString.Length - 2));
            }
            else if (!connectionString.Contains(";"))
            {
                var generic = _configuration["magic:databases:mysql:generic"];
                connectionString = generic.Replace("{database}", connectionString);
            }
            return connectionString;
        }

        #endregion
    }
}
