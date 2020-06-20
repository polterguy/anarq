/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System.IO;
using magic.lambda.io.contracts;
using System.Collections.Generic;

namespace magic.lambda.io.folder.services
{
    /// <inheritdoc/>
    public class FolderService : IFolderService
    {
        /// <inheritdoc/>
        public void Create(string path)
        {
            Directory.CreateDirectory(path);
        }

        /// <inheritdoc/>
        public void Delete(string path)
        {
            Directory.Delete(path, true);
        }

        /// <inheritdoc/>
        public bool Exists(string path)
        {
            return Directory.Exists(path);
        }

        /// <inheritdoc/>
        public IEnumerable<string> ListFolders(string folder)
        {
            return Directory.GetDirectories(folder);
        }
    }
}
