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
    /// [insert-after] slot allowing you to insert a range of nodes after some other node
    /// in your lambda graph object.
    /// </summary>
    [Slot(Name = "insert-after")]
    [Slot(Name = "wait.insert-after")]
    public class InsertAfter : ISlot, ISlotAsync
    {
        /// <summary>
        /// Implementation of signal
        /// </summary>
        /// <param name="signaler">Signaler used to signal</param>
        /// <param name="input">Parameters passed from signaler</param>
        public void Signal(ISignaler signaler, Node input)
        {
            signaler.Signal("eval", input);
            Insert(input);
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
            Insert(input);
        }

        #region [ -- Private helper methods -- ]

        void Insert(Node input)
        {
            // Looping through each destination.
            foreach (var idxDest in input.Evaluate().ToList()) // To avoid changing collection during enumeration
            {
                /*
                 * Looping through each source node and adding its children to currently iterated destination.
                 * 
                 * Notice, Reverse() to make sure ordering becomes what caller expects.
                 */
                foreach (var idxSource in input.Children.SelectMany(x => x.Children).Reverse())
                {
                    idxDest.InsertAfter(idxSource.Clone()); // Cloning in case of multiple destinations.
                }
            }
        }

        #endregion
    }
}
