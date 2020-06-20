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
    /// [strings.concat] slot for concatenating two or more strings together to become one.
    /// </summary>
    [Slot(Name = "strings.join")]
    [Slot(Name = "wait.strings.join")]
    public class Join : ISlot, ISlotAsync
    {
        /// <summary>
        /// Implementation of slot.
        /// </summary>
        /// <param name="signaler">Signaler used to raise signal.</param>
        /// <param name="input">Arguments to your slot.</param>
        public void Signal(ISignaler signaler, Node input)
        {
            if (!input.Children.Any())
                throw new ApplicationException("No arguments provided to [strings.concat]");

            signaler.Signal("eval", input);

            input.Value = string.Join(input.Children.First(x => x.Name != "").GetEx<string>(), input.Evaluate().Select(x => x.GetEx<string>()).ToArray());
        }

        /// <summary>
        /// Implementation of slot.
        /// </summary>
        /// <param name="signaler">Signaler used to raise the signal.</param>
        /// <param name="input">Arguments to slot.</param>
        /// <returns>An awaitable task.</returns>
        public async Task SignalAsync(ISignaler signaler, Node input)
        {
            if (!input.Children.Any())
                throw new ApplicationException("No arguments provided to [strings.concat]");

            await signaler.SignalAsync("wait.eval", input);

            input.Value = string.Join(input.Children.First(x => x.Name != "").GetEx<string>(), input.Evaluate().Select(x => x.GetEx<string>()).ToArray());
        }
    }
}
