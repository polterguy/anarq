/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using magic.node;

namespace magic.lambda.scheduler.utilities.jobs
{
    /// <summary>
    /// Class wrapping a single job, with its repetition pattern being "every n'th of x",
    /// where n is a positive integer value, and x is some sort of entity, such as "days", "hours", etc -
    /// And its associated lambda object to be executed when the job is due.
    /// </summary>
    public sealed class EveryEntityRepeatJob : RepeatJob
    {
        /// <summary>
        /// Repetition pattern for job.
        /// </summary>
        public enum RepetitionEntityType
        {
            /// <summary>
            /// Every n second.
            /// </summary>
            seconds,

            /// <summary>
            /// Every n minute.
            /// </summary>
            minutes,

            /// <summary>
            /// Every n hour.
            /// </summary>
            hours,

            /// <summary>
            /// Every n day.
            /// </summary>
            days
        };

        readonly private RepetitionEntityType _repetition;
        readonly private long _repetitionValue;

        /// <summary>
        /// Creates a new job that repeat every n days/hours/minutes/seconds.
        /// </summary>
        /// <param name="name">Name of new job.</param>
        /// <param name="description">Description of job.</param>
        /// <param name="lambda">Lambda object to execute as job is due.</param>
        /// <param name="persisted">If true, then job is persisted to disc.</param>
        /// <param name="repetition">Repetition pattern.</param>
        /// <param name="repetitionValue">Number of entities declared through repetition pattern.</param>
        public EveryEntityRepeatJob(
            string name, 
            string description, 
            Node lambda,
            bool persisted,
            RepetitionEntityType repetition,
            long repetitionValue)
            : base(name, description, lambda, persisted)
        {
            // Sanity checking and decorating instance.
            if (repetitionValue < 1)
                throw new ArgumentException("Repetition value must be a positive integer value.");

            _repetition = repetition;
            _repetitionValue = repetitionValue;
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
                    _repetition.ToString(),
                    new Node[]
                    {
                        new Node("value", _repetitionValue)
                    }));
            return result;
        }

        /// <summary>
        /// Calculates the job's next due date.
        /// </summary>
        protected override DateTime CalculateNextDue()
        {
            switch (_repetition)
            {
                case RepetitionEntityType.seconds:
                    return DateTime.Now.AddSeconds(_repetitionValue);

                case RepetitionEntityType.minutes:
                    return DateTime.Now.AddMinutes(_repetitionValue);

                case RepetitionEntityType.hours:
                    return DateTime.Now.AddHours(_repetitionValue);

                case RepetitionEntityType.days:
                    return DateTime.Now.AddDays(_repetitionValue);

                default:
                    throw new ApplicationException("Oops, you've made it into an impossible code branch!");
            }
        }

        #endregion
    }
}
