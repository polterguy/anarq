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
    /// [and] slot allowing you to group multiple comparisons (for instance), where all of these must evaluate
    /// to true, for the [and] slot as a whole to evaluate to true.
    /// </summary>
    [Slot(Name = "and")]
    [Slot(Name = "wait.and")]
    public class And : ISlot, ISlotAsync
    {
        /// <summary>
        /// Implementation of signal
        /// </summary>
        /// <param name="signaler">Signaler used to signal</param>
        /// <param name="input">Parameters passed from signaler</param>
        public void Signal(ISignaler signaler, Node input)
        {
            if (input.Children.Count() < 2)
                throw new ApplicationException("Operator [and] requires at least two children nodes");
            signaler.Signal("eval", input);

            input.Value = IsTrue(input);
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
                throw new ApplicationException("Operator [and] requires at least two children nodes");

            await signaler.SignalAsync("wait.eval", input);

            input.Value = IsTrue(input);
        }

        #region [ -- Private helper methods -- ]

        bool IsTrue(Node input)
        {
            foreach (var idx in input.Children)
            {
                if (!idx.GetEx<bool>())
                    return false;
            }
            return true;
        }

        #endregion
    }
}
