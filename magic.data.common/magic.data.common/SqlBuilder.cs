/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.Linq;
using System.Text;
using System.Globalization;
using magic.node;
using magic.node.extensions;
using magic.signals.contracts;

namespace magic.data.common
{
    /// <summary>
    /// Common base class for SQL generators, allowing you to generate SQL from a lambda object.
    /// </summary>
    public abstract class SqlBuilder
    {
        /// <summary>
        /// Creates a new SQL builder.
        /// </summary>
        /// <param name="node">Root node to generate your SQL from.</param>
        /// <param name="escapeChar">Escape character to use for escaping table names etc.</param>
        public SqlBuilder(Node node, string escapeChar)
        {
            Root = node ?? throw new ArgumentNullException(nameof(node));
            EscapeChar = escapeChar ?? throw new ArgumentNullException(nameof(escapeChar));
        }

        /// <summary>
        /// Builds your SQL statement, and returns a structured SQL statement, plus any parameters.
        /// </summary>
        /// <returns>Node containing SQL as root node, and parameters as children.</returns>
        public abstract Node Build();

        /// <summary>
        /// Signals to inherited class if this is a pure generate job, or if it should also evaluate the SQL command.
        /// </summary>
        public bool IsGenerateOnly => Root.Children.FirstOrDefault(x => x.Name == "generate")?.Get<bool>() ?? false;

        /// <summary>
        /// Returns the escape character, which is normally for instance " or `
        /// </summary>
        protected string EscapeChar { get; private set; }

        /// <summary>
        /// Generic helper method to create an SqlBuilder of type T, and use it to semantically
        /// traverse a node hierarchy, to create the relevant SQL and its parameter collection.
        /// </summary>
        /// <typeparam name="T">Type of SQL builder to create.</typeparam>
        /// <param name="signaler">Signaler for instance.</param>
        /// <param name="input">Node to parser.</param>
        /// <returns>If execution of node should be done, the method will return the node to execute.</returns>
        public static Node Parse<T>(ISignaler signaler, Node input) where T : SqlBuilder
        {
            /*
             * Unfortunately this is our only means to create an instance of type,
             * since it requires arguments in its CTOR, and we can't create constraints
             * for constructor arguments using generic constraints.
             */
            var builder = Activator.CreateInstance(typeof(T), new object[] { input, signaler }) as T;
            var sqlNode = builder.Build();

            // Checking if this is a "build only" invocation.
            if (builder.IsGenerateOnly)
            {
                input.Value = sqlNode.Value;
                input.Clear();
                input.AddRange(sqlNode.Children.ToList());
                return null ;
            }
            return sqlNode;
        }

        #region [ -- Protected helper methods and properties -- ]

        /// <summary>
        /// Root node from which the SQL generator is being evaluated towards.
        /// </summary>
        protected Node Root { get; private set; }

        /// <summary>
        /// Securely adds the table name into the specified builder.
        /// </summary>
        /// <param name="builder">StringBuilder to append the table name into.</param>
        protected virtual void GetTableName(StringBuilder builder)
        {
            // Retrieving actual table name from [table] node.
            var tableName = Root.Children.FirstOrDefault(x => x.Name == "table")?.GetEx<string>();
            if (tableName == null)
                throw new ApplicationException($"No table name supplied to '{GetType().FullName}'");

            /*
             * Notice, if table name contains ".", we assume these are namespace qualifiers
             * (MS SQL server type of namespaces).
             */
            var first = true;
            foreach (var idx in tableName.Split('.'))
            {
                if (first)
                    first = false;
                else
                    builder.Append(".");
                builder.Append(EscapeChar);
                builder.Append(idx.Replace(EscapeChar, EscapeChar + EscapeChar));
                builder.Append(EscapeChar);
            }
        }

        /// <summary>
        /// Builds the 'where' parts of the SQL statement.
        /// </summary>
        /// <param name="whereNode">Current input node from where to start looking for semantic where parts.</param>
        /// <param name="builder">String builder to put the results into.</param>
        protected virtual void BuildWhere(Node whereNode, StringBuilder builder)
        {
            // finding where node, if any, and doing some basic sanity checking.
            var where = Root.Children.Where(x => x.Name == "where");
            if (where.Count() > 1)
                throw new ApplicationException($"Syntax error in '{GetType().FullName}', too many [where] nodes");

            // Checking we actuall have a [where] declaration at all.
            if (!where.Any() || !where.First().Children.Any())
                return;

            // Appending actual "where" parts into SQL.
            builder.Append(" where ");

            /*
             * Recursively looping through each level, and appending its parts
             * as a "name/value" collection, making sure we add each value as an
             * SQL parameter.
             */
            int levelNo = 0;
            foreach (var idx in where.First().Children)
            {
                switch (idx.Name)
                {
                    case "and":
                        if (levelNo != 0)
                            builder.Append(" and ");
                        BuildWhereLevel(whereNode, builder, idx, "and", ref levelNo);
                        break;

                    case "or":
                        if (levelNo != 0)
                            builder.Append(" or ");
                        BuildWhereLevel(whereNode, builder, idx, "or", ref levelNo);
                        break;

                    default:
                        throw new ArgumentException($"I don't understand '{idx.Name}' as a where clause while trying to build SQL");
                }
            }
        }

        #endregion

        #region [ -- Private helper methods -- ]

        /*
         * Building one "where level" (within one set of paranthesis),
         * and recursivelu adding a new level for each "and" and "or"
         * parts we can find in our level.
         */
        void BuildWhereLevel(
            Node result,
            StringBuilder builder,
            Node level,
            string logicalOperator,
            ref int levelNo,
            string comparisonOperator = "=",
            bool paranthesis = true)
        {
            if (paranthesis)
                builder.Append("(");

            bool first = true;
            foreach (var idxCol in level.Children)
            {
                if (first)
                    first = false;
                else
                    builder.Append(" " + logicalOperator + " ");

                switch (idxCol.Name)
                {
                    case "and":
                        BuildWhereLevel(result, builder, idxCol, "and", ref levelNo);
                        break;

                    case "or":
                        BuildWhereLevel(result, builder, idxCol, "or", ref levelNo);
                        break;

                    case "in":

                        // TODO: Refactor and create one implementation, shared with the piece of code below.
                        levelNo = CreateInCriteria(
                            result, 
                            builder, 
                            levelNo, 
                            idxCol.Children.First().Name, 
                            idxCol.Children.First().Children.Select(x => x.GetEx<long>()).ToArray());
                        break;

                    // TODO: Implement "in".
                    case ">":
                    case "<":
                    case ">=":
                    case "<=":
                    case "!=":
                    case "=":
                    case "like":

                        /*
                         * Notice, calling self with comparison operator explicitly set this time,
                         * and no needs to add paranthesis.
                         */
                        BuildWhereLevel(result, builder, idxCol, logicalOperator, ref levelNo, idxCol.Name, false);
                        break;

                    default:

                        var comparisonValue = idxCol.GetEx<object>();
                        var currentOperator = comparisonOperator;
                        var sqlArgumentName = "@" + levelNo;
                        var columnName = idxCol.Name;
                        if (columnName.StartsWith("."))
                        {
                            // Allowing for escaped column names, to suppor columns containing "." as a part of their names.
                            columnName = columnName.Substring(1);
                        }
                        else if (columnName.Contains("."))
                        {
                            /*
                             * Notice, for simplicity reasons, and to allow passing in operators
                             * as a single level hierarchy, we allow for an additional method to supply the comparison
                             * operator, which is having the operator to the right of a ".", where the column name is
                             * the first parts.
                             * 
                             * Assuming first part is our operator.
                             */
                            var entities = columnName.Split('.').Reverse();
                            switch (entities.First())
                            {
                                case "like":
                                    currentOperator = "like";
                                    break;
                                case "mt":
                                    currentOperator = ">";
                                    break;
                                case "lt":
                                    currentOperator = "<";
                                    break;
                                case "mteq":
                                    currentOperator = ">=";
                                    break;
                                case "lteq":
                                    currentOperator = "<=";
                                    break;
                                case "neq":
                                    currentOperator = "!=";
                                    break;
                                case "eq":
                                    currentOperator = "=";
                                    break;
                                case "in":
                                    currentOperator = "in";
                                    break;
                                default:
                                    throw new ArgumentException($"'{columnName}' is not understood by the SQL generator, did you intend to supply '.{columnName}'?");
                            }
                            columnName = string.Join(".", entities.Skip(1).Reverse());
                        }
                        if (currentOperator == "in")
                        {
                            levelNo = CreateInCriteria(
                                result, 
                                builder, 
                                levelNo, 
                                columnName,
                                comparisonValue.ToString()
                                    .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                    .Select(x => Convert.ToInt64(x, CultureInfo.InvariantCulture)).ToArray());
                        }
                        else
                        {
                            var criteria = EscapeChar +
                                columnName.Replace(EscapeChar, EscapeChar + EscapeChar) +
                                EscapeChar + " " + currentOperator + " " +
                                sqlArgumentName;
                            builder.Append(criteria);
                            result.Add(new Node(sqlArgumentName, comparisonValue));
                            ++levelNo;
                        }
                        break;
                }
            }

            if (paranthesis)
                builder.Append(")");
        }

        /*
         * Creates an "in" SQL criteria.
         */
        int CreateInCriteria(
            Node result, 
            StringBuilder builder, 
            int levelNo, 
            string columnName, 
            params long[] values)
        {
            builder.Append(
                EscapeChar +
                columnName.Replace(EscapeChar, EscapeChar + EscapeChar) +
                EscapeChar + " in ");
            builder.Append("(");
            var firstInValue = true;
            foreach (var idx in values)
            {
                if (firstInValue)
                    firstInValue = false;
                else
                    builder.Append(",");
                builder.Append("@" + levelNo);
                result.Add(new Node("@" + levelNo, idx));
                ++levelNo;
            }
            builder.Append(")");
            return levelNo;
        }

        #endregion
    }
}
