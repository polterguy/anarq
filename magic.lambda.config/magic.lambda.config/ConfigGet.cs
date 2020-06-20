/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using magic.node;
using magic.node.extensions;
using magic.signals.contracts;

namespace magic.lambda.config
{
    /// <summary>
    /// [config] slot for retrieving configuration settings for your application.
    /// </summary>
    [Slot(Name = "config.get")]
    public class ConfigGet : ISlot
    {
        readonly IConfiguration _configuration;

        /// <summary>
        /// Creates a new instance of your class.
        /// </summary>
        /// <param name="configuration">Configuration settings for your application.</param>
        public ConfigGet(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Implementation of your slot.
        /// </summary>
        /// <param name="signaler">Signaler used to signal your slot.</param>
        /// <param name="input">Arguments to your slot.</param>
        public void Signal(ISignaler signaler, Node input)
        {
            input.Value = _configuration[input.GetEx<string>() ?? throw new ArgumentNullException("No value provided to [config]")];
        }
    }
}
