/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;

namespace magic.lambda.logging.helpers
{
    /// <summary>
    /// Implementation interface to allow usage of any actual logging
    /// implementation. Notice, must be provided by user of library.
    /// </summary>
    public interface ILog
    {
        /// <summary>
        /// Logs a debug entry.
        /// </summary>
        /// <param name="value">Entry to log.</param>
        void Debug(string value);

        /// <summary>
        /// Logs an info entry.
        /// </summary>
        /// <param name="value">Entry to log.</param>
        void Info(string value);

        /// <summary>
        /// Logs an error, optionally associated with an exception.
        /// </summary>
        /// <param name="value">Entry to log.</param>
        /// <param name="exception">Exception to log.</param>
        void Error(string value, Exception exception = null);

        /// <summary>
        /// Logs a fatal error, optionally associated with an exception.
        /// </summary>
        /// <param name="value">Entry to log.</param>
        /// <param name="exception">Exception to log.</param>
        void Fatal(string value, Exception exception = null);
    }
}
