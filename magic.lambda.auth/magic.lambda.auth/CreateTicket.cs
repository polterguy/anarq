/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using magic.node;
using magic.node.extensions;
using magic.signals.contracts;
using magic.lambda.auth.helpers;

namespace magic.lambda.auth
{
    /// <summary>
    /// [auth.ticket.create] slot for creating a new JWT token.
    /// </summary>
    [Slot(Name = "auth.ticket.create")]
    public class CreateTicket : ISlot
    {
        readonly IConfiguration _configuration;

        /// <summary>
        /// Creates a new instance of the class.
        /// </summary>
        /// <param name="configuration">Configuration for application.</param>
        public CreateTicket(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Implementation for the slots.
        /// </summary>
        /// <param name="signaler">Signaler used to raise the signal.</param>
        /// <param name="input">Arguments to your signal.</param>
        public void Signal(ISignaler signaler, Node input)
        {
            if (input.Value != null)
                throw new ArgumentException($"[auth.ticket.create] don't know how to handle parameters in its value.");

            if (input.Children.Any(x => x.Name != "username" && x.Name != "roles"))
                throw new ApplicationException("[auth.ticket.create] can only handle [username] and [roles] children nodes");

            var usernameNode = input.Children.Where(x => x.Name == "username");
            var rolesNode = input.Children.Where(x => x.Name == "roles");

            if (usernameNode.Count() != 1)
                throw new ApplicationException("[auth.ticket.create] must be given a [username] argument at the minimum");

            var username = usernameNode.First().GetEx<string>();
            var roles = rolesNode.FirstOrDefault()?.Children.Select(x => x.GetEx<string>());

            input.Clear();
            input.Value = TicketFactory.CreateTicket(_configuration, new Ticket(username, roles));
        }
    }
}
