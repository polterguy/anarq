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
    /// [or] slot allowing you to group multiple comparisons (for instance), where at least one of these must evaluate
    /// to true, for the [or] slot as a whole to evaluate to true.
    /// </summary>
    [Slot(Name = "or")]
    [Slot(Name = "wait.or")]
    public class Or : ISlot, ISlotAsync
    {
        /// <summary>
        /// Implementation of signal
        /// </summary>
        /// <param name="signaler">Signaler used to signal</param>
        /// <param name="input">Parameters passed from signaler</param>
        public void Signal(ISignaler signaler, Node input)
        {
            if (input.Children.Count() < 2)
                throw new ApplicationException("Operator [or] requires at least two children nodes");

            // Notice, to support short circuit evaluation, we cannot use same logic as we're using in [and].
            foreach (var idx in input.Children)
            {
                if (idx.Name.FirstOrDefault() != '.')
                    signaler.Signal(idx.Name, idx);

                if (idx.GetEx<bool>())
                {
                    input.Value = true;
                    return;
                }
            }
            input.Value = false;
        }

        /// <summary>
        /// Implementation of signal
        /// </summary>
        /// <param name="signaler">Signaler used to signal</param>
        /// <param name="input">Parameters passed from signaler</param>
        /// <returns>An awaitable task.</returns>
        public async Task SignalAsync(ISignaler signaler, Node input)
        {
            if (input.Children.Count() < 2)
                throw new ApplicationException("Operator [or] requires at least two children nodes");

            // Notice, to support short circuit evaluation, we cannot use same logic as we're using in [and].
            foreach (var idx in input.Children)
            {
                if (idx.Name.FirstOrDefault() != '.')
                    await signaler.SignalAsync(idx.Name, idx);

                if (idx.GetEx<bool>())
                {
                    input.Value = true;
                    return;
                }
            }
            input.Value = false;
        }
    }
}
