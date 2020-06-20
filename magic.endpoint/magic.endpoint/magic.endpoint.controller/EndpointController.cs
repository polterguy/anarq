/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;
using magic.endpoint.contracts;

namespace magic.endpoint.controller
{
    /// <summary>
    /// Hyperlambda controller for evaluating a Hyperlambda file, from a URL
    /// and a verb, allowing the caller tooptionally pass in arguments, if the
    /// endpoint can accept arguments.
    /// </summary>
    [Route("/{*url}", Order = int.MaxValue)]
    public class EndpointController : ControllerBase
    {
        readonly IExecutorAsync _executor;

        /// <summary>
        /// Creates a new instance of your type.
        /// </summary>
        /// <param name="executor">Service implementation.</param>
        public EndpointController(IExecutorAsync executor)
        {
            _executor = executor ?? throw new ArgumentNullException(nameof(executor));
        }

        /// <summary>
        /// Executes a dynamically registered Hyperlambda HTTP GET endpoint.
        /// </summary>
        /// <param name="url">The requested URL.</param>
        [HttpGet]
        public async Task<ActionResult> Get(string url)
        {
            DateTime ifModifiedSince = DateTime.MinValue;
            if (Request.Headers.ContainsKey("If-Modified-Since"))
            {
                Request.Headers.TryGetValue("If-Modified-Since", out StringValues ifModifiedSinceHeader);
                ifModifiedSince = DateTime.ParseExact(
                    ifModifiedSinceHeader.ToString(),
                    "ddd, dd MMM yyyy HH:mm:ss 'GMT'",
                    CultureInfo.InvariantCulture.DateTimeFormat,
                    DateTimeStyles.AssumeUniversal);
                ifModifiedSince = ifModifiedSince.ToUniversalTime();
            }
            return TransformToActionResult(
                await _executor.ExecuteGetAsync(
                    WebUtility.UrlDecode(url),
                    ifModifiedSince,
                    Request.Query.Select(x => new Tuple<string, string>(x.Key, x.Value))));
        }

        /// <summary>
        /// Executes a dynamically registered Hyperlambda HTTP DELETE endpoint.
        /// </summary>
        /// <param name="url">The requested URL.</param>
        [HttpDelete]
        public async Task<ActionResult> Delete(string url)
        {
            return TransformToActionResult(
                await _executor.ExecuteDeleteAsync(
                    WebUtility.UrlDecode(url), 
                    Request.Query.Select(x => new Tuple<string, string>(x.Key, x.Value))));
        }

        /// <summary>
        /// Executes a dynamically registered Hyperlambda HTTP POST endpoint.
        /// </summary>
        /// <param name="url">The requested URL.</param>
        /// <param name="payload">Payload from client.</param>
        [HttpPost]
        public async Task<ActionResult> Post(string url, [FromBody] JContainer payload)
        {
            return TransformToActionResult(
                await _executor.ExecutePostAsync(
                    WebUtility.UrlDecode(url),
                    Request.Query.Select(x => new Tuple<string, string>(x.Key, x.Value)),
                    payload));
        }

        /// <summary>
        /// Executes a dynamically registered Hyperlambda HTTP PUT endpoint.
        /// </summary>
        /// <param name="url">The requested URL.</param>
        /// <param name="payload">Payload from client.</param>
        [HttpPut]
        public async Task<ActionResult> Put(string url, [FromBody] JContainer payload)
        {
            return TransformToActionResult(
                await _executor.ExecutePutAsync(
                    WebUtility.UrlDecode(url),
                    Request.Query.Select(x => new Tuple<string, string>(x.Key, x.Value)),
                    payload));
        }

        #region [ -- Private helper methods -- ]

        /*
         * Transforms from our internal HttpResponse wrapper to an ActionResult
         */
        ActionResult TransformToActionResult(HttpResponse response)
        {
            // Making sure we attach any HTTP headers to the response.
            foreach (var idx in response.Headers)
            {
                Response.Headers.Add(idx.Key, idx.Value);
            }

            // If empty result, we return nothing.
            if (response.Content == null)
                return new StatusCodeResult(response.Result);

            // Sanity checking to verify status code is success.
            if (response.Result < 200 || response.Result >= 300)
            {
                // Making sure we dispose any already added streams/disposables, if already added.
                if (response.Content is IDisposable strResponse)
                    strResponse.Dispose();

                return new StatusCodeResult(response.Result);
            }

            // Defaulting Content-Type return header to "application/octet-stream" if no header has been explicitly set.
            var contentType = response.Headers["Content-Type"];

            // Checking if this is a stream content result.
            if (response.Content is Stream stream)
                return new FileStreamResult(stream, contentType);

            // Figuring out type of result, and acting accordingly.
            switch (contentType)
            {
                case "application/json":

                    // JSON result, converting if necessary.
                    if (response.Content is string strContent)
                        return new JsonResult(JToken.Parse(strContent)) { StatusCode = response.Result };
                    return new ObjectResult(response.Content) { StatusCode = response.Result };

                default:

                    // Default, returning as "whatever content".
                    if (contentType.StartsWith("text/"))
                        return new ContentResult
                        {
                            StatusCode = response.Result,
                            Content = Convert.ToString(response.Content ?? "", CultureInfo.InvariantCulture),
                            ContentType = contentType,
                        };
                    return new ObjectResult(response.Content) { StatusCode = response.Result };
            }
        }

        #endregion
    }
}

