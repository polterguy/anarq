/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using magic.node;
using magic.node.extensions;
using magic.signals.contracts;

namespace magic.lambda.exceptions
{
    // TODO: Implement custom exception for this.
    /// <summary>
    /// [throw] slot that throws an exception.
    /// </summary>
    [Slot(Name = "throw")]
    public class Throw : ISlot
    {
        /// <summary>
        /// Implementation of signal
        /// </summary>
        /// <param name="signaler">Signaler used to signal</param>
        /// <param name="input">Parameters passed from signaler</param>
        public void Signal(ISignaler signaler, Node input)
        {
            throw new ApplicationException(input.GetEx<string>() ?? "[no-message]");
        }
    }
}
