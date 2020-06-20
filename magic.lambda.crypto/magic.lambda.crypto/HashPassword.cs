/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using bc = BCrypt.Net;
using magic.node;
using magic.node.extensions;
using magic.signals.contracts;

namespace magic.lambda.crypto
{
    /// <summary>
    /// [crypto.password.hash] slot to create a cryptographically secure hashed version of a password,
    /// for storing securely into a database. The slot uses bcrypt or BlowFish hashing, with a per-user based
    /// salt, making it highly secure for storing passwords hashed into a database, preventing Rainbow Dictionary
    /// attacks.
    /// </summary>
    [Slot(Name = "crypto.password.hash")]
    public class HashPassword : ISlot
    {
        /// <summary>
        /// Implementation of slot.
        /// </summary>
        /// <param name="signaler">Signaler invoking slot.</param>
        /// <param name="input">Arguments to slot.</param>
        public void Signal(ISignaler signaler, Node input)
        {
            input.Value = bc.BCrypt.HashPassword(input.GetEx<string>());
        }
    }
}
