/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.Threading.Tasks;
using magic.node;
using magic.signals.contracts;

namespace magic.lambda.branching
{
    /// <summary>
    /// [else] slot for matching with an [if] and/or [else-if] slot. Must come after either or the previously mentioned slots.
    /// </summary>
    [Slot(Name = "else")]
    [Slot(Name = "wait.else")]
    public class Else : ISlot, ISlotAsync
    {
        /// <summary>
        /// Implementation of signal
        /// </summary>
        /// <param name="signaler">Signaler used to signal</param>
        /// <param name="input">Parameters passed from signaler</param>
        public void Signal(ISignaler signaler, Node input)
        {
            if (ShouldEvaluate(input))
                signaler.Signal("eval", input);
        }

        /// <summary>
        /// Implementation of signal
        /// </summary>
        /// <param name="signaler">Signaler used to signal</param>
        /// <param name="input">Parameters passed from signaler</param>
        /// <returns>An awaitable task.</returns>
        public async Task SignalAsync(ISignaler signaler, Node input)
        {
            if (ShouldEvaluate(input))
                await signaler.SignalAsync("wait.eval", input);
        }

        #region [ -- Private helper methods -- ]

        /*
         * Helper method for the above, to check if we should evaluate [else] at all.
         */
        bool ShouldEvaluate(Node input)
        {
            /*
             * Traversing backwards in graph, finding our if we should evaluate
             * or not, and sanity checking invocation at the same time.
             */
            var previous = input.Previous;
            if (previous == null ||
                (previous.Name != "if" && previous.Name != "else-if" &&
                previous.Name != "wait.if" && previous.Name != "wait.else-if"))
                throw new ApplicationException("[else] must have an [if] or [else-if] before it");

            return ElseIf.PreviousIsFalse(previous);
        }

        #endregion
    }
}
