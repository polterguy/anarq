/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System.Threading.Tasks;
using magic.node;
using magic.signals.contracts;
using magic.lambda.math.utilities;

namespace magic.lambda.math
{
    /// <summary>
    /// [-] slot for performing subtractions.
    /// </summary>
    [Slot(Name = "math.subtract")]
    [Slot(Name = "wait.math.subtract")]
    public class Subtraction : ISlot, ISlotAsync
    {
        /// <summary>
        /// Slot implementation.
        /// </summary>
        /// <param name="signaler">Signaler that raised the signal.</param>
        /// <param name="input">Arguments to slot.</param>
        public void Signal(ISignaler signaler, Node input)
        {
            signaler.Signal("eval", input);
            dynamic sum = Utilities.GetBase(input);
            foreach (var idx in Utilities.AllButBase(input))
            {
                sum -= idx;
            }
            input.Value = sum;
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
            dynamic sum = Utilities.GetBase(input);
            foreach (var idx in Utilities.AllButBase(input))
            {
                sum -= idx;
            }
            input.Value = sum;
        }
    }
}
