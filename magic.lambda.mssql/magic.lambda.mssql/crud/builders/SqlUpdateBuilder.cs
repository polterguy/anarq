/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using magic.node;
using com = magic.data.common;
using magic.signals.contracts;

namespace magic.lambda.mssql.crud.builders
{
    /// <summary>
    /// Builder to create an update type of MS SQL Server statement.
    /// </summary>
    public class SqlUpdateBuilder : com.SqlUpdateBuilder
    {
        /// <summary>
        /// Creates an instance of your type.
        /// </summary>
        /// <param name="node">Arguments to create your statement from.</param>
        /// <param name="signaler">Signaler used to invoke the original slot.</param>
        public SqlUpdateBuilder(Node node, ISignaler signaler)
            : base(node, "\"")
        { }
    }
}
