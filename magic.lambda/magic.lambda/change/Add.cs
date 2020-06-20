/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System.Linq;
using System.Threading.Tasks;
using magic.node;
using magic.node.extensions;
using magic.signals.contracts;

namespace magic.lambda.change
{
    /// <summary>
    /// [add] slot allowing you to append nodes into some destination node.
    /// </summary>
    [Slot(Name = "add")]
    [Slot(Name = "wait.add")]
    public class Add : ISlot, ISlotAsync
    {
        /// <summary>
        /// Implementation of signal
        /// </summary>
        /// <param name="signaler">Signaler used to signal</param>
        /// <param name="input">Parameters passed from signaler</param>
        public void Signal(ISignaler signaler, Node input)
        {
            signaler.Signal("eval", input);
            AddResult(input);
        }

        /// <summary>
        /// Implementation of signal
        /// </summary>
        /// <param name="signaler">Signaler used to signal</param>
        /// <param name="input">Parameters passed from signaler</param>
        /// <returns>An awaitable task.</returns>
        public async Task SignalAsync(ISignaler signaler, Node input)
        {
            await signaler.SignalAsync("wait.eval", input);
            AddResult(input);
        }

        #region [ -- Private helper methods -- ]

        /*
         * Adds result from source into destination nodes.
         */
        void AddResult(Node input)
        {
            // Iterating through each destination.
            foreach (var idxDest in input.Evaluate())
            {
                idxDest.AddRange(input.Children.SelectMany(x => x.Children).Select(x2 => x2.Clone()));
            }
        }

        #endregion
    }
}
