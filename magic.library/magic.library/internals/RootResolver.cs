/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System.IO;
using magic.lambda.io.contracts;
using Microsoft.Extensions.Configuration;

namespace magic.library.internals
{
    internal sealed class RootResolver : IRootResolver
    {
        public RootResolver(IConfiguration configuration)
        {
            RootFolder = (configuration["magic:io:root-folder"] ?? "~/files/")
                .Replace("~", Directory.GetCurrentDirectory())
                .TrimEnd('/') + "/";
        }

        public string RootFolder { get; }
    }
}