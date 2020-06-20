/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.Linq;
using System.Globalization;
using magic.node;
using magic.node.extensions;
using magic.node.expressions;
using magic.signals.contracts;
using magic.node.extensions.hyperlambda;

namespace magic.lambda.change
{
    /// <summary>
    /// [convert] slot allowing you to convert values of nodes from one type to some other type.
    /// </summary>
    [Slot(Name = "convert")]
    public class Convert : ISlot
    {
        /// <summary>
        /// Implementation of signal
        /// </summary>
        /// <param name="signaler">Signaler used to signal</param>
        /// <param name="input">Parameters passed from signaler</param>
        public void Signal(ISignaler signaler, Node input)
        {
            if (input.Children.Count() != 1 || !input.Children.Any(x => x.Name == "type"))
                throw new ApplicationException("[convert] can only handle one argument, which is [type]");

            var value = input.GetEx<object>();
            var type = input.Children.First().GetEx<string>();
            switch (type)
            {
                case "int":
                    input.Value = System.Convert.ToInt32(value ?? 0, CultureInfo.InvariantCulture);
                    break;

                case "uint":
                    input.Value = System.Convert.ToUInt32(value ?? 0, CultureInfo.InvariantCulture);
                    break;

                case "long":
                    input.Value = System.Convert.ToInt64(value ?? 0, CultureInfo.InvariantCulture);
                    break;

                case "ulong":
                    input.Value = System.Convert.ToUInt64(value ?? 0, CultureInfo.InvariantCulture);
                    break;

                case "decimal":
                    input.Value = System.Convert.ToDecimal(value ?? 0, CultureInfo.InvariantCulture);
                    break;

                case "double":
                    input.Value = System.Convert.ToDouble(value ?? 0, CultureInfo.InvariantCulture);
                    break;

                case "single":
                    input.Value = System.Convert.ToSingle(value ?? 0, CultureInfo.InvariantCulture);
                    break;

                case "bool":
                    input.Value = value?.Equals("true") ?? false;
                    break;

                case "date":
                    input.Value = DateTime.ParseExact(value?.ToString() ?? DateTime.MinValue.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture), "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);
                    break;

                case "guid":
                    input.Value = new Guid(value?.ToString() ?? Guid.NewGuid().ToString());
                    break;

                case "char":
                    input.Value = System.Convert.ToChar(value ?? 0, CultureInfo.InvariantCulture);
                    break;

                case "byte":
                    input.Value = System.Convert.ToByte(value ?? 0, CultureInfo.InvariantCulture);
                    break;

                case "x":
                    input.Value = new Expression(value?.ToString() ?? "");
                    break;

                case "string":
                    input.Value = value?.ToString() ?? "";
                    break;

                case "node":
                    input.Value = new Parser(value?.ToString() ?? "").Lambda();
                    break;

                default:
                    throw new ApplicationException($"Unknown type '{type}' when invoking [convert]");
            }
        }
    }
}
