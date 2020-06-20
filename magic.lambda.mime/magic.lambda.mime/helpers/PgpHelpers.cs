/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System.IO;
using System.Text;
using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Bcpg.OpenPgp;

namespace magic.lambda.mime.helpers
{
    /// <summary>
    /// Helper class for PGP parts of Magic and its MIME helpers.
    /// </summary>
    public static class PgpHelpers
    {
        /// <summary>
        /// Returns the fingerprint for specified public PGP key.
        /// </summary>
        /// <param name="key">Key to retrieve fingerprint of.</param>
        /// <returns>Fingerprint of specified public key.</returns>
        public static string GetFingerprint(PgpPublicKey key)
        {
            var builder = new StringBuilder();
            var data = key.GetFingerprint();
            for (int idx = 0; idx < data.Length; idx++)
            {
                builder.Append(data[idx].ToString("x2"));
            }
            return builder.ToString().ToUpperInvariant();
        }

        /// <summary>
        /// Returns actual public PGP key, in armored ASCII format.
        /// </summary>
        /// <param name="key">Key to retrieve.</param>
        /// <returns>Armored ASCII format representing public key</returns>
        public static string GetAsciiArmoredPublicKey(PgpPublicKey key)
        {
            using (var memStream = new MemoryStream())
            {
                using (var armored = new ArmoredOutputStream(memStream))
                {
                    key.Encode(armored);
                    armored.Flush();
                }
                memStream.Flush();
                memStream.Position = 0;
                using (var sr = new StreamReader(memStream))
                {
                    return sr.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// Returns actual secret key, in armored ASCII format.
        /// </summary>
        /// <param name="key">Key to retrieve.</param>
        /// <returns>Armored ASCII format representing private key.</returns>
        public static string GetAsciiArmoredSecretKey(PgpSecretKey key)
        {
            using (var memStream = new MemoryStream())
            {
                using (var armored = new ArmoredOutputStream(memStream))
                {
                    key.Encode(armored);
                    armored.Flush();
                }
                memStream.Flush();
                memStream.Position = 0;
                using (var sr = new StreamReader(memStream))
                {
                    return sr.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// Returns a PGP secret key from the given ASCII armored secret key string content.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static PgpSecretKey GetSecretKeyFromAsciiArmored(string key)
        {
            using (var memStream = new MemoryStream(Encoding.UTF8.GetBytes(key)))
            {
                using (var armored = new ArmoredInputStream(memStream))
                {
                    var result = new PgpSecretKeyRing(armored);
                    return result.GetSecretKey();
                }
            }
        }

        /// <summary>
        /// Returns a PGP secret key ring from the given ASCII armored secret key string content.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static PgpSecretKeyRing GetSecretKeyRingFromAsciiArmored(string key)
        {
            using (var memStream = new MemoryStream(Encoding.UTF8.GetBytes(key)))
            {
                using (var armored = new ArmoredInputStream(memStream))
                {
                    return new PgpSecretKeyRing(armored);
                }
            }
        }

        public static PgpPublicKey GetPublicKeyFromAsciiArmored(string key)
        {
            using (var memStream = new MemoryStream(Encoding.UTF8.GetBytes(key)))
            {
                using (var armored = new ArmoredInputStream(memStream))
                {
                    var result = new PgpPublicKeyRing(armored);
                    return result.GetPublicKey();
                }
            }
        }
    }
}
