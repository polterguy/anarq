/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using magic.node;
using magic.signals.contracts;
using magic.lambda.logging.helpers;

namespace magic.lambda.logging
{
    /// <summary>
    /// [log.info] slot for logging informational pieces of log entries.
    /// </summary>
    [Slot(Name = "log.info")]
    public class LogInfo : ISlot
    {
        readonly ILog _logger;

        /// <summary>
        /// Creates an instance of your type.
        /// </summary>
        /// <param name="logger">Actual implementation.</param>
        public LogInfo(ILog logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Slot implementation.
        /// </summary>
        /// <param name="signaler">Signaler that raised the signal.</param>
        /// <param name="input">Arguments to slot.</param>
        public void Signal(ISignaler signaler, Node input)
        {
            var entry = Helper.GetLogInfo(signaler, input);
            _logger.Info(entry);
        }
    }
}
