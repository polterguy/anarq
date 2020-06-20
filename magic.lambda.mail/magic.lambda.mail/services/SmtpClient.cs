/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.Threading.Tasks;
using MimeKit;
using mk = MailKit.Net.Smtp;
using contract = magic.lambda.mime.contracts;

namespace magic.lambda.mime.services
{
    /// <summary>
    /// Default service implementation class for ISmtpClient.
    /// 
    /// Notice, class is simply an adapter towards MailKit's SmtpClient.
    /// </summary>
    public sealed class SmtpClient : contract.ISmtpClient
    {
        readonly Lazy<mk.SmtpClient> _client = new Lazy<mk.SmtpClient>(() => new mk.SmtpClient());

        public void Authenticate(string username, string password)
        {
            _client.Value.Authenticate(username, password);
        }

        public async Task AuthenticateAsync(string username, string password)
        {
            await _client.Value.AuthenticateAsync(username, password);
        }

        public void Connect(string host, int port, bool useSsl)
        {
            _client.Value.Connect(host, port, useSsl);
        }

        public async Task ConnectAsync(string host, int port, bool useSsl)
        {
            await _client.Value.ConnectAsync(host, port, useSsl);
        }

        public void Disconnect(bool quit)
        {
            _client.Value.Disconnect(quit);
        }

        public async Task DisconnectAsync(bool quit)
        {
            await _client.Value.DisconnectAsync(quit);
        }

        public void Dispose()
        {
            _client.Value?.Dispose();
        }

        public void Send(MimeMessage message)
        {
            _client.Value.Send(message);
        }

        public async Task SendAsync(MimeMessage message)
        {
            await _client.Value.SendAsync(message);
        }
    }
}
