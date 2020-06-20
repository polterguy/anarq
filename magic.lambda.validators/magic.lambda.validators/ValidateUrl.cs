/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using magic.node;
using magic.node.extensions;
using magic.signals.contracts;

namespace magic.lambda.validators
{
    /// <summary>
    /// [validators.url] slot, for verifying that some input is a valid URL.
    /// </summary>
    [Slot(Name = "validators.url")]
    public class ValidateUrl : ISlot
    {
        /// <summary>
        /// Implementation of slot.
        /// </summary>
        /// <param name="signaler">Signaler used to raise the signal.</param>
        /// <param name="input">Arguments to signal.</param>
        public void Signal(ISignaler signaler, Node input)
        {
            var url = input.GetEx<string>();
            bool result = Uri.TryCreate(url, UriKind.Absolute, out Uri res);
            input.Value = null;
            input.Clear();
            if (!result || (res.Scheme != Uri.UriSchemeHttp && res.Scheme != Uri.UriSchemeHttps))
                throw new ArgumentException($"'{url}' is not a valid URL, it needs to be prepended with http:// or https:// and also be a valid URL");
        }
    }
}
