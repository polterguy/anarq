/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.Linq;
using System.Text;
using magic.node;
using magic.node.extensions;

namespace magic.data.common
{
    /// <summary>
    /// Specialised insert SQL builder, to create an insert SQL statement by semantically traversing an input node.
    /// </summary>
    public abstract class SqlCreateBuilder : SqlBuilder
    {
        /// <summary>
        /// Creates an insert SQL statement
        /// </summary>
        /// <param name="node">Root node to generate your SQL from.</param>
        /// <param name="escapeChar">Escape character to use for escaping table names etc.</param>
        public SqlCreateBuilder(Node node, string escapeChar)
            : base(node, escapeChar)
        { }

        /// <summary>
        /// Builds your insert SQL statement, and returns a structured SQL statement, plus any parameters.
        /// </summary>
        /// <returns>Node containing insert SQL as root node, and parameters as children.</returns>
        public override Node Build()
        {
            // Return value.
            var result = new Node("sql");
            var builder = new StringBuilder();

            // Starting build process.
            builder.Append("insert into ");

            // Getting table name from base class.
            GetTableName(builder);

            // Building insertion [values].
            BuildValues(result, builder);

            // In case derived class wants to inject something here ...
            GetTail(builder);

            // Returning result to caller.
            result.Value = builder.ToString();
            return result;
        }

        #region [ -- Protected helper methods -- ]

        /// <summary>
        /// Adds the 'values' parts of your SQL to the specified string builder.
        /// </summary>
        /// <param name="valuesNode">Current input node from where to start looking for semantic values parts.</param>
        /// <param name="builder">String builder to put the results into.</param>
        protected virtual void BuildValues(Node valuesNode, StringBuilder builder)
        {
            // Appending actual insertion values.
            var values = Root.Children.Where(x => x.Name == "values");

            // Sanity checking, making sure there's exactly one [values] node.
            if (values.Count() != 1)
                throw new ApplicationException($"Exactly one [values] needs to be provided to '{GetType().FullName}'");

            // Appending column names.
            builder.Append(" (");
            var first = true;
            foreach (var idx in values.First().Children)
            {
                if (first)
                    first = false;
                else
                    builder.Append(", ");

                builder.Append(EscapeChar + idx.Name.Replace(EscapeChar, EscapeChar + EscapeChar) + EscapeChar);
            }

            // Appending actual values, as parameters.
            builder.Append(")");

            // In case derived class wants to inject something here ...
            GetInBetween(builder);

            builder.Append(" values (");
            var idxNo = 0;
            foreach (var idx in values.First().Children)
            {
                if (idxNo > 0)
                    builder.Append(", ");

                if (idx.Value == null)
                {
                    builder.Append("null");
                }
                else
                {
                    builder.Append("@" + idxNo);
                    valuesNode.Add(new Node("@" + idxNo, idx.GetEx<object>()));
                    ++idxNo;
                }
            }
            builder.Append(")");
        }

        /// <summary>
        /// Returns the tail for your SQL statement, which by default is none.
        /// </summary>
        /// <param name="builder">Where to put your tail.</param>
        protected virtual void GetTail(StringBuilder builder)
        { }

        /// <summary>
        /// Adds "in between" parts to your SQL, which might include specialized SQL text, depending upon your adapter.
        /// Default implementation adds nothing.
        /// </summary>
        /// <param name="builder">Where to put the resulting in between parts.</param>
        protected virtual void GetInBetween(StringBuilder builder)
        { }

        #endregion
    }
}
