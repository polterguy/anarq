/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using magic.node;
using magic.node.extensions;
using magic.signals.contracts;

namespace magic.lambda.strings
{
    /// <summary>
    /// [strings.replace-not-of] slot for replacing occurrencies of any single character not matching
    /// the specified character with some substitute character.
    /// </summary>
    [Slot(Name = "strings.replace-not-of")]
    [Slot(Name = "wait.strings.replace-not-of")]
    public class ReplaceNotOf : ISlot, ISlotAsync
    {
        /// <summary>
        /// Implementation of slot.
        /// </summary>
        /// <param name="signaler">Signaler used to raise signal.</param>
        /// <param name="input">Arguments to your slot.</param>
        public void Signal(ISignaler signaler, Node input)
        {
            // Sanity checking.
            if (input.Children.Count() != 2)
                throw new ArgumentException("[strings.replace-not-of] requires exactly two arguments, the first being a list of characters to not replace, the other beings its replacement character(s)");

            signaler.Signal("eval", input);

            var original = input.GetEx<string>();
            var what = input.Children.First().GetEx<string>();
            var with = input.Children.Skip(1).First().GetEx<string>();

            // Substituting.
            var result = new StringBuilder();
            foreach (var idx in original)
            {
                if (what.IndexOf(idx) != -1)
                    result.Append(idx);
                else
                    result.Append(with);
            }
            input.Value = result.ToString();
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
            if (input.Children.Count() != 2)
                throw new ArgumentException("[strings.replace-not-of] requires exactly two arguments, the first being a list of characters to not replace, the other beings its replacement character(s)");

            await signaler.SignalAsync("wait.eval", input);

            var original = input.GetEx<string>();
            var what = input.Children.First().GetEx<string>();
            var with = input.Children.Skip(1).First().GetEx<string>();

            // Substituting.
            var result = new StringBuilder();
            foreach (var idx in original)
            {
                if (what.IndexOf(idx) != -1)
                    result.Append(idx);
                else
                    result.Append(with);
            }
            input.Value = result.ToString();
        }
    }
}
