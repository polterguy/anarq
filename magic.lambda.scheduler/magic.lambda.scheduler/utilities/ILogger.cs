/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;

namespace magic.lambda.scheduler.utilities
{
    /// <summary>
    /// Interface necessary to provide in order to log errors occurring
    /// during execution of jobs.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Invoked when an error occurs during execution of a job.
        /// </summary>
        /// <param name="jobName">Name of job that created the error.</param>
        /// <param name="err">Exception that occurred.</param>
        void LogError(string jobName, Exception err);

        /// <summary>
        /// Invoked when a job is being scheduled for execution.
        /// </summary>
        /// <param name="description">Will contain a description of the job, and
        /// its name.</param>
        void LogInfo(string description);
    }
}
