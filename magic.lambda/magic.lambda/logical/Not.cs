/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using magic.node;
using magic.node.extensions;
using magic.signals.contracts;

namespace magic.lambda.logical
{
    /// <summary>
    /// [not] slot, negating the value of its first children's value.
    /// </summary>
    [Slot(Name = "not")]
    [Slot(Name = "wait.not")]
    public class Not : ISlot, ISlotAsync
    {
        /// <summary>
        /// Implementation of signal
        /// </summary>
        /// <param name="signaler">Signaler used to signal</param>
        /// <param name="input">Parameters passed from signaler</param>
        public void Signal(ISignaler signaler, Node input)
        {
            if (input.Children.Count() != 1)
                throw new ApplicationException("Operator [not] requires exactly one child");

            signaler.Signal("eval", input);

            input.Value = !input.Children.First().GetEx<bool>();
        }

        /// <summary>
        /// Implementation of signal
        /// </summary>
        /// <param name="signaler">Signaler used to signal</param>
        /// <param name="input">Parameters passed from signaler</param>
        /// <returns>An awaitable task.</returns>
        public async Task SignalAsync(ISignaler signaler, Node input)
        {
            if (input.Children.Count() != 1)
                throw new ApplicationException("Operator [not] requires exactly one child");

            await signaler.SignalAsync("wait.eval", input);

            input.Value = !input.Children.First().GetEx<bool>();
        }
    }
}
