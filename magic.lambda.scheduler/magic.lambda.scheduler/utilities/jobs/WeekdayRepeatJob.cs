/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using magic.node;

namespace magic.lambda.scheduler.utilities.jobs
{
    /// <summary>
    /// Class wrapping a single job, with a weekly repetition pattern, a
    /// declaration of what hour during the day job should be executed,
    /// and its associated lambda object to be executed when job is to be executed.
    /// </summary>
    public sealed class WeekdayRepeatJob : RepeatJob
    {
        readonly DayOfWeek _weekday;
        readonly int _hours;
        readonly int _minutes;

        /// <summary>
        /// Constructor creating a job that is to be executed once every specified weekday,
        /// at some specified time of the day.
        /// </summary>
        /// <param name="name">The name of your job.</param>
        /// <param name="description">Description of your job.</param>
        /// <param name="lambda">Actual lambda object to be executed when job is due.</param>
        /// <param name="persisted">If true, then job is persisted to disc.</param>
        /// <param name="weekday">Which day of the week the job should be executed</param>
        /// <param name="hours">At what hour during the day the job should be executed.</param>
        /// <param name="minutes">At what minute, within its hours, the job should be executed.</param>
        public WeekdayRepeatJob(
            string name, 
            string description, 
            Node lambda,
            bool persisted,
            DayOfWeek weekday,
            int hours,
            int minutes)
            : base(name, description, lambda, persisted)
        {
            _weekday = weekday;
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
                    _weekday.ToString(),
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
            var when = DateTime.Now
                .Date
                .AddHours(_hours)
                .AddMinutes(_minutes);

            while (when.AddMilliseconds(250) < DateTime.Now || _weekday != when.DayOfWeek)
            {
                when = when.AddDays(1);
            }
            return when;
        }

        #endregion
    }
}
