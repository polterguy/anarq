/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.Linq;
using magic.node;
using magic.http.contracts;

namespace magic.lambda.http
{
    /*
     * Helper class to create some sort of Node result out of an HTTP response.
     */
    internal static class Common
    {
        /*
         * Creates a common lambda structure out of an HTTP Response, adding
         * the HTTP headers, status code, and content into the lambda structure.
         */
        public static void CreateResponse(Node input, Response<string> response)
        {
            input.Clear();
            input.Value = (int)response.Status;
            input.Add(new Node("headers", null, response.Headers.Select(x => new Node(x.Key, x.Value))));
            input.Add(new Node("content", response.Content));
        }

        /*
         * Sanity checks input arguments to verify no unsupported arguments are given
         * for an HTTP REST invocation.
         */
        public static void SanityCheckInput(Node input, bool allowPayload = false)
        {
            if (allowPayload)
            {
                if (input.Children.Count() > 2 || input.Children.Any(x => x.Name != "token" && x.Name != "headers" && x.Name != "payload"))
                    throw new ArgumentException("[http.xxx] can only handle one [token] or alternatively [headers] child node in addition to a [payload] argument");
            }
            else
            {
                if (input.Children.Count() > 1 || input.Children.Any(x => x.Name != "token" && x.Name != "headers"))
                    throw new ArgumentException("[http.xxx] can only handle one [token] or alternatively [headers] child node");
            }
        }
    }
}
