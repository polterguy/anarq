/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System.Linq;
using Newtonsoft.Json.Linq;
using magic.node;
using magic.node.extensions;
using magic.signals.contracts;
using magic.lambda.json.utilities;

namespace magic.lambda.json
{
    /// <summary>
    /// [lambda2json] slot for transforming a lambda hierarchy to a JSON string.
    /// </summary>
    [Slot(Name = "lambda2json")]
    public class Lambda2Json : ISlot
    {
        /// <summary>
        /// Slot implementation.
        /// </summary>
        /// <param name="signaler">Signaler that raised signal.</param>
        /// <param name="input">Arguments to slot.</param>
        public void Signal(ISignaler signaler, Node input)
        {
            var tmp = new Node();
            var format = false;
            if (input.Value != null)
            {
                format = input.Children.FirstOrDefault(x => x.Name == "format")?.GetEx<bool>() ?? false;
                tmp.AddRange(input.Evaluate().Select(x => x.Clone()));
            }
            else
            {
                tmp.AddRange(input.Children.Select(x => x.Clone()));
            }

            var token = Transformer.TransformToJSON(tmp);
            input.Clear();
            input.Value = token.ToString(format ? Newtonsoft.Json.Formatting.Indented : Newtonsoft.Json.Formatting.None);
        }
    }
}
