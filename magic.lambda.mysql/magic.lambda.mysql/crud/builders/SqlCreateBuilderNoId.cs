/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System.Text;
using magic.node;
using com = magic.data.common;
using magic.signals.contracts;

namespace magic.lambda.mysql.crud.builders
{
    /// <summary>
    /// Specialised insert SQL builder, to create an insert MySQL SQL statement
    /// by semantically traversing an input node, that does not return the ID
    /// of the newly created record.
    /// </summary>
    public class SqlCreateBuilderNoId : com.SqlCreateBuilder
    {
        /// <summary>
        /// Creates an insert SQL statement
        /// </summary>
        /// <param name="node">Root node to generate your SQL from.</param>
        /// <param name="signaler">Signaler to invoke slots.</param>
        public SqlCreateBuilderNoId(Node node, ISignaler signaler)
            : base(node, "`")
        { }
    }
}
