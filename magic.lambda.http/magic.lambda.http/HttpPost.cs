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
    /// Invokes the HTTP POST verb towards some resource.
    /// </summary>
    [Slot(Name = "http.post")]
    [Slot(Name = "wait.http.post")]
    public class HttpPost : ISlot, ISlotAsync
    {
        readonly IHttpClient _httpClient;

        /// <summary>
        /// Creates an instance of your class.
        /// </summary>
        /// <param name="httpClient">HTTP client to use for invocation.</param>
        public HttpPost(IHttpClient httpClient)
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
            Common.SanityCheckInput(input, true);

            var url = input.GetEx<string>();
            var token = input.Children.FirstOrDefault(x => x.Name == "token")?.GetEx<string>();
            var payload = input.Children.FirstOrDefault(x => x.Name == "payload")?.GetEx<string>() ??
                throw new ArgumentException("No [payload] supplied to [http.post]");
            var headers = input.Children.FirstOrDefault(x => x.Name == "headers")?.Children
                .ToDictionary(x1 => x1.Name, x2 => x2.GetEx<string>());

            // Invoking endpoint, passing in payload, and returning result as value of root node.
            var response = token == null ?
                _httpClient.PostAsync<string, string>(url, payload, headers).GetAwaiter().GetResult() :
                _httpClient.PostAsync<string, string>(url, payload, token).GetAwaiter().GetResult();
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
            Common.SanityCheckInput(input, true);

            var url = input.GetEx<string>();
            var token = input.Children.FirstOrDefault(x => x.Name == "token")?.GetEx<string>();
            var payload = input.Children.FirstOrDefault(x => x.Name == "payload")?.GetEx<string>() ??
                throw new ArgumentException("No [payload] supplied to [http.post]");
            var headers = input.Children.FirstOrDefault(x => x.Name == "headers")?.Children
                .ToDictionary(x1 => x1.Name, x2 => x2.GetEx<string>());

            // Invoking endpoint, passing in payload, and returning result as value of root node.
            var response = token == null ?
                await _httpClient.PostAsync<string, string>(url, payload, headers) :
                await _httpClient.PostAsync<string, string>(url, payload, token);
            Common.CreateResponse(input, response);
        }
    }
}
