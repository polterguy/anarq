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
    /// [strings.to-upper] slot that returns the uppercase value of its specified argument.
    /// </summary>
    [Slot(Name = "strings.to-upper")]
    public class ToUpper : ISlot
    {
        /// <summary>
        /// Implementation of slot.
        /// </summary>
        /// <param name="signaler">Signaler used to raise signal.</param>
        /// <param name="input">Arguments to your slot.</param>
        public void Signal(ISignaler signaler, Node input)
        {
            // Sanity checking.
            if (input.Children.Any())
                throw new ApplicationException("[strings.to-upper] must be given exactly one argument that contains value to UPPERCASE");

            input.Value = input.GetEx<string>().ToUpperInvariant();
        }
    }
}
