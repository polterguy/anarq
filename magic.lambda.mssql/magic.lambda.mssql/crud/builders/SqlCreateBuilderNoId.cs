/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System.Text;
using magic.node;
using com = magic.data.common;
using magic.signals.contracts;

namespace magic.lambda.mssql.crud.builders
{
    /// <summary>
    /// Create SQL type of builder for MS SQL Server types of statements,
    /// that does not return the generated record's ID.
    ///
    /// This is useful for times when you don't have auto_increment or generated
    /// IDs on your table.
    /// </summary>
    public class SqlCreateBuilderNoId : com.SqlCreateBuilder
    {
        /// <summary>
        /// Creates a new instance of your class.
        /// </summary>
        /// <param name="node">Arguments used to semantically build your SQL.</param>
        /// <param name="signaler">Signaler used to invoke the original slot.</param>
        public SqlCreateBuilderNoId(Node node, ISignaler signaler)
            : base(node, "\"")
        { }
    }
}
