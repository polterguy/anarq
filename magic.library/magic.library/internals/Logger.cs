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
    internal class Logger : lambda.logging.helpers.ILog
    {
        readonly static ILog _logger = LogManager.GetLogger(typeof(Logger));

        public void Debug(string value)
        {
            _logger.Debug(value);
        }

        public void Error(string value, Exception exception = null)
        {
            _logger.Error(value);
        }

        public void Fatal(string value, Exception exception = null)
        {
            _logger.Fatal(value);
        }

        public void Info(string value)
        {
            _logger.Info(value);
        }
    }
}