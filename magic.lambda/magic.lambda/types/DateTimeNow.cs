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
    [Slot(Name = "date.now")]
    public class DateTimeNow : ISlot
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
                input.Value = DateTime.Now;
            else
                input.Value = DateTime.Now.ToString(format, CultureInfo.InvariantCulture);
        }
    }
}
