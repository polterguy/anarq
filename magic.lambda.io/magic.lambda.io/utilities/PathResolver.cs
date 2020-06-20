/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.IO;
using magic.node;
using magic.node.extensions;
using magic.signals.contracts;
using magic.lambda.io.contracts;

namespace magic.lambda.io.utilities
{
    internal static class PathResolver
    {
        public static string CombinePaths(string root, string path)
        {
            return root.Replace("\\", "/").TrimEnd('/') + "/" + path.Replace("\\", "/").TrimStart('/');
        }

        public static string Normalize(string path)
        {
            return path.Replace("\\", "/").TrimEnd('/');
        }
    }
}
