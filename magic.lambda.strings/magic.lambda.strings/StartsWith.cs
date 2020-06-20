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

namespace magic.lambda.strings
{
    /// <summary>
    /// [strings.starts-with] slot that returns true if the specified string starts with its value
    /// from its first argument.
    /// </summary>
    [Slot(Name = "strings.starts-with")]
    [Slot(Name = "wait.strings.starts-with")]
    public class StartsWith : ISlot, ISlotAsync
    {
        /// <summary>
        /// Implementation of slot.
        /// </summary>
        /// <param name="signaler">Signaler used to raise signal.</param>
        /// <param name="input">Arguments to your slot.</param>
        public void Signal(ISignaler signaler, Node input)
        {
            // Sanity checking.
            if (input.Children.Count() != 1)
                throw new ApplicationException("[strings.starts-with] must be given exactly one argument that contains value to look for");

            signaler.Signal("eval", input);

            input.Value = input.GetEx<string>()
                .StartsWith(input.Children.First().GetEx<string>(), StringComparison.InvariantCulture);
        }

        /// <summary>
        /// Implementation of slot.
        /// </summary>
        /// <param name="signaler">Signaler used to raise the signal.</param>
        /// <param name="input">Arguments to slot.</param>
        /// <returns>An awaitable task.</returns>
        public async Task SignalAsync(ISignaler signaler, Node input)
        {
            // Sanity checking.
            if (input.Children.Count() != 1)
                throw new ApplicationException("[strings.starts-with] must be given exactly one argument that contains value to look for");

            await signaler.SignalAsync("wait.eval", input);

            input.Value = input.GetEx<string>()
                .StartsWith(input.Children.First().GetEx<string>(), StringComparison.InvariantCulture);
        }
    }
}
