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
    /// [validators.date] slot, for verifying that some date is between [min] and [max] values.
    /// </summary>
    [Slot(Name = "validators.date")]
    public class ValidateDate : ISlot
    {
        /// <summary>
        /// Implementation of slot.
        /// </summary>
        /// <param name="signaler">Signaler used to raise the signal.</param>
        /// <param name="input">Arguments to signal.</param>
        public void Signal(ISignaler signaler, Node input)
        {
            var min = input.Children.FirstOrDefault(x => x.Name == "min")?.GetEx<DateTime>() ?? DateTime.MinValue;
            var max = input.Children.FirstOrDefault(x => x.Name == "max")?.GetEx<DateTime>() ?? DateTime.MaxValue;
            var value = input.GetEx<DateTime>();
            input.Value = null;
            input.Clear();
            if (value < min || value > max)
                throw new ArgumentException($"The date time value of '{value}' is not between '{min}' and '{max}', which is a mandatory condition");
        }
    }
}
