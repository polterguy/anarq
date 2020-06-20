/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace magic.endpoint.contracts
{
    /// <summary>
    /// Service interface for executing a Magic endpoint when some URL is
    /// requested.
    /// </summary>
    public interface IExecutorAsync
    {
        /// <summary>
        /// Executes an HTTP GET endpoint with the specified URL and the
        /// specified QUERY arguments.
        /// </summary>
        /// <param name="url">URL that was requested.</param>
        /// <param name="ifModifiedSince">Only return document if it has been modified since this date.</param>
        /// <param name="args">QUERY arguments to your endpoint.</param>
        /// <returns>The result of the evaluation.</returns>
        Task<HttpResponse> ExecuteGetAsync(
            string url, 
            DateTime ifModifiedSince,
            IEnumerable<Tuple<string, string>> args);

        /// <summary>
        /// Executes an HTTP DELETE endpoint with the specified URL and the
        /// specified QUERY arguments.
        /// </summary>
        /// <param name="url">URL that was requested.</param>
        /// <param name="args">QUERY arguments to your endpoint.</param>
        /// <returns>The result of the evaluation.</returns>
        Task<HttpResponse> ExecuteDeleteAsync(
            string url, 
            IEnumerable<Tuple<string, string>> args);

        /// <summary>
        /// Executes an HTTP POST endpoint with the specified URL and the
        /// specified payload.
        /// </summary>
        /// <param name="url">URL that was requested.</param>
        /// <param name="args">HTTP arguments to endpoints.</param>
        /// <param name="payload">JSON payload to your endpoint.</param>
        /// <returns>The result of the evaluation.</returns>
        Task<HttpResponse> ExecutePostAsync(
            string url, 
            IEnumerable<Tuple<string, string>> args,
            JContainer payload);

        /// <summary>
        /// Executes an HTTP PUT endpoint with the specified URL and the
        /// specified payload.
        /// </summary>
        /// <param name="url">URL that was requested.</param>
        /// <param name="args">HTTP arguments to endpoints.</param>
        /// <param name="payload">JSON payload to your endpoint.</param>
        /// <returns>The result of the evaluation.</returns>
        Task<HttpResponse> ExecutePutAsync(
            string url, 
            IEnumerable<Tuple<string, string>> args, 
            JContainer payload);
    }
}
