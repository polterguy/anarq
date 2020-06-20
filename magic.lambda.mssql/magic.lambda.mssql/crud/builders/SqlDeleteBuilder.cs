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
    /// Builder for creating a delete type of MS SQL Server statement.
    /// </summary>
    public class SqlDeleteBuilder : com.SqlDeleteBuilder
    {
        /// <summary>
        /// Creates an instance of your type.
        /// </summary>
        /// <param name="node">Arguments to build your SQL from.</param>
        /// <param name="signaler">Signaler used to invoke the original slot.</param>
        public SqlDeleteBuilder(Node node, ISignaler signaler)
            : base(node, "\"")
        { }
    }
}
