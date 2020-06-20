/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

namespace magic.lambda.io.contracts
{
    // TODO: Rethink this. We have a slot now that does the same!
    /// <summary>
    /// Contracts for resolving root folder on disc for magic.lambda.io
    /// </summary>
    public interface IRootResolver
    {
        /// <summary>
        /// Returns the root folder that magic.lambda.io should treat as the root folder for its IO operations.
        /// </summary>
        string RootFolder { get; }
    }
}
