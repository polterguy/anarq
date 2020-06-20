/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using log4net;

namespace magic.library.internals
{
    /*
     * Internal class, implementing the ILog interface
     * for magic.lambda.logging.
     *
     * Uses log4net internally.
     */
    internal class TaskLogger : lambda.scheduler.utilities.ILogger
    {
        readonly static ILog _logger = LogManager.GetLogger(typeof(Logger));

        public void LogError(string taskName, Exception err)
        {
            _logger.Error($"An error occurred during evaluation of task named '{taskName}'", err);
        }

        public void LogInfo(string description)
        {
            _logger.Info(description);
        }
    }
}