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
    /// Specialised delete SQL builder, to create a delete MySQL SQL statement
    /// by semantically traversing an input node.
    /// </summary>
    public class SqlDeleteBuilder : com.SqlDeleteBuilder
    {
        /// <summary>
        /// Creates a delete SQL statement
        /// </summary>
        /// <param name="node">Root node to generate your SQL from.</param>
        /// <param name="signaler">Signaler to invoke slots.</param>
        public SqlDeleteBuilder(Node node, ISignaler signaler)
            : base(node, "`")
        { }
    }
}
