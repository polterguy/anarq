/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System.Linq;
using MimeKit;
using magic.node;
using magic.node.extensions;
using magic.signals.contracts;

namespace magic.lambda.mime
{
    /// <summary>
    /// Parses a MimeEntity message and returns its as a hierarchical object of lambda to caller.
    /// </summary>
    [Slot(Name = ".mime.parse")]
    public class MimeParsePrivate : ISlot
    {
        /// <summary>
        /// Implementation of your slot.
        /// </summary>
        /// <param name="signaler">Signaler that raised the signal.</param>
        /// <param name="input">Arguments to your slot.</param>
        public void Signal(ISignaler signaler, Node input)
        {
            var message = input.Value as MimeEntity;
            helpers.MimeParser.Parse(
                input,
                message,
                (fingerprint) => input.Children
                    .FirstOrDefault(x => x.Name == "key")?
                    .GetEx<string>(),
                (sec) => input.Children
                    .FirstOrDefault(x => x.Name == "key")?
                    .Children
                    .FirstOrDefault(x => x.Name == "password")?
                    .GetEx<string>());
            input.Value = null;
        }
    }
}
