/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System.IO;
using System.Text;
using System.Linq;
using MimeKit;
using magic.node;
using magic.node.extensions;
using magic.signals.contracts;

namespace magic.lambda.mime
{
    /// <summary>
    /// Parses a MIME message and returns its as a hierarchical object of lambda to caller.
    /// </summary>
    [Slot(Name = "mime.parse")]
    public class MimeParse : ISlot
    {
        /// <summary>
        /// Implementation of your slot.
        /// </summary>
        /// <param name="signaler">Signaler that raised the signal.</param>
        /// <param name="input">Arguments to your slot.</param>
        public void Signal(ISignaler signaler, Node input)
        {
            try
            {
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(input.GetEx<string>())))
                {
                    var message = MimeEntity.Load(stream);
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
            finally
            {
                input.Children.FirstOrDefault(x => x.Name == "key")?.UnTie();
            }
        }
    }
}
