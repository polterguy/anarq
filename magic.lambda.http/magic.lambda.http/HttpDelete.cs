/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using magic.node;
using magic.http.contracts;
using magic.node.extensions;
using magic.signals.contracts;

namespace magic.lambda.http
{
    /// <summary>
    /// Invokes the HTTP DELETE verb towards some resource.
    /// </summary>
    [Slot(Name = "http.delete")]
    [Slot(Name = "wait.http.delete")]
    public class HttpDelete : ISlot, ISlotAsync
    {
        readonly IHttpClient _httpClient;

        /// <summary>
        /// Creates a new instance of your class.
        /// </summary>
        /// <param name="httpClient">HTTP client to use for invocation.</param>
        public HttpDelete(IHttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        /// <summary>
        /// Implementation of your slot.
        /// </summary>
        /// <param name="signaler">Signaler that raised the signal.</param>
        /// <param name="input">Arguments to your slot.</param>
        public void Signal(ISignaler signaler, Node input)
        {
            // Sanity checking input arguments.
            Common.SanityCheckInput(input);

            // Retrieving URL and (optional) token.
            var url = input.GetEx<string>();
            var token = input.Children.FirstOrDefault(x => x.Name == "token")?.GetEx<string>();
            var headers = input.Children.FirstOrDefault(x => x.Name == "headers")?.Children
                .ToDictionary(x1 => x1.Name, x2 => x2.GetEx<string>());

            // Invoking endpoint and returning result as value of root node.
            var response = token == null ?
                _httpClient.DeleteAsync<string>(url, headers).GetAwaiter().GetResult():
                _httpClient.DeleteAsync<string>(url, token).GetAwaiter().GetResult();
            Common.CreateResponse(input, response);
        }

        /// <summary>
        /// Implementation of your slot.
        /// </summary>
        /// <param name="signaler">Signaler that raised the signal.</param>
        /// <param name="input">Arguments to your slot.</param>
        /// <returns>An awaitable task.</returns>
        public async Task SignalAsync(ISignaler signaler, Node input)
        {
            // Sanity checking input arguments.
            Common.SanityCheckInput(input);

            // Retrieving URL and (optional) token.
            var url = input.GetEx<string>();
            var token = input.Children.FirstOrDefault(x => x.Name == "token")?.GetEx<string>();
            var headers = input.Children.FirstOrDefault(x => x.Name == "headers")?.Children
                .ToDictionary(x1 => x1.Name, x2 => x2.GetEx<string>());

            // Invoking endpoint and returning result as value of root node.
            var response = token == null ?
                await _httpClient.DeleteAsync<string>(url, headers) :
                await _httpClient.DeleteAsync<string>(url, token);
            Common.CreateResponse(input, response);
        }
    }
}
