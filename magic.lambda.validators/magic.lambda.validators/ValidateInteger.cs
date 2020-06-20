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
    /// [validators.integer] slot, for verifying that some integer number is between [min] and [max] values.
    /// </summary>
    [Slot(Name = "validators.integer")]
    public class ValidateInteger : ISlot
    {
        /// <summary>
        /// Implementation of slot.
        /// </summary>
        /// <param name="signaler">Signaler used to raise the signal.</param>
        /// <param name="input">Arguments to signal.</param>
        public void Signal(ISignaler signaler, Node input)
        {
            var min = input.Children.FirstOrDefault(x => x.Name == "min")?.GetEx<long>() ?? long.MinValue;
            var max = input.Children.FirstOrDefault(x => x.Name == "max")?.GetEx<long>() ?? long.MaxValue;
            var value = input.GetEx<long>();
            input.Value = null;
            input.Clear();
            if (value < min || value > max)
                throw new ArgumentException($"{value} for is not between {min} and {max}, which is a mandatory condition");
        }
    }
}
