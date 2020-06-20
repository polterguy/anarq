/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using magic.node;
using magic.node.extensions;
using magic.signals.contracts;

namespace magic.lambda.slots
{
    /// <summary>
    /// [slots.exists] slot that will check if a dynamic slot exists or not.
    /// </summary>
    [Slot(Name = "slots.exists")]
    public class Exists : ISlot
    {
        /// <summary>
        /// Slot implementation.
        /// </summary>
        /// <param name="signaler">Signaler that raised signal.</param>
        /// <param name="input">Arguments to slot.</param>
        public void Signal(ISignaler signaler, Node input)
        {
            // Retrieving slot's lambda, no reasons to clone, GetSlot will clone.
            input.Value = Create.SlotExists(input.GetEx<string>());
        }
    }
}
