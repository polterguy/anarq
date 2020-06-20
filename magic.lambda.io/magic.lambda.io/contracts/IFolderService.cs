/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System.Collections.Generic;

namespace magic.lambda.io.contracts
{
    /// <summary>
    /// Contracts for handling folders for magic.lambda.io
    /// </summary>
    public interface IFolderService
    {
        /// <summary>
        /// Creates a new folder with hte specified path.
        /// </summary>
        /// <param name="path">Path to folder.</param>
        void Create(string path);

        /// <summary>
        /// returns true if specified folder exists.
        /// </summary>
        /// <param name="path">Path to folder to check for.</param>
        /// <returns>True if folder exists.</returns>
        bool Exists(string path);

        /// <summary>
        /// Deletes the specified folder.
        /// </summary>
        /// <param name="path">Path of folder to delete.</param>
        void Delete(string path);

        /// <summary>
        /// Lists all folders within the specified folder.
        /// </summary>
        /// <param name="folder">Folder to query for folders.</param>
        /// <returns>Absolute paths to ist of folders the specified folder contains</returns>
        IEnumerable<string> ListFolders(string folder);
    }
}
