/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System.IO;
using System.Linq;
using Microsoft.AspNetCore.StaticFiles;
using magic.node;
using magic.node.extensions;
using magic.signals.contracts;
using magic.endpoint.contracts;
using magic.endpoint.services.utilities;

namespace magic.endpoint.services.slots
{
    /// <summary>
    /// [http.response.return-file] slot for returning a file stream to caller.
    /// </summary>
    [Slot(Name = "http.response.return-file")]
    public class ReturnFile : ISlot
    {
        /// <summary>
        /// Implementation of your slot.
        /// </summary>
        /// <param name="signaler">Signaler used to raise signal.</param>
        /// <param name="input">Arguments to your slot.</param>
        public void Signal(ISignaler signaler, Node input)
        {
            // Figuring out which file caller wants to return.
            var filename = input.GetEx<string>();

            // Retrieving content response object such that we can set its content to the requested file stream.
            var response = signaler.Peek<HttpResponse>("http.response");

            // Decorating HTTP response headers according to what we know about file.
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(filename, out string contentType))
                contentType = "application/octet-stream";
            response.Headers["Content-Type"] = contentType;

            // Returning file to caller.
            response.Content = File.OpenRead(Utilities.RootFolder + filename);
        }
    }
}
