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
    /// Create SQL type of builder for MS SQL Server types of statements.
    /// </summary>
    public class SqlCreateBuilder : com.SqlCreateBuilder
    {
        /// <summary>
        /// Creates a new instance of your class.
        /// </summary>
        /// <param name="node">Arguments used to semantically build your SQL.</param>
        /// <param name="signaler">Signaler used to invoke the original slot.</param>
        public SqlCreateBuilder(Node node, ISignaler signaler)
            : base(node, "\"")
        { }

        /// <summary>
        /// Makes sure we can select the scope identity for inserted record.
        /// </summary>
        /// <param name="builder">Where to put our SQL.</param>
        protected override void GetTail(StringBuilder builder)
        {
            builder.Append("; select scope_identity();");
        }
    }
}
