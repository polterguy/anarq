/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using magic.node;
using magic.signals.contracts;
using magic.lambda.mime.helpers;

namespace magic.lambda.mime
{
    /// <summary>
    /// Creates a MIME entity and returns it as a MimeKit MimeEntity to caller (hidden).
    /// 
    /// Notice, caller is responsible for disposing any streams created during process, but this
    /// can be easily done by using the MimeBuilder.DisposeStreams on the MimeEntity returned.
    /// </summary>
    [Slot(Name = ".mime.create")]
    public class MimeCreatePrivate : ISlot
    {
        /// <summary>
        /// Implementation of your slot.
        /// </summary>
        /// <param name="signaler">Signaler that raised the signal.</param>
        /// <param name="input">Arguments to your slot.</param>
        public void Signal(ISignaler signaler, Node input)
        {
            input.Value = MimeCreator.Create(signaler, input);
            input.Clear();
        }
    }
}
