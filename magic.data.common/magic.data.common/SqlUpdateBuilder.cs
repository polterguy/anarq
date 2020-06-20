﻿/*
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
    /// Specialised update SQL builder, to create a select SQL statement by semantically traversing an input node.
    /// </summary>
    public abstract class SqlUpdateBuilder : SqlBuilder
    {
        /// <summary>
        /// Creates an update SQL statement
        /// </summary>
        /// <param name="node">Root node to generate your SQL from.</param>
        /// <param name="escapeChar">Escape character to use for escaping table names etc.</param>
        public SqlUpdateBuilder(Node node, string escapeChar)
            : base(node, escapeChar)
        { }

        /// <summary>
        /// Builds your update SQL statement, and returns a structured SQL statement, plus any parameters.
        /// </summary>
        /// <returns>Node containing update SQL as root node, and parameters as children.</returns>
        public override Node Build()
        {
            // Return value.
            var result = new Node("sql");
            var builder = new StringBuilder();

            // Building SQL text and parameter collection.
            builder.Append("update ");

            // Getting table name from base class.
            GetTableName(builder);

            // Adding set
            builder.Append(" set ");

            // Adding [values].
            GetValues(builder, result);

            // Getting [where] clause.
            BuildWhere(result, builder);

            // Returning result to caller.
            result.Value = builder.ToString();
            return result;
        }

        #region [ -- Private helper methods -- ]

        void GetValues(StringBuilder builder, Node result)
        {
            var valuesNodes = Root.Children.Where(x => x.Name == "values");
            if (!valuesNodes.Any() || !valuesNodes.First().Children.Any())
                throw new ApplicationException($"Missing [values] node in '{GetType().FullName}'");

            var idxNo = 0;
            foreach (var idxCol in valuesNodes.First().Children)
            {
                if (idxNo > 0)
                    builder.Append(", ");
                builder.Append(EscapeChar + idxCol.Name.Replace(EscapeChar, EscapeChar + EscapeChar) + EscapeChar);
                if (idxCol.Value == null)
                {
                    builder.Append(" = null");
                }
                else
                {
                    builder.Append(" = @v" + idxNo);
                    result.Add(new Node("@v" + idxNo, idxCol.GetEx<object>()));
                    ++idxNo;
                }
            }
        }

        #endregion
    }
}
