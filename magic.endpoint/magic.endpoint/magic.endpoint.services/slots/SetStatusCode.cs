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
    /// [http.response.status-code] slot for modifying the HTTP status code of the response.
    /// </summary>
    [Slot(Name = "http.response.status-code.set")]
    public class SetStatusCode : ISlot
    {
        /// <summary>
        /// Implementation of your slot.
        /// </summary>
        /// <param name="signaler">Signaler used to raise signal.</param>
        /// <param name="input">Arguments to your slot.</param>
        public void Signal(ISignaler signaler, Node input)
        {
            var response = signaler.Peek<HttpResponse>("http.response");
            response.Result = input.GetEx<int>();
        }
    }
}
