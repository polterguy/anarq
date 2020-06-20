/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using MimeKit;
using magic.node;
using magic.node.extensions;
using magic.signals.contracts;
using magic.lambda.mail.helpers;
using contracts = magic.lambda.mime.contracts;

namespace magic.lambda.mail
{
    /// <summary>
    /// Fetches all new messages from the specified POP3 account.
    /// </summary>
    [Slot(Name = "mail.pop3.fetch")]
    [Slot(Name = "wait.mail.pop3.fetch")]
    public class MailPop3Fetch : ISlotAsync, ISlot
    {
        readonly IConfiguration _configuration;
        readonly contracts.IPop3Client _client;
        readonly Func<int, int, int, bool> Done = (idx, count, max) => idx < count && (max == -1 || count < max);

        /// <summary>
        /// Constructor for your class.
        /// </summary>
        /// <param name="configuration">IConfiguration dependency provided argument.</param>
        /// <param name="client">POP3 client implementation</param>
        public MailPop3Fetch(IConfiguration configuration, contracts.IPop3Client client)
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
            var settings = new Pop3Settings(input, _configuration);
            _client.Connect(settings.Connection.Server, settings.Connection.Port, settings.Connection.Secure);
            try
            {
                if (settings.Connection.HasCredentials)
                    _client.Authenticate(settings.Connection.Username, settings.Connection.Password);

                var count = _client.GetMessageCount();
                for (var idx = 0; Done(idx, count, settings.Max); idx++)
                {
                    var message = _client.GetMessage(idx);
                    HandleMessage(message, signaler, settings.Lambda, settings.Raw);
                    signaler.Signal("eval", settings.Lambda);

                    // Cleaning up [.lambda] object by removing [.message].
                    settings.Lambda.Children
                        .FirstOrDefault(x => x.Name == ".message")?.UnTie();
                }
            }
            finally
            {
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
            var settings = new Pop3Settings(input, _configuration);
            await _client.ConnectAsync(settings.Connection.Server, settings.Connection.Port, settings.Connection.Secure);
            try
            {
                if (settings.Connection.HasCredentials)
                    await _client.AuthenticateAsync(settings.Connection.Username, settings.Connection.Password);

                var count = await _client.GetMessageCountAsync();
                for (var idx = 0; Done(idx, count, settings.Max); idx++)
                {
                    var message = await _client.GetMessageAsync(idx);
                    HandleMessage(message, signaler, settings.Lambda, settings.Raw);
                    await signaler.SignalAsync("wait.eval", settings.Lambda);

                    // Cleaning up [.lambda] object by removing [.message].
                    settings.Lambda.Children
                        .FirstOrDefault(x => x.Name == ".message")?.UnTie();
                }
            }
            finally
            {
                await _client.DisconnectAsync(true);
            }
        }

        #region [ -- Private helper methods and classes -- ]

        /*
         * Helper class to encapsulate POP3 settings, such as connection settings, and other
         * types of configurations, such as how many messages to retrieve, etc.
         */
        class Pop3Settings
        {
            public Pop3Settings(Node input, IConfiguration configuration)
            {
                Connection = new ConnectionSettings(
                    configuration,
                    input.Children.FirstOrDefault(x => x.Name == "server"),
                    "pop3");

                Max = input.Children.SingleOrDefault(x => x.Name == "max")?.GetEx<int>() ?? 50;
                Raw = input.Children.SingleOrDefault(x => x.Name == "raw")?.GetEx<bool>() ?? false;
                Lambda = input.Children.FirstOrDefault(x => x.Name == ".lambda") ??
                    throw new ArgumentNullException("No [.lambda] provided to [wait.mail.pop3.fetch]");
            }

            public ConnectionSettings Connection { get; private set; }

            public int Max { get; private set; }

            public bool Raw { get; private set; }

            public Node Lambda { get; private set; }
        }

        /*
         * Helper method to handle one single message, by parsing it (unless raw is true), and invoking [.lambda]
         * callback to notify client of message retrieved.
         */
        void HandleMessage(
            MimeMessage message,
            ISignaler signaler,
            Node lambda,
            bool raw)
        {
            var messageNode = new Node(".message");
            lambda.Insert(0, messageNode);

            if (raw)
            {
                messageNode.Value = message.ToString();
            }
            else
            {
                messageNode.Add(new Node("subject", message.Subject));
                AddRecipient(message.From.Select(x => x as MailboxAddress), messageNode, "from");
                AddRecipient(message.To.Select(x => x as MailboxAddress), messageNode, "to");
                AddRecipient(message.Cc.Select(x => x as MailboxAddress), messageNode, "cc");
                AddRecipient(message.Bcc.Select(x => x as MailboxAddress), messageNode, "bcc");

                var parseNode = new Node("", message.Body);
                signaler.Signal(".mime.parse", parseNode);
                messageNode.AddRange(parseNode.Children);
            }
        }

        /*
         * Helper method to handle a specific type of recipient, and creating a lambda list of nodes,
         * wrapping recipient's email address.
         */
        void AddRecipient(IEnumerable<MailboxAddress> items, Node node, string nodeName)
        {
            if (items == null || !items.Any())
                return;
            var collectionNode = new Node(nodeName);
            foreach (var idx in items)
            {
                if (idx == null)
                    continue; // Might be other types of addresses in theory ...
                collectionNode.Add(new Node(idx.Name, idx.Address));
            }
            node.Add(collectionNode);
        }

        #endregion
    }
}
