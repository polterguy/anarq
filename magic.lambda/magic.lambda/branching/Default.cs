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
    /// [default] slot for [switch] slots.
    /// </summary>
    [Slot(Name = "default")]
    [Slot(Name = "wait.default")]
    public class Default : ISlot, ISlotAsync
    {
        /// <summary>
        /// Implementation of signal
        /// </summary>
        /// <param name="signaler">Signaler used to signal</param>
        /// <param name="input">Parameters passed from signaler</param>
        public void Signal(ISignaler signaler, Node input)
        {
            SanityCheckInvocation(input);
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
            SanityCheckInvocation(input);
            await signaler.SignalAsync("wait.eval", input);
        }

        #region [ -- Private helper methods -- ]

        void SanityCheckInvocation(Node input)
        {
            if (input.Parent?.Name != "switch")
                throw new ApplicationException("[defalut] must be a child of [switch]");
        }

        #endregion
    }
}
