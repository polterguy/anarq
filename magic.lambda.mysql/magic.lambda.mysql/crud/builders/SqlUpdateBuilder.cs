/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using magic.node;
using com = magic.data.common;
using magic.signals.contracts;

namespace magic.lambda.mysql.crud.builders
{
    /// <summary>
    /// Specialised update SQL builder, to create a select MySQL SQL statement
    /// by semantically traversing an input node.
    /// </summary>
    public class SqlUpdateBuilder : com.SqlUpdateBuilder
    {
        /// <summary>
        /// Creates an update SQL statement
        /// </summary>
        /// <param name="node">Root node to generate your SQL from.</param>
        /// <param name="signaler">Signaler to invoke slots.</param>
        public SqlUpdateBuilder(Node node, ISignaler signaler)
            : base(node, "`")
        { }
    }
}
