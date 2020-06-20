/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.Linq;
using magic.node;
using magic.node.extensions;
using magic.signals.contracts;

namespace magic.lambda.source
{
    // TODO: Consider renaming.
    /// <summary>
    /// [get-count] slot that will return the count of nodes found for an expression.
    /// </summary>
    [Slot(Name = "get-count")]
    public class GetCount : ISlot
    {
        /// <summary>
        /// Implementation of signal
        /// </summary>
        /// <param name="signaler">Signaler used to signal</param>
        /// <param name="input">Parameters passed from signaler</param>
        public void Signal(ISignaler signaler, Node input)
        {
            if (input.Value == null)
                throw new ApplicationException("No expression source provided for [count]");

            var src = input.Evaluate();
            input.Value = src.Count();
        }
    }
}
