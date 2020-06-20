/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using magic.node;

namespace magic.lambda.scheduler.utilities.jobs
{
    /// <summary>
    /// Class wrapping a single job, with its repetition pattern being
    /// "every last day of the month", at some specified time, and its
    /// associated lambda object to be executed when job is due.
    /// </summary>
    public sealed class LastDayOfMonthJob : RepeatJob
    {
        readonly int _hours;
        readonly int _minutes;

        /// <summary>
        /// Creates a new job that only executes on the very last day of the month,
        /// at some specific hour and minute during each last day of the month.
        /// </summary>
        /// <param name="name">Name of job.</param>
        /// <param name="description">Description for job.</param>
        /// <param name="lambda">Lambda object to be executed when job is due.</param>
        /// <param name="persisted">If true, then job is persisted to disc.</param>
        /// <param name="hours">At which hour during the day the job should executed.</param>
        /// <param name="minutes">At which minute, within the hour, the job should execute.</param>
        public LastDayOfMonthJob(
            string name, 
            string description, 
            Node lambda,
            bool persisted,
            int hours,
            int minutes)
            : base(name, description, lambda, persisted)
        {
            // Sanity checking invocation.
            if (hours < 0 || hours > 23)
                throw new ArgumentException($"{nameof(hours)} must be between 0 and 23");
            if (minutes < 0 || minutes > 59)
                throw new ArgumentException($"{nameof(hours)} must be between 0 and 59");

            _hours = hours;
            _minutes = minutes;
        }

        #region [ -- Overridden abstract base class methods -- ]

        /// <summary>
        /// Returns the node representation of the job.
        /// </summary>
        /// <returns>A node representing the declaration of the job as when created.</returns>
        public override Node GetNode()
        {
            var result = base.GetNode();
            result.Add(
                new Node(
                    "repeat", 
                    "last-day-of-month", 
                    new Node[]
                    {
                        new Node("time", FormatHours(_hours, _minutes))
                    }));
            return result;
        }

        /// <summary>
        /// Calculates the next due date for the job.
        /// </summary>
        protected override DateTime CalculateNextDue()
        {
            var candidate = new DateTime(
                DateTime.Now.Year,
                DateTime.Now.Month,
                DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month),
                _hours,
                _minutes,
                0,
                DateTimeKind.Utc);

            if (candidate.AddMilliseconds(250) < DateTime.Now)
            {
                // Shifting date one month ahead, since candidate date has already passed.
                var year = DateTime.Now.Month == 12 ? DateTime.Now.Year + 1 : DateTime.Now.Year;
                var month = DateTime.Now.Month == 12 ? 1 : DateTime.Now.Month + 1;
                candidate = new DateTime(
                    year,
                    month,
                    DateTime.DaysInMonth(year, month),
                    _hours,
                    _minutes,
                    0,
                    DateTimeKind.Utc);
            }
            return candidate;
        }

        #endregion
    }
}
