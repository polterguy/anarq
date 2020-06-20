/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using MimeKit;
using magic.node;
using magic.node.extensions;
using magic.signals.contracts;
using magic.lambda.mail.helpers;
using magic.lambda.mime.helpers;
using contracts = magic.lambda.mime.contracts;

namespace magic.lambda.mail
{
    /// <summary>
    /// Sends email messages through an SMTP server.
    /// </summary>
    [Slot(Name = "mail.smtp.send")]
    [Slot(Name = "wait.mail.smtp.send")]
    public class MailSmtpSend : ISlotAsync, ISlot
    {
        readonly IConfiguration _configuration;
        readonly contracts.ISmtpClient _client;

        /// <summary>
        /// Constructor for your SMTP slot class.
        /// </summary>
        /// <param name="configuration">IConfiguration dependency provided argument.</param>
        /// <param name="client">SMTP client implementation</param>
        public MailSmtpSend(IConfiguration configuration, contracts.ISmtpClient client)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        /// <summary>
        /// Implementation of your slot.
        /// </summary>
        /// <param name="signaler">Signaler that raised the signal.</param>
        /// <param name="input">Arguments to your slot.</param>
        public void Signal(ISignaler signaler, Node input)
        {
            // Retrieving server connection settings.
            var settings = new ConnectionSettings(
                _configuration,
                input.Children.FirstOrDefault(x => x.Name == "server"),
                "smtp");

            // Connecting and authenticating (unless username is null).
            _client.Connect(settings.Server, settings.Port, settings.Secure);
            try
            {
                // Checking if we're supposed to authenticate
                if (settings.Username != null || settings.Password != null)
                    _client.Authenticate(settings.Username, settings.Password);

                // Iterating through each message and sending.
                foreach (var idxMsgNode in input.Children.Where(x => x.Name == "message"))
                {
                    // Creating MimeMessage, making sure we dispose any streams created in the process.
                    var message = CreateMessage(signaler, idxMsgNode);
                    try
                    {
                        // Sending message over existing SMTP connection.
                        _client.Send(message);
                    }
                    finally
                    {
                        MimeCreator.Dispose(message.Body);
                    }
                }
            }
            finally
            {
                // Disconnecting and sending QUIT signal to SMTP server.
                _client.Disconnect(true);
            }
        }

        /// <summary>
        /// Implementation of your slot.
        /// </summary>
        /// <param name="signaler">Signaler that raised the signal.</param>
        /// <param name="input">Arguments to your slot.</param>
        public async Task SignalAsync(ISignaler signaler, Node input)
        {
            // Retrieving server connection settings.
            var settings = new ConnectionSettings(
                _configuration,
                input.Children.FirstOrDefault(x => x.Name == "server"),
                "smtp");

            // Connecting and authenticating (unless username is null).
            await _client.ConnectAsync(settings.Server, settings.Port, settings.Secure);
            try
            {
                // Checking if we're supposed to authenticate
                if (settings.Username != null || settings.Password != null)
                    await _client.AuthenticateAsync(settings.Username, settings.Password);

                // Iterating through each message and sending.
                foreach (var idxMsgNode in input.Children.Where(x => x.Name == "message"))
                {
                    // Creating MimeMessage, making sure we dispose any streams created in the process.
                    var message = CreateMessage(signaler, idxMsgNode);
                    try
                    {
                        // Sending message over existing SMTP connection.
                        await _client.SendAsync(message);
                    }
                    finally
                    {
                        MimeCreator.Dispose(message.Body);
                    }
                }
            }
            finally
            {
                // Disconnecting and sending QUIT signal to SMTP server.
                await _client.DisconnectAsync(true);
            }
        }

        #region [ -- Private helpers -- ]

        /*
         * Creates a MimeMessage according to given node, and returns to caller.
         */
        MimeMessage CreateMessage(ISignaler signaler, Node node)
        {
            // Creating message.
            var message = new MimeMessage
            {
                // Decorating MimeMessage with subject.
                Subject = node.Children
                    .FirstOrDefault(x => x.Name == "subject")?
                    .GetEx<string>() ??
                    "" // Defaulting to empty string as subject.
            };

            // Decorating MimeMessage with from, to, cc, and bcc.
            message.To.AddRange(GetAddresses(node.Children.FirstOrDefault(x => x.Name == "to"), true));
            message.From.AddRange(GetAddresses(node.Children.FirstOrDefault(x => x.Name == "from"), true, "magic:smtp:"));
            message.Cc.AddRange(GetAddresses(node.Children.FirstOrDefault(x => x.Name == "cc")));
            message.Bcc.AddRange(GetAddresses(node.Children.FirstOrDefault(x => x.Name == "bcc")));

            // Creating actual MimeEntity to send.
            var clone = node.Clone();
            signaler.Signal(".mime.create", clone);
            var entity = clone.Value as MimeEntity;
            message.Body = entity;

            // Returning message (and streams) to caller.
            return message;
        }

        /*
         * Returns a bunch of email addresses by iterating the children of the specified node,
         * and transforming each into a valid MailboxAddress.
         */
        IEnumerable<MailboxAddress> GetAddresses(
            Node iterator,
            bool throwOnEmpty = false,
            string configPrefix = null)
        {
            if (throwOnEmpty && (iterator == null || !iterator.Children.Any()))
            {
                if (!string.IsNullOrEmpty(configPrefix))
                {
                    var fromName = _configuration[configPrefix + "from:name"];
                    var fromAddress = _configuration[configPrefix + "from:address"];
                    if (!string.IsNullOrEmpty(fromName) && !string.IsNullOrEmpty(fromAddress))
                    {
                        yield return new MailboxAddress(fromName, fromAddress);
                        yield break;
                    }
                }
                throw new ArgumentNullException("Missing mandatory address field");
            }
            if (iterator == null)
                yield break;
            foreach (var idx in iterator.Children)
            {
                yield return new MailboxAddress(idx.Name, idx.GetEx<string>());
            }
        }

        #endregion
    }
}
