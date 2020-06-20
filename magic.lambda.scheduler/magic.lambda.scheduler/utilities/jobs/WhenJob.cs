/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using magic.node;

namespace magic.lambda.scheduler.utilities.jobs
{
    /// <summary>
    /// Class wrapping a single job, with its due date,
    /// and its associated lambda object to be executed when job is due.
    ///
    /// Notice, this type of job is only execueted onde, for then to be deleted
    /// from the scheduler after execution.
    /// </summary>
    public sealed class WhenJob : Job
    {
        /// <summary>
        /// Constructor creating a job that is to be executed only once,
        /// and then deleted.
        /// </summary>
        /// <param name="name">The name of your job.</param>
        /// <param name="description">Description for your job.</param>
        /// <param name="lambda">Actual lambda object to be executed when job is due.</param>
        /// <param name="persisted">If true, then job is persisted to disc.</param>
        /// <param name="when">Date of when job should be executed.</param>
        public WhenJob(
            string name, 
            string description, 
            Node lambda,
            bool persisted,
            DateTime when)
            : base(name, description, lambda, persisted)
        {
            // Making sure we never create a job that should have been executed in the past.
            if (when.AddMilliseconds(25) < DateTime.Now)
                when = DateTime.Now.AddMilliseconds(25);
            Due = when;
        }

        #region [ -- Overridden abstract base class methods -- ]

        /// <summary>
        /// Returns the node representation of the job.
        /// </summary>
        /// <returns>A node representing the declaration of the job as when created.</returns>
        public override Node GetNode()
        {
            var result = base.GetNode();
            result.Add(new Node("when", Due));
            result.Add(new Node("persisted", Persisted));
            return result;
        }

        /// <summary>
        /// Calculates the next due date for the job.
        /// 
        /// Notice, do not invoke this method for this type of job, since it will throw an exception,
        /// since job is not repeating, and its only execution date should have been supplied when
        /// the job was created.
        /// </summary>
        protected override DateTime CalculateNextDue()
        {
            return Due;
        }

        #endregion
    }
}
