/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System.Linq;
using magic.node;
using magic.node.extensions;
using magic.signals.contracts;
using magic.node.extensions.hyperlambda;

namespace magic.lambda.hyperlambda
{
    /// <summary>
    /// [lambda] slot, allowing you to transform a piece of Hyperlambda to a lambda hierarchy.
    /// </summary>
    [Slot(Name = "hyper2lambda")]
    public class Hyper2Lambda : ISlot
    {
        /// <summary>
        /// Implementation of your slot.
        /// </summary>
        /// <param name="signaler">Signaler that raised the signal.</param>
        /// <param name="input">Arguments to your slot.</param>
        public void Signal(ISignaler signaler, Node input)
        {
            var parser = new Parser(input.GetEx<string>());
            input.AddRange(parser.Lambda().Children.ToList());
            input.Value = null;
        }
    }
}
