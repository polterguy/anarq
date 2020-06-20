/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.Linq;
using magic.node;
using magic.node.extensions;
using magic.signals.contracts;

namespace magic.lambda.strings
{
    /// <summary>
    /// [strings.trim]/[strings.trim-start]/[strings.trim-end] slot for trimming a specified string,
    /// optionally passing in a string that defines which characters to trim away.
    /// </summary>
    [Slot(Name = "strings.trim")]
    [Slot(Name = "strings.trim-start")]
    [Slot(Name = "strings.trim-end")]
    public class Trim : ISlot
    {
        /// <summary>
        /// Implementation of slot.
        /// </summary>
        /// <param name="signaler">Signaler used to raise signal.</param>
        /// <param name="input">Arguments to your slot.</param>
        public void Signal(ISignaler signaler, Node input)
        {
            // Sanity checking.
            if (input.Children.Count() > 1)
                throw new ArgumentException("[strings.trim] can handle at most one argument");

            signaler.Signal("eval", input);

            var original = input.GetEx<string>();
            var what = input.Children.FirstOrDefault()?.GetEx<string>();
            if (what != null)
                input.Value = input.Name == "strings.trim-start" ?
                    original.TrimStart(what.ToCharArray()) :
                    input.Name == "strings.trim-end" ?
                        original.TrimEnd(what.ToCharArray()) : 
                        original.Trim(what.ToCharArray());
            else
                input.Value = input.Name == "strings.trim-start" ?
                    original.TrimStart() :
                    input.Name == "strings.trim-end" ?
                        original.TrimEnd() :
                        original.Trim();
        }
    }
}
