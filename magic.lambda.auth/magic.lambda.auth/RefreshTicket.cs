/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using Microsoft.Extensions.Configuration;
using magic.node;
using magic.signals.contracts;
using magic.lambda.auth.helpers;
using magic.lambda.auth.contracts;

namespace magic.lambda.auth
{
    /// <summary>
    /// [auth.ticket.refresh] slot refreshing an existing ticket, resulting in a new ticket,
    /// with a postponed expiration time, to avoid having users having to login every time their
    /// token expires.
    /// </summary>
    [Slot(Name = "auth.ticket.refresh")]
    public class RefreshTicket : ISlot
    {
        readonly ITicketProvider _ticketProvider;
        readonly IConfiguration _configuration;

        /// <summary>
        /// Creates a new instance of the class.
        /// </summary>
        /// <param name="configuration">Configuration for your application.</param>
        /// <param name="ticketProvider">Ticket provider, necessary to retrieve the authenticated user.</param>
        public RefreshTicket(IConfiguration configuration, ITicketProvider ticketProvider)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _ticketProvider = ticketProvider ?? throw new ArgumentNullException(nameof(ticketProvider));
        }

        /// <summary>
        /// Implementation for your slot.
        /// </summary>
        /// <param name="signaler">Signaler used to raise the signal.</param>
        /// <param name="input">Arguments to your slot.</param>
        public void Signal(ISignaler signaler, Node input)
        {
            // This will throw is ticket is expired, doesn't exist, etc.
            TicketFactory.VerifyTicket(_ticketProvider, null);

            // Retrieving old ticket and using its data to create a new ticket.
            input.Value = TicketFactory.CreateTicket(_configuration, TicketFactory.GetTicket(_ticketProvider));
        }
    }
}
