/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.Linq;
using magic.node;
using magic.node.extensions;
using magic.signals.contracts;

namespace magic.lambda.validators
{
    /// <summary>
    /// [validators.enum] slot, for verifying that some string value is one of the specified options.
    /// </summary>
    [Slot(Name = "validators.enum")]
    public class ValidateEnum : ISlot
    {
        /// <summary>
        /// Implementation of slot.
        /// </summary>
        /// <param name="signaler">Signaler used to raise the signal.</param>
        /// <param name="input">Arguments to signal.</param>
        public void Signal(ISignaler signaler, Node input)
        {
            var value = input.GetEx<string>();
            if (!input.Children.Any(x => x.Get<string>() == value))
            {
                var legalValues = input.Children.Select(x => "'" + x.Get<string>() + "'");
                var legalValueString = string.Join(", ", legalValues.ToArray());
                input.Clear();
                throw new ArgumentException($"'{value}' is not a legal value for field, [{legalValueString}] is a legal value for input.");
            }
            input.Value = null;
            input.Clear();
        }
    }
}
