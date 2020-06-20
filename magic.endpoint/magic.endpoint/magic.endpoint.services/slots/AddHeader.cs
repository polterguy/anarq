/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using magic.node;
using magic.node.extensions;
using magic.signals.contracts;
using magic.endpoint.contracts;

namespace magic.endpoint.services.slots
{
    /// <summary>
    /// [http.response.headers.add] slot for adding a Response HTTP header that will be
    /// returned back to the client as an HTTP header.
    /// </summary>
    [Slot(Name = "http.response.headers.add")]
    public class AddHeader : ISlot
    {
        /// <summary>
        /// Implementation of your slot.
        /// </summary>
        /// <param name="signaler">Signaler used to raise signal.</param>
        /// <param name="input">Arguments to your slot.</param>
        public void Signal(ISignaler signaler, Node input)
        {
            var response = signaler.Peek<HttpResponse>("http.response");
            foreach (var idx in input.Children)
            {
                response.Headers[idx.Name] = idx.GetEx<string>();
            }
        }
    }
}
