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
    /// [date.format] slot, allowing you to format dates.
    /// </summary>
    [Slot(Name = "date.format")]
    public class DateTimeFormat : ISlot
    {
        /// <summary>
        /// Implementation of signal
        /// </summary>
        /// <param name="signaler">Signaler used to signal</param>
        /// <param name="input">Parameters passed from signaler</param>
        public void Signal(ISignaler signaler, Node input)
        {
            var format = input.Children.FirstOrDefault(x => x.Name == "format")?.GetEx<string>();
            if (format == null)
                throw new ArgumentNullException("No [format] provide to [date.format]");
            input.Value = input.GetEx<DateTime>().ToString(format, CultureInfo.InvariantCulture);
        }
    }
}
