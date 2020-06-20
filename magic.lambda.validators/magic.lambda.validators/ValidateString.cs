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
    /// [validators.string] slot, for verifying that some string is between [min] and [max] in length.
    /// </summary>
    [Slot(Name = "validators.string")]
    public class ValidateString : ISlot
    {
        /// <summary>
        /// Implementation of slot.
        /// </summary>
        /// <param name="signaler">Signaler used to raise the signal.</param>
        /// <param name="input">Arguments to signal.</param>
        public void Signal(ISignaler signaler, Node input)
        {
            var min = input.Children.FirstOrDefault(x => x.Name == "min")?.GetEx<int>() ?? 0;
            var max = input.Children.FirstOrDefault(x => x.Name == "max")?.GetEx<int>() ?? int.MaxValue;
            var value = input.GetEx<string>();
            input.Value = null;
            input.Clear();
            if (value.Length < min || value.Length > max)
                throw new ArgumentException($"'{value}' for is not between {min} and {max} in length, which is a mandatory condition");
        }
    }
}
