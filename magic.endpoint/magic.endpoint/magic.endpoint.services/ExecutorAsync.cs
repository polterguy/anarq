/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using magic.node;
using magic.node.extensions;
using magic.signals.contracts;
using magic.endpoint.contracts;
using magic.endpoint.services.utilities;
using magic.node.extensions.hyperlambda;

namespace magic.endpoint.services
{
    /// <summary>
    /// Implementation of IExecutor contract, allowing you to
    /// execute a dynamically created Hyperlambda endpoint.
    /// </summary>
    public class ExecutorAsync : IExecutorAsync
    {
        readonly ISignaler _signaler;
        readonly IConfiguration _configuration;

        /// <summary>
        /// Creates an instance of your type.
        /// </summary>
        /// <param name="signaler">Signaler necessary evaluate endpoint.</param>
        /// <param name="configuration">Configuration object for application.</param>
        public ExecutorAsync(ISignaler signaler, IConfiguration configuration)
        {
            _signaler = signaler ?? throw new ArgumentNullException(nameof(signaler));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Executes an HTTP GET endpoint with the specified URL and the
        /// specified arguments.
        /// </summary>
        /// <param name="url">URL that was requested.</param>
        /// <param name="ifModifiedSince">Only return document if it has been modified since this date.</param>
        /// <param name="args">Arguments to your endpoint.</param>
        /// <returns>The result of the evaluation.</returns>
        public async Task<HttpResponse> ExecuteGetAsync(
            string url,
            DateTime ifModifiedSince,
            IEnumerable<Tuple<string, string>> args)
        {
            // Notice, supporting static document, and default document.
            if (url == null)
            {
                // Checking if default static document exists.
                if (File.Exists(Utilities.RootFolder + "static/index.html"))
                    return GetStaticDocument("index.html", ifModifiedSince);
            }
            else
            {
                // Checking if this is a request for a static document.
                if (url.Contains("."))
                    return GetStaticDocument(url, ifModifiedSince);
            }

            // Executing dynamically resolved URL.
            return await ExecuteUrl(url, "get", ifModifiedSince, args, null);
        }

        /// <summary>
        /// Executes an HTTP DELETE endpoint with the specified URL and the
        /// specified arguments.
        /// </summary>
        /// <param name="url">URL that was requested.</param>
        /// <param name="args">Arguments to your endpoint.</param>
        /// <returns>The result of the evaluation.</returns>
        public async Task<HttpResponse> ExecuteDeleteAsync(
            string url, 
            IEnumerable<Tuple<string, string>> args)
        {
            return await ExecuteUrl(url, "delete", DateTime.MinValue, args);
        }

        /// <summary>
        /// Executes an HTTP POST endpoint with the specified URL and the
        /// specified payload.
        /// </summary>
        /// <param name="url">URL that was requested.</param>
        /// <param name="args">Arguments to your endpoint.</param>
        /// <param name="payload">JSON payload to your endpoint.</param>
        /// <returns>The result of the evaluation.</returns>
        public async Task<HttpResponse> ExecutePostAsync(
            string url,
            IEnumerable<Tuple<string, string>> args,
            JContainer payload)
        {
            return await ExecuteUrl(url, "post", DateTime.MinValue, args, payload);
        }

        /// <summary>
        /// Executes an HTTP PUT endpoint with the specified URL and the
        /// specified payload.
        /// </summary>
        /// <param name="url">URL that was requested.</param>
        /// <param name="args">Arguments to your endpoint.</param>
        /// <param name="payload">JSON payload to your endpoint.</param>
        /// <returns>The result of the evaluation.</returns>
        public async Task<HttpResponse> ExecutePutAsync(
            string url,
            IEnumerable<Tuple<string, string>> args,
            JContainer payload)
        {
            return await ExecuteUrl(url, "put", DateTime.MinValue, args, payload);
        }

        #region [ -- Private helper methods -- ]

        /*
         * Returns a static document to caller.
         */
        HttpResponse GetStaticDocument(string url, DateTime ifModifiedSince)
        {
            // Checking that file exists.
            var fullpath = Utilities.RootFolder + "static/" + url.TrimStart('/');
            if (!File.Exists(fullpath))
            {
                return new HttpResponse
                {
                    Result = 404
                };
            }

            // Checking if we should return a 304.
            var fileTime = File.GetLastWriteTimeUtc(fullpath);
            if (ifModifiedSince > fileTime)
            {
                // That sweet 304 response!
                return new HttpResponse
                {
                    Result = 304,
                };
            }

            // Returning file to caller, making sure we get the MIME type correct.
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(fullpath, out string contentType))
                contentType = "application/octet-stream";
            var result = new HttpResponse
            {
                Content = File.OpenRead(fullpath),
                Result = 200,
            };

            // Applying HTTP headers according to settings.
            result.Headers["Content-Type"] = contentType;
            result.Headers["Last-Modified"] = fileTime.ToString("r");
            result.Headers["Cache-Control"] = "public, max-age=" + _configuration["magic:staticFiles:maxAge"];
            return result;
        }

        /*
         * Executes a URL that was given QUERY arguments.
         */
        async Task<HttpResponse> ExecuteUrl(
            string url,
            string verb,
            DateTime ifModifiedSince,
            IEnumerable<Tuple<string, string>> args,
            JContainer payload = null)
        {
            // Defaulting url to "" if not given.
            url = url ?? "";

            // Retrieving file, and verifying it exists.
            var path = Utilities.GetEndpointFile(url, verb);
            if (!File.Exists(path))
                return new HttpResponse { Result = 404 };

            // Reading and parsing file as Hyperlambda.
            using (var stream = File.OpenRead(path))
            {
                // Creating a lambda object out of file.
                var lambda = new Parser(stream).Lambda();

                /*
                 * Attaching arguments to lambda, which will also to some
                 * extent sanity check the arguments, and possibly convert
                 * them according to the declaration node.
                 *
                 * Notice, if we are requesting a non-magic URL, we also attach the "If-Modified-Since" HTTP
                 * header, to allow for our dynamic URL resolver to return 304 responses.
                 */
                AttachArguments(lambda, url, args, payload);
                if (verb == "get" && !url.StartsWith("magic/"))
                    lambda.Children.First(x => x.Name == ".arguments").Add(new Node("If-Modified-Since", ifModifiedSince));

                /*
                 * Evaluating our lambda async, making sure we allow for the
                 * lambda object to return values, and to modify the response HTTP headers,
                 * and its status code, etc.
                 */
                var evalResult = new Node();
                var httpResponse = new HttpResponse();

                /*
                 * Making sure we default content type to "application/json" if this is a
                 * "magically resolved URL". If it's not, we default content to HTML.
                 */
                if (url.StartsWith("magic/"))
                    httpResponse.Headers["Content-Type"] = "application/json";
                else
                    httpResponse.Headers["Content-Type"] = "text/html";

                /*
                 * Evaluating our lambda async, making sure we allow for the
                 * lambda object to return values, and to modify the response HTTP headers,
                 * and its status code, etc.
                 *
                 * Wrapping execution in try/catch block, to make sure we dispose disposables
                 * in case of exceptions.
                 */
                try
                {
                    await _signaler.ScopeAsync("http.response", httpResponse, async () =>
                    {
                        await _signaler.ScopeAsync("slots.result", evalResult, async () =>
                        {
                            await _signaler.SignalAsync("wait.eval", lambda);
                        });
                    });

                    // Retrieving content for request.
                    httpResponse.Content = GetReturnValue(evalResult);
                    return httpResponse;
                }
                catch
                {
                    if (evalResult.Value is IDisposable disposable)
                        disposable.Dispose();
                    if (httpResponse.Content is IDisposable disposable2)
                        disposable2.Dispose();
                    throw;
                }
            }
        }

        /*
         * Attaches arguments (payload + query params) to lambda node.
         */
        void AttachArguments(
            Node lambda, 
            string url,
            IEnumerable<Tuple<string, string>> args, 
            JContainer payload)
        {
            // Checking if file has [.arguments] node, and removing it if it exists.
            var fileArgs = lambda.Children.FirstOrDefault(x => x.Name == ".arguments");
            fileArgs?.UnTie();

            // Our arguments node.
            var argsNode = new Node(".arguments");

            // We only pass in URL if this is not a Magically resolved URL.
            if (!url.StartsWith("magic/"))
                argsNode.Add(new Node("url", url));

            // First payload arguments.
            if (payload != null)
            {
                // Converting the given arguments from JSON to lambda.
                argsNode.Value = payload;
                _signaler.Signal(".json2lambda-raw", argsNode);
                argsNode.Value = null; // To remove actual JContainer from node.

                /*
                 * Checking if we need to convert the individual arguments,
                 * which is true if lambda file contains [.arguments] declaration.
                 */
                if (fileArgs != null)
                {
                    foreach (var idxArg in argsNode.Children)
                    {
                        idxArg.Value = ConvertArgument(
                            idxArg,
                            fileArgs.Children.FirstOrDefault(x => x.Name == idxArg.Name));
                    }
                }
            }

            // Then doing QUERY parameters.
            if (args != null)
            {
                foreach (var idxArg in args)
                {
                    object value = idxArg.Item2;
                    var declaration = fileArgs?.Children.FirstOrDefault(x => x.Name == idxArg.Item1);
                    if (declaration != null)
                        value = Parser.ConvertValue(idxArg.Item2, declaration.Get<string>());
                    argsNode.Add(
                        new Node(
                            idxArg.Item1,
                            value));
                }
            }

            // Inserting the arguments specified to the endpoint as arguments, but only if there are any arguments.
            lambda.Insert(0, argsNode);
        }

        /*
         * Converts the given input argument to the type specified in the
         * declaration node. Making sure the argument is legally given to the
         * endpoint.
         */
        object ConvertArgument(Node node, Node declaration)
        {
            if (declaration == null)
                throw new ApplicationException($"I don't know how to handle the '{node.Name}' argument");

            if (node.Value == null)
                return null; // Allowing for null values

            var type = declaration.Get<string>();
            if (string.IsNullOrEmpty(type))
            {
                // No conversion can be done on main node, but declaration node might have children.
                if (declaration.Children.Any())
                {
                    if (node.Children.Count() == 1 && node.Children.First().Name == "." && node.Children.First().Value == null)
                    {
                        // Array!
                        if (declaration.Children.Count() != 1 || declaration.Children.First().Name != "." || declaration.Children.First().Value != null)
                            throw new ArgumentException($"We were given an array argument ('{node.Children.First().Value}') where an object argument was expected.");

                        foreach (var idxArg in node.Children.First().Children)
                        {
                            idxArg.Value = ConvertArgument(idxArg, declaration.Children.First().Children.FirstOrDefault(x => x.Name == idxArg.Name));
                        }
                    }
                    else
                    {
                        // Object!
                        foreach (var idxArg in node.Children)
                        {
                            idxArg.Value = ConvertArgument(idxArg, declaration.Children.FirstOrDefault(x => x.Name == idxArg.Name));
                        }
                    }
                }
                return node.Value;

            } else if (type == "*")
            {
                // Any object tolerated!
                return node.Value;
            }
            return Parser.ConvertValue(node.Value, type);
        }

        /*
         * Creates a JContainer of some sort from the given lambda node.
         */
        object GetReturnValue(Node lambda)
        {
            // Checking if we have a value.
            if (lambda.Value != null)
            {
                if (lambda.Value is Stream)
                    return lambda.Value;
                return lambda.Get<string>();
            }

            // Checking if we have children.
            if (lambda.Children.Any())
            {
                var convert = new Node();
                convert.AddRange(lambda.Children.ToList());
                _signaler.Signal(".lambda2json-raw", convert);
                return convert.Value as JToken;
            }

            // Nothing here ...
            return null;
        }

        #endregion
    }
}
