/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using MimeKit;
using MimeKit.IO;
using MimeKit.Cryptography;
using Org.BouncyCastle.Bcpg.OpenPgp;
using magic.node;
using magic.node.extensions;
using magic.signals.contracts;

namespace magic.lambda.mime.helpers
{
    /// <summary>
    /// Helper class to create MIME messages.
    /// </summary>
    public static class MimeCreator
    {
        /// <summary>
        /// Creates a MimeEntity given the structured input node, and returns MimeEntity to caller.
        /// </summary>
        /// <param name="signaler">Signaler used to construct message.</param>
        /// <param name="input">Hierarchical node structure representing the MIME message in lambda format.</param>
        /// <returns></returns>
        public static MimeEntity Create(ISignaler signaler, Node input)
        {
            var messageNodes = input.Children.Where(x => x.Name == "entity");
            if (messageNodes.Count() != 1)
                throw new ArgumentException("Too many [entity] nodes found for slot to handle.");
            return CreateEntity(signaler, messageNodes.First());
        }

        /// <summary>
        /// Helper method to dispose a MimeEntity's streams.
        /// </summary>
        /// <param name="entity">Entity to iterate over to dispose all associated streams.</param>
        public static void Dispose(MimeEntity entity)
        {
            if (entity is MimePart part)
            {
                part.Content?.Stream?.Dispose();
            }
            else if (entity is Multipart multi)
            {
                foreach (var idx in multi)
                {
                    Dispose(idx);
                }
            }
        }

        #region [ -- Private helper methods -- ]

        /*
         * Create MimeEntity, or MIME part to be specific.
         */
        static MimeEntity CreateEntity(ISignaler signaler, Node input)
        {
            MimeEntity result = null;

            // Finding Content-Type of entity.
            var type = input.GetEx<string>();
            if (!type.Contains("/"))
                throw new ArgumentException($"'{type}' is an unknown MIME Content-Type. Please provide valid Content-Type as value of node.");
            var tokens = type.Split('/');
            if (tokens.Length != 2)
                throw new ArgumentException($"'{type}' is an unknown MIME Content-Type. Please provide valid Content-Type as value of node.");
            var mainType = tokens[0];
            var subType = tokens[1];
            switch (mainType)
            {
                case "text":
                    result = CreateLeafPart(signaler, mainType, subType, input);
                    break;
                case "multipart":
                    result = CreateMultipart(signaler, subType, input);
                    break;
            }

            // Retrieving cryptographic parameters, assuming if specified, the entity should be encrypted, signed, or both.
            var encryptionKey = input.Children
                .FirstOrDefault(x => x.Name == "encrypt");
            var signingKey = input.Children
                .FirstOrDefault(x => x.Name == "sign")?
                .GetEx<string>();
            var signingKeyPassword = input.Children
                .FirstOrDefault(x => x.Name == "sign")?
                .Children
                .FirstOrDefault(x => x.Name == "password")?
                .GetEx<string>();

            // Checking if entity should be encrypted, cryptographically signed, or both.
            if (encryptionKey != null && !string.IsNullOrEmpty(signingKey))
            {
                // Cryptographically signing entity, AND encrypting it.
                result = SignAndEncrypt(result, encryptionKey, signingKey, signingKeyPassword);
            }
            else if (encryptionKey != null)
            {
                // Only encrypting entity.
                result = Encrypt(result, encryptionKey);
            }
            else if (!string.IsNullOrEmpty(signingKey))
            {
                // Only cryptographically signing entity.
                result = Sign(result, signingKey, signingKeyPassword);
            }
            return result;
        }

        /*
         * Creates a leaf part, implying no MimePart children.
         */
        static MimePart CreateLeafPart(
            ISignaler signaler,
            string mainType,
            string subType,
            Node messageNode)
        {
            // Retrieving [content] node.
            var contentNode = messageNode.Children.FirstOrDefault(x => x.Name == "content") ??
                messageNode.Children.FirstOrDefault(x => x.Name == "filename") ??
                throw new ArgumentNullException("No [content] or [filename] provided in [entity]");

            var result = new MimePart(ContentType.Parse(mainType + "/" + subType));
            DecorateEntityHeaders(result, messageNode);
            switch (contentNode.Name)
            {
                case "content":
                    CreateContentObjectFromObject(contentNode, result);
                    break;
                case "filename":
                    CreateContentObjectFromFilename(signaler, contentNode, result);
                    break;
            }
            return result;
        }

        /*
         * Creates a multipart of some sort.
         */
        static Multipart CreateMultipart(
            ISignaler signaler,
            string subType,
            Node messageNode)
        {
            var result = new Multipart(subType);
            DecorateEntityHeaders(result, messageNode);
            foreach (var idxPart in messageNode.Children.Where(x => x.Name == "entity"))
            {
                result.Add(CreateEntity(signaler, idxPart));
            }
            return result;
        }

        /*
         * Creates ContentObject from value found in node.
         */
        static void CreateContentObjectFromObject(Node contentNode, MimePart part)
        {
            var stream = new MemoryBlockStream();
            var content = contentNode.GetEx<string>() ??
                throw new ArgumentNullException("No actual [content] supplied to message");
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;

            var encoding = ContentEncoding.Default;
            var encodingNode = contentNode.Children.FirstOrDefault(x => x.Name == "Content-Encoding");
            if (encodingNode != null)
                encoding = (ContentEncoding)Enum.Parse(typeof(ContentEncoding), encodingNode.GetEx<string>());
            part.Content = new MimeContent(stream, encoding);
        }

        /*
         * Creates ContentObject from filename.
         */
        static void CreateContentObjectFromFilename(
            ISignaler signaler,
            Node contentNode,
            MimePart part)
        {
            var filename = contentNode.GetEx<string>() ?? throw new ArgumentNullException("No [filename] value provided");

            // Checking if explicit encoding was supplied.
            ContentEncoding encoding = ContentEncoding.Default;
            var encodingNode = contentNode.Children.FirstOrDefault(x => x.Name == "Content-Encoding");
            if (encodingNode != null)
                encoding = (ContentEncoding)Enum.Parse(typeof(ContentEncoding), encodingNode.GetEx<string>());

            // Checking if explicit disposition was specified.
            if (part.ContentDisposition == null)
            {
                // Defaulting Content-Disposition to; "attachment; filename=whatever.xyz"
                part.ContentDisposition = new ContentDisposition("attachment")
                {
                    FileName = Path.GetFileName(filename)
                };
            }
            var rootPath = new Node();
            signaler.Signal(".io.folder.root", rootPath);
            part.Content = new MimeContent(
                File.OpenRead(
                    rootPath.GetEx<string>() +
                    filename.TrimStart('/')),
                encoding);
        }

        /*
         * Decorates MimeEntity with headers specified in Node children collection.
         */
        static void DecorateEntityHeaders(MimeEntity entity, Node messageNode)
        {
            var headerNode = messageNode.Children.FirstOrDefault(x => x.Name == "headers");
            if (headerNode == null)
                return; // No headers
            foreach (var idx in headerNode.Children.Where(ix => ix.Name != "Content-Type" && ix.Name != "content"))
            {
                entity.Headers.Replace(idx.Name, idx.GetEx<string>());
            }
        }

        /*
         * Encrypts an entity.
         */
        static MultipartEncrypted Encrypt(MimeEntity entity, Node encryptionNode)
        {
            return MultipartEncrypted.Encrypt(GetEncryptionKeys(encryptionNode), entity);
        }

        /*
         * Cryptographically signs an entity.
         */
        static MultipartSigned Sign(
            MimeEntity entity,
            string armoredPrivateKey,
            string keyPassword)
        {
            var algo = DigestAlgorithm.Sha256;
            using (var ctx = new CreatePgpMimeContext { Password = keyPassword })
            {
                return MultipartSigned.Create(
                    ctx,
                    PgpHelpers.GetSecretKeyFromAsciiArmored(armoredPrivateKey),
                    algo,
                    entity);
            }
        }

        /*
         * Cryptographically signs and encrypts an entity.
         */
        static MultipartEncrypted SignAndEncrypt(
            MimeEntity entity,
            Node encryptionNode,
            string armoredPrivateKey,
            string keyPassword)
        {
            var algo = DigestAlgorithm.Sha256;
            using (var ctx = new CreatePgpMimeContext { Password = keyPassword })
            {
                return MultipartEncrypted.SignAndEncrypt(
                    ctx,
                    PgpHelpers.GetSecretKeyFromAsciiArmored(armoredPrivateKey),
                    algo,
                    GetEncryptionKeys(encryptionNode),
                    entity);
            }
        }

        /*
         * Returns all public keys referenced in lambda object, somehow.
         * Values can exist either as value of node, and/or valuesof children of node given.
         */
        static IEnumerable<PgpPublicKey> GetEncryptionKeys(Node encryptionKey)
        {
            // Returning any public encryption key found in value of node first.
            if (encryptionKey.Value != null)
            {
                var result = PgpHelpers.GetPublicKeyFromAsciiArmored(encryptionKey.GetEx<string>());

                // Sanity checking key, before returning to caller.
                SanityCheckCryptographyKey(result);
                yield return result;
            }

            // Looping through children, in case caller provided a collection of encryption keys, that should all be used.
            foreach (var idx in encryptionKey.Children)
            {
                var result = PgpHelpers.GetPublicKeyFromAsciiArmored(idx.GetEx<string>());

                // Sanity checking key, before returning to caller.
                SanityCheckCryptographyKey(result);
                yield return result;
            }
        }

        /*
         * Sanity checks public PGP key, to make sure it's valid for encrypting MIME entities.
         */
        static void SanityCheckCryptographyKey(PgpPublicKey key)
        {
            if (!key.IsEncryptionKey)
                throw new ArgumentException($"Key with fingerprint of '{PgpHelpers.GetFingerprint(key)}' is not an encryption key");
            if (key.IsRevoked())
                throw new ArgumentException($"Key with fingerprint of '{PgpHelpers.GetFingerprint(key)}' is revoked");
        }

        #endregion
    }
}
