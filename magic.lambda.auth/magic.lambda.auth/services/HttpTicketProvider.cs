/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.Linq;
using System.Security.Claims;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using magic.lambda.auth.contracts;

namespace magic.lambda.auth.services
{
    /// <summary>
    /// HTTP ticket provider service implementation.
    /// Provides a thin layer of abstraction between retrieving authenticated user, 
    /// and the HttpContext to disassociate the HttpContext form the ticket declaring currently logged in user.
    /// </summary>
    public class HttpTicketProvider : ITicketProvider
    {
        readonly IHttpContextAccessor _contextAccessor;

        /// <summary>
        /// Creates a new instance of class.
        /// </summary>
        /// <param name="contextAccessor">HTTP context accessor, necessary to retrieve the currently 
        /// authenticated user from the HttpContext.</param>
        public HttpTicketProvider(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
        }

        /// <summary>
        /// Returns username for currently logged in user.
        /// </summary>
        public string Username => _contextAccessor.HttpContext.User.Identity.Name;

        /// <summary>
        /// Returns all roles for currently logged in user, if any.
        /// </summary>
        public IEnumerable<string> Roles => (_contextAccessor.HttpContext.User.Identity as ClaimsIdentity).Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(x => x.Value);

        /// <summary>
        /// Returns true if user belongs to specified role.
        /// </summary>
        /// <param name="role">Role tro check for.</param>
        /// <returns>True if user belongs to role.</returns>
        public bool InRole(string role)
        {
            return _contextAccessor.HttpContext.User.IsInRole(role?.Trim() ?? throw new ArgumentNullException(nameof(role)));
        }

        /// <summary>
        /// Returns true if user is authenticated at all.
        /// </summary>
        /// <returns>True if user is authenticated.</returns>
        public bool IsAuthenticated()
        {
            return _contextAccessor.HttpContext.User.Identity.IsAuthenticated;
        }
    }
}
