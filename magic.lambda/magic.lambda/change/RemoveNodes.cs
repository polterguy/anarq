/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System.Linq;
using magic.node;
using magic.node.extensions;
using magic.signals.contracts;

namespace magic.lambda.change
{
    /// <summary>
    /// [remove-nodes] slot allowing you to remove nodes from your lambda graph object.
    /// </summary>
    [Slot(Name = "remove-nodes")]
    public class RemoveNodes : ISlot
    {
        /// <summary>
        /// Implementation of signal
        /// </summary>
        /// <param name="signaler">Signaler used to signal</param>
        /// <param name="input">Parameters passed from signaler</param>
        public void Signal(ISignaler signaler, Node input)
        {
            foreach (var idx in input.Evaluate().ToList())
            {
                idx.UnTie();
            }
        }
    }
}
