/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.Threading.Tasks;
using MimeKit;

namespace magic.lambda.mime.contracts
{
    /// <summary>
    /// Abstract POP3 interface dependency injected into POP3 fetcher class.
    /// </summary>
    public interface IMailClient : IDisposable
    {
        /// <summary>
        /// Connects to an SMTP server
        /// </summary>
        /// <param name="host">URL or IP address of your server.</param>
        /// <param name="port">Port to use for connection.</param>
        /// <param name="useSsl">If true, will use SSL/TLS to connect.</param>
        void Connect(string host, int port, bool useSsl);

        /// <summary>
        /// Connects asynchronously to an SMTP server
        /// </summary>
        /// <param name="host">URL or IP address of your server.</param>
        /// <param name="port">Port to use for connection.</param>
        /// <param name="useSsl">If true, will use SSL/TLS to connect.</param>
        /// <returns>Awaitable task</returns>
        Task ConnectAsync(string host, int port, bool useSsl);

        /// <summary>
        /// Authenticates you to an already connected SMTP server.
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        void Authenticate(string username, string password);

        /// <summary>
        /// Authenticates you to an already connected SMTP server.
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <returns>Awaitable task</returns>
        Task AuthenticateAsync(string username, string password);

        /// <summary>
        /// Disconnects from an already connected SMTP server.
        /// </summary>
        /// <param name="quit">Whether or not to send the QUIT signal.</param>
        void Disconnect(bool quit);

        /// <summary>
        /// Disconnects from an already connected SMTP server.
        /// </summary>
        /// <param name="quit">Whether or not to send the QUIT signal.</param>
        /// <returns>Awaitable task</returns>
        Task DisconnectAsync(bool quit);
    }
}
