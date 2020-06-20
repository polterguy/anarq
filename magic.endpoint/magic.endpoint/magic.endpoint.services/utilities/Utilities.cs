
/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;

namespace magic.endpoint.services.utilities
{
    /// <summary>
    /// Utility class, mostly here to retrieve and set the RootFolder of where
    /// to resolve Hyperlambda files.
    /// </summary>
    public static class Utilities
    {
        /// <summary>
        /// The root folder from where to resolve dynamic Hyperlambda files from.
        /// 
        /// Notice, this property needs to be set by client code before evaluating dynamic
        /// Hyperlambda files.
        /// </summary>
        public static string RootFolder { get; set; }

        /*
         * Returns true if request URL contains only legal characters.
         */
        internal static bool IsLegalHttpName(string requestUrl)
        {
            foreach (var idx in requestUrl)
            {
                switch (idx)
                {
                    case '-':
                    case '_':
                    case '/':
                        break;
                    default:
                        if ((idx < 'a' || idx > 'z') &&
                            (idx < 'A' || idx > 'Z') &&
                            (idx < '0' || idx > '9'))
                            return false;
                        break;
                }
            }
            return true;
        }

        /*
         * Returns the path to the endpoints file matching the specified
         * URL and verb.
         */
        internal static string GetEndpointFile(string url, string verb)
        {
            // Sanity checking invocation.
            if (!IsLegalHttpName(url))
                throw new ApplicationException($"The URL '{url}' is not a legal URL for Magic");

            // Making sure we resolve "magic/" folder files correctly.
            if (url.StartsWith("magic/"))
                return RootFolder + url.Substring(6) + $".{verb}.hl";

            // Default URL resolver for anything but "module/" files.
            return RootFolder + "url-resolver.hl";
        }
    }
}
