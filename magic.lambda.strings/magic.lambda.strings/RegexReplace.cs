/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using magic.node;
using magic.node.extensions;
using magic.signals.contracts;

namespace magic.lambda.strings
{
    /// <summary>
    /// [strings.regex-replace] slot that will perform a substitution of the regular expression
    /// matches from [what] with [with] found in your source string. [what] is expected
    /// to be a valid regular expression.
    /// </summary>
    [Slot(Name = "strings.regex-replace")]
    [Slot(Name = "wait.strings.regex-replace")]
    public class RegexReplace : ISlot, ISlotAsync
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
                throw new ArgumentException("[strings.regex-replace] requires exactly two arguments, the first being a regular expression of what to look for, the other beings its substitute");

            signaler.Signal("eval", input);

            var original = input.GetEx<string>();
            var what = input.Children.First().GetEx<string>();
            var with = input.Children.Skip(1).First().GetEx<string>();

            // Substituting.
            var ex = new Regex(what);
            input.Value = ex.Replace(original, with);
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
                throw new ArgumentException("[strings.regex-replace] requires exactly two arguments, the first being a regular expression of what to look for, the other beings its substitute");

            await signaler.SignalAsync("wait.eval", input);

            var original = input.GetEx<string>();
            var what = input.Children.First().GetEx<string>();
            var with = input.Children.Skip(1).First().GetEx<string>();

            // Substituting.
            var ex = new Regex(what);
            input.Value = ex.Replace(original, with);
        }
    }
}
