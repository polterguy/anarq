/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.Linq;
using System.Text;
using System.Security;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using magic.lambda.auth.contracts;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;

namespace magic.lambda.auth.helpers
{
    /// <summary>
    /// Helper class for creating, retrieving and verifying a JWT token from a ticket.
    /// </summary>
    public static class TicketFactory
    {
        /// <summary>
        /// Creates a JWT token from the specified ticket.
        /// </summary>
        /// <param name="configuration">Configuration settings.</param>
        /// <param name="ticket">Existing user ticket, containing username and roles.</param>
        /// <returns></returns>
        public static string CreateTicket(IConfiguration configuration, Ticket ticket)
        {
            // Getting data to put into token.
            var secret = configuration["magic:auth:secret"] ??
                throw new SecurityException("We couldn't find any 'magic:auth:secret' setting in your applications configuration");
            var validMinutes = int.Parse(configuration["magic:auth:valid-minutes"] ?? "20");
            var key = Encoding.UTF8.GetBytes(secret);

            // Creating our token descriptor.
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, ticket.Username),
                }),
                Expires = DateTime.UtcNow.AddMinutes(validMinutes),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            };

            // Adding all roles.
            tokenDescriptor.Subject.AddClaims(ticket.Roles.Select(x => new Claim(ClaimTypes.Role, x)));

            // Creating token and returning to caller.
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// Verifies that the current user belongs to the specified role.
        /// </summary>
        /// <param name="ticketProvider">Service provider, needed to retrieve the IHttpContextAccessor</param>
        /// <param name="roles"></param>
        public static void VerifyTicket(ITicketProvider ticketProvider, string roles)
        {
            if (!ticketProvider.IsAuthenticated())
                throw new SecurityException("Access denied");

            if (!string.IsNullOrEmpty(roles))
            {
                foreach (var idxRole in roles.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (ticketProvider.InRole(idxRole))
                        return;
                }
                throw new SecurityException("Access denied");
            }
        }

        /// <summary>
        /// Returns the ticket belonging to the specified user.
        /// </summary>
        /// <param name="ticketProvider">Service provider, necessary to retrieve the IHttpContextAccessor</param>
        /// <returns></returns>
        public static Ticket GetTicket(ITicketProvider ticketProvider)
        {
            return new Ticket(ticketProvider.Username, ticketProvider.Roles);
        }
    }
}
