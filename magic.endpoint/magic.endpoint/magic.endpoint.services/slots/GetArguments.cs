/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.IO;
using System.Linq;
using magic.node;
using magic.node.extensions;
using magic.signals.contracts;
using magic.endpoint.services.utilities;
using magic.node.extensions.hyperlambda;

namespace magic.endpoint.services.slots
{
    /// <summary>
    /// [system.endpoint] slot for retrieving the arguments and meta
    /// information your Magic endpoint can handle.
    /// </summary>
    [Slot(Name = "endpoints.get-arguments")]
    public class GetArguments : ISlot
    {
        /// <summary>
        /// Implementation of your slot.
        /// </summary>
        /// <param name="signaler">Signaler used to raise signal.</param>
        /// <param name="input">Arguments to your slot.</param>
        public void Signal(ISignaler signaler, Node input)
        {
            // Retrieving arguments to invocation.
            var url = input.Children.First(x => x.Name == "url").GetEx<string>();
            var verb = input.Children.First(x => x.Name == "verb").GetEx<string>();
            if (!Utilities.IsLegalHttpName(url))
                throw new ApplicationException($"Oops, '{url}' is not a valid HTTP URL for Magic");

            switch (verb)
            {
                case "get":
                case "delete":
                case "post":
                case "put":
                    break;
                default:
                    throw new ApplicationException($"I don't know how to '{verb}', only 'post', 'put', 'delete' and 'get'");
            }

            // Cleaning out results.
            input.Clear();

            /*
             * Opening file, and trying to find its [.arguments] node.
             * 
             * Notice the Substring(6) invocation simply removes the "magic/"
             * parts of the URL.
             */
            var filename = Utilities.RootFolder + url.TrimStart('/').Substring(6) + "." + verb + ".hl";
            if (!File.Exists(filename))
                throw new ApplicationException($"No endpoint found at '{url}' for verb '{verb}'");

            // Parsing file as Hyperlambda.
            using (var stream = File.OpenRead(filename))
            {
                var lambda = new Parser(stream).Lambda();
                var argsNode = lambda.Children.FirstOrDefault(x => x.Name == ".arguments");
                if (argsNode == null)
                    return;

                // We have arguments in file endpoint.
                input.AddRange(argsNode.Children.ToList());
            }
        }
    }
}
