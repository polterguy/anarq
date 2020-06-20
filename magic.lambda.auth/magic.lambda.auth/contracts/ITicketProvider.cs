/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System.Collections.Generic;

namespace magic.lambda.auth.contracts
{
    /// <summary>
    /// Interface for retrieving authenticated user.
    /// </summary>
    public interface ITicketProvider
    {
        /// <summary>
        /// Returns the currently authenticated user's username.
        /// </summary>
        string Username { get; }

        /// <summary>
        /// Returns all roles for currently authenticated user.
        /// </summary>
        IEnumerable<string> Roles { get; }

        /// <summary>
        /// Returns true if current user belongs to specified role.
        /// </summary>
        /// <param name="role">Role to check for.</param>
        /// <returns>True if user belongs to role.</returns>
        bool InRole(string role);

        /// <summary>
        /// Returns true if current user is authenticated.
        /// </summary>
        /// <returns>True if user is authenticated.</returns>
        bool IsAuthenticated();
    }
}
