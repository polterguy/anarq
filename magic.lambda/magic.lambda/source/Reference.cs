/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using magic.node;
using magic.node.extensions;
using magic.signals.contracts;
using System;
using System.Linq;

namespace magic.lambda.source
{
    /// <summary>
    /// [reference] slot that will set its own value to the specified expression's evaluated node, by reference.
    /// </summary>
    [Slot(Name = "reference")]
    public class Reference : ISlot
    {
        /// <summary>
        /// Implementation of signal
        /// </summary>
        /// <param name="signaler">Signaler used to signal</param>
        /// <param name="input">Parameters passed from signaler</param>
        public void Signal(ISignaler signaler, Node input)
        {
            if (input.Value == null)
                throw new ArgumentException("No value provided to [reference]");

            var src = input.Evaluate();
            if (src.Count() > 1)
                throw new ArgumentException("Expressions provided to [reference] returned more than one node");
            input.Value = src.FirstOrDefault();
        }
    }
}
