﻿/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using magic.node;
using magic.node.extensions;
using magic.signals.contracts;

namespace magic.lambda.source
{
    // TODO: Consider renaming.
    /// <summary>
    /// [get-nodes] slot that will return all nodes from evaluating an expression.
    /// </summary>
    [Slot(Name = "get-nodes")]
    public class GetNodes : ISlot
    {
        /// <summary>
        /// Implementation of signal
        /// </summary>
        /// <param name="signaler">Signaler used to signal</param>
        /// <param name="input">Parameters passed from signaler</param>
        public void Signal(ISignaler signaler, Node input)
        {
            if (input.Value == null)
                return;

            var src = input.Evaluate();
            foreach (var idx in src)
            {
                input.Add(idx.Clone());
            }
        }
    }
}
