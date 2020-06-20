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
    /// Imports a private PGP key ring, which often is a master key, in addition to its sub keys,
    /// and the public key for the master key. Typically returns 3 keys in total.
    /// </summary>
    [Slot(Name = "pgp.keys.private.import")]
    public class PgpKeysPrivateImport : ISlot
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
                throw new ArgumentNullException("No value provided to [pgp.keys.private.import]");
            var lambda = input.Children.FirstOrDefault(x => x.Name == ".lambda") ??
                throw new ArgumentNullException("No [.lambda] provided to [pgp.keys.private.import]");

            // Unwrapping key(s) and iterating through them, importing them one at the time.
            using (var memStream = new MemoryStream(Encoding.UTF8.GetBytes(keyPlainText)))
            {
                using (var armored = new ArmoredInputStream(memStream))
                {
                    var key = new PgpSecretKeyRing(armored);
                    foreach (PgpSecretKey idxKey in key.GetSecretKeys())
                    {
                        InvokeLambda(signaler, lambda, idxKey);
                    }

                    // Then doing public key for master key in secret key chain.
                    var publicKey = key.GetPublicKey();
                    if (publicKey != null)
                        PgpKeysPublicImport.InvokeLambda(signaler, lambda, publicKey);
                }
            }
        }

        #region [ -- Private and internal helper methods -- ]

        internal static void InvokeLambda(
            ISignaler signaler,
            Node lambda,
            PgpSecretKey idxKey)
        {
            // Parametrizing [.lambda] callback with key and data.
            var keyNode = new Node(".key");
            keyNode.Add(new Node("private", true));
            keyNode.Add(new Node("fingerprint", PgpHelpers.GetFingerprint(idxKey.PublicKey)));
            keyNode.Add(new Node("id", idxKey.KeyId));
            keyNode.Add(new Node("content", PgpHelpers.GetAsciiArmoredSecretKey(idxKey)));
            keyNode.Add(new Node("is-master", idxKey.IsMasterKey));
            keyNode.Add(new Node("is-signing-key", idxKey.IsSigningKey));
            keyNode.Add(new Node("encryption-algorithm", idxKey.KeyEncryptionAlgorithm.ToString()));

            // Adding ID for key.
            var ids = new Node("ids");
            foreach (var idxId in idxKey.UserIds)
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
