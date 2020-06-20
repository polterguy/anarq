/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.IO;
using System.Text;
using System.Linq;
using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Bcpg.OpenPgp;
using magic.node;
using magic.node.extensions;
using magic.signals.contracts;
using magic.lambda.mime.helpers;

namespace magic.lambda.mime
{
    /// <summary>
    /// Imports a public PGP key ring, which often is a master key, in addition to its sub keys.
    /// </summary>
    [Slot(Name = "pgp.keys.public.import")]
    public class PgpKeysPublicImport : ISlot
    {
        /// <summary>
        /// Implementation of your slot.
        /// </summary>
        /// <param name="signaler">Signaler that raised the signal.</param>
        /// <param name="input">Arguments to your slot.</param>
        public void Signal(ISignaler signaler, Node input)
        {
            // Sanity checking invocation.
            var keyPlainText = input.GetEx<string>() ?? 
                throw new ArgumentNullException("No value provided to [pgp.keys.public.import]");
            var lambda = input.Children.FirstOrDefault(x => x.Name == ".lambda") ??
                throw new ArgumentNullException("No [.lambda] provided to [pgp.keys.public.import]");

            // Unwrapping key(s) and iterating through them, importing them one at the time.
            using (var memStream = new MemoryStream(Encoding.UTF8.GetBytes(keyPlainText)))
            {
                using (var armored = new ArmoredInputStream(memStream))
                {
                    var key = new PgpPublicKeyRing(armored);
                    foreach (PgpPublicKey idxKey in key.GetPublicKeys())
                    {
                        InvokeLambda(signaler, lambda, idxKey);
                    }
                }
            }
        }

        #region [ -- Private and internal herlper methods -- ]

        internal static void InvokeLambda(
            ISignaler signaler,
            Node lambda,
            PgpPublicKey key)
        {
            // Parametrizing [.lambda] callback with key and data.
            var keyNode = new Node(".key");
            keyNode.Add(new Node("private", false));
            keyNode.Add(new Node("fingerprint", PgpHelpers.GetFingerprint(key)));
            keyNode.Add(new Node("id", key.KeyId));
            keyNode.Add(new Node("content", PgpHelpers.GetAsciiArmoredPublicKey(key)));
            keyNode.Add(new Node("created", key.CreationTime));
            keyNode.Add(new Node("valid-seconds", key.GetValidSeconds()));
            keyNode.Add(new Node("algorithm", key.Algorithm.ToString()));
            keyNode.Add(new Node("bit-strength", key.BitStrength));
            keyNode.Add(new Node("is-encryption", key.IsEncryptionKey));
            keyNode.Add(new Node("is-master", key.IsMasterKey));
            keyNode.Add(new Node("is-revoked", key.IsRevoked()));

            // Adding ID for key.
            var ids = new Node("ids");
            foreach (var idxId in key.GetUserIds())
            {
                ids.Add(new Node(".", idxId.ToString()));
            }
            if (ids.Children.Any())
                keyNode.Add(ids);

            // Invoking [.lambda] making sure we reset it after evaluation.
            var exe = lambda.Clone();
            lambda.Insert(0, keyNode);
            signaler.Signal("eval", lambda);
            lambda.Clear();
            lambda.AddRange(exe.Children.ToList());
        }

        #endregion
    }
}
