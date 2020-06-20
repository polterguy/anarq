/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.Linq;
using magic.node;
using magic.signals.contracts;
using magic.lambda.auth.helpers;
using magic.lambda.auth.contracts;

namespace magic.lambda.auth
{
    /// <summary>
    /// [auth.ticket.get] slot for getting the username and roles claim(s) for currently logged in user.
    /// </summary>
    [Slot(Name = "auth.ticket.get")]
    public class GetTicket : ISlot
    {
        readonly ITicketProvider _ticketProvider;

        /// <summary>
        /// Creates a new instance of the class.
        /// </summary>
        /// <param name="ticketProvider">Your ticket provider.</param>
        public GetTicket(ITicketProvider ticketProvider)
        {
            _ticketProvider = ticketProvider ?? throw new ArgumentNullException(nameof(ticketProvider));
        }

        /// <summary>
        /// Implementation for the slots.
        /// </summary>
        /// <param name="signaler">Signaler used to raise the signal.</param>
        /// <param name="input">Arguments to your signal.</param>
        public void Signal(ISignaler signaler, Node input)
        {
            var ticket = TicketFactory.GetTicket(_ticketProvider);
            input.Value = ticket.Username;
            input.AddRange(ticket.Roles.Select(x => new Node("", x)));
        }
    }
}
