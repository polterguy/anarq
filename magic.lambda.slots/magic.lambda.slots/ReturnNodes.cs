/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System.Linq;
using magic.node;
using magic.node.extensions;
using magic.signals.contracts;

namespace magic.lambda.slots
{
    /// <summary>
    /// [return-nodes] slot for returning nodes from some evaluation object.
    /// </summary>
    [Slot(Name = "return-nodes")]
    public class ReturnNodes : ISlot
    {
        /// <summary>
        /// Slot implementation.
        /// </summary>
        /// <param name="signaler">Signaler that raised signal.</param>
        /// <param name="input">Arguments to slot.</param>
        public void Signal(ISignaler signaler, Node input)
        {
            signaler.Peek<Node>("slots.result").AddRange(input.Value == null ? input.Children.ToList() : input.Evaluate());
        }
    }
}
