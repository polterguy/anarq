/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System.Linq;
using System.Threading.Tasks;
using magic.node;
using magic.node.extensions;
using magic.signals.contracts;

namespace magic.lambda.math
{
    /// <summary>
    /// [increment] slot for incrementing some value, optionally by a [step] argument.
    /// </summary>
    [Slot(Name = "math.increment")]
    [Slot(Name = "wait.math.increment")]
    public class Increment : ISlot, ISlotAsync
    {
        /// <summary>
        /// Slot implementation.
        /// </summary>
        /// <param name="signaler">Signaler that raised the signal.</param>
        /// <param name="input">Arguments to slot.</param>
        public void Signal(ISignaler signaler, Node input)
        {
            signaler.Signal("eval", input);
            var step = GetStep(input);
            foreach (var idx in input.Evaluate())
            {
                idx.Value = idx.Get<dynamic>() + step;
            }
        }

        /// <summary>
        /// Implementation of slot.
        /// </summary>
        /// <param name="signaler">Signaler used to raise the signal.</param>
        /// <param name="input">Arguments to slot.</param>
        /// <returns>An awaitable task.</returns>
        public async Task SignalAsync(ISignaler signaler, Node input)
        {
            await signaler.SignalAsync("wait.eval", input);
            var step = GetStep(input);
            foreach (var idx in input.Evaluate())
            {
                idx.Value = idx.Get<dynamic>() + step;
            }
        }

        #region [ -- Private helper methods -- ]

        dynamic GetStep(Node input)
        {
            return input.Children.FirstOrDefault()?.Value ?? 1;
        }

        #endregion
    }
}
