/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.Linq;
using System.Text.RegularExpressions;
using magic.node;
using magic.node.extensions;
using magic.signals.contracts;

namespace magic.lambda.validators
{
    /// <summary>
    /// [validators.regex] slot, for verifying that some input is matching some specified regular expression found in [regex].
    /// </summary>
    [Slot(Name = "validators.regex")]
    public class ValidateRegex : ISlot
    {
        /// <summary>
        /// Implementation of slot.
        /// </summary>
        /// <param name="signaler">Signaler used to raise the signal.</param>
        /// <param name="input">Arguments to signal.</param>
        public void Signal(ISignaler signaler, Node input)
        {
            var value = input.GetEx<string>();
            var pattern = input.Children.First(x => x.Name == "regex").GetEx<string>();
            var isMatch = new Regex(pattern).IsMatch(value);
            if (!isMatch)
                throw new ArgumentException($"Value of '{value}' does not conform to regular expression of '{pattern}'");

            input.Value = null;
            input.Clear();
        }
    }
}
