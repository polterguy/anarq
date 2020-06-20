/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.Linq;
using System.Globalization;
using magic.node;
using magic.node.extensions;
using magic.signals.contracts;

namespace magic.lambda.types
{
    /// <summary>
    /// [date.now] slot, allowing you to retrieve server time.
    /// </summary>
    [Slot(Name = "date.future")]
    public class DateTimeFuture : ISlot
    {
        /// <summary>
        /// Implementation of signal
        /// </summary>
        /// <param name="signaler">Signaler used to signal</param>
        /// <param name="input">Parameters passed from signaler</param>
        public void Signal(ISignaler signaler, Node input)
        {
            var offset = input.Children.SingleOrDefault() ??
                throw new ArgumentNullException("No offset parameter, or multiple parameters supplied to [date.future]");
            var value = offset.GetEx<int>();
            var date = DateTime.Now;
            switch (offset.Name)
            {
                case "years":
                    date = date.AddYears(value);
                    break;
                case "months":
                    date = date.AddMonths(value);
                    break;
                case "days":
                    date = date.AddDays(value);
                    break;
                case "hours":
                    date = date.AddHours(value);
                    break;
                case "minutes":
                    date = date.AddMinutes(value);
                    break;
                case "seconds":
                    date = date.AddSeconds(value);
                    break;
            }
            input.Value = date;
        }
    }
}
