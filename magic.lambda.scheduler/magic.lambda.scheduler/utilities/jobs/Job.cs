/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using magic.node;
using magic.node.extensions;

namespace magic.lambda.scheduler.utilities.jobs
{
    /// <summary>
    /// Class wrapping a single job, with its repetition pattern, or due date,
    /// and its associated lambda object to be executed when the job is due.
    /// </summary>
    public abstract class Job : IDisposable
    {
        bool _disposed;
        Timer _timer;

        /// <summary>
        /// Protected constructor to avoid direct instantiation, but
        /// forcing instantiation through factory create method instead.
        /// </summary>
        /// <param name="name">The name of your job.</param>
        /// <param name="description">Description for your job.</param>
        /// <param name="lambda">Actual lambda object to be evaluated when job is due.</param>
        protected Job(
            string name, 
            string description, 
            Node lambda,
            bool persisted)
        {
            Name = name;
            Description = description;
            Lambda = lambda.Clone();
            Persisted = persisted;
        }

        /// <summary>
        /// Name of job.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Description of job.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Actual lambda object to be executed when job is due.
        /// </summary>
        public Node Lambda { get; private set; }

        /// <summary>
        /// The due date for the next time the job should be executed.
        /// </summary>
        public DateTime Due { get; internal set; }

        /// <summary>
        /// If true, then job is persisted to disc.
        /// </summary>
        public bool Persisted { get; private set; }

        /// <summary>
        /// Creates a new job according to the declaration found in the specified node.
        /// </summary>
        /// <param name="jobNode">Declaration of job.</param>
        /// <param name="fromPersistentStorage">If true, will fetch the name of the job from the
        /// name of the node instead of from its value.</param>
        /// <returns>Newly created job.</returns>
        public static Job CreateJob(
            Node jobNode,
            bool fromPersistentStorage = false,
            bool sanityCheckName = false)
        {
            // Figuring out what type of job caller requests.
            var repetitionPattern = jobNode.Children.Where(x => x.Name == "repeat" || x.Name == "when" || x.Name == "immediate");
            if (repetitionPattern.Count() != 1)
                throw new ArgumentException("A job must have exactly one [repeat], [when] or [immediate] argument.");

            var name = fromPersistentStorage ? 
                jobNode.Name : 
                jobNode.GetEx<string>() ?? 
                throw new ArgumentException("No name give to job");
            if (sanityCheckName && !IsLegalName(name))
                throw new ArgumentException($"'{name}' is not a legal name for a task, only [0-9a-z], '-' and '_' can be used for your task's name");

            var description = jobNode.Children
                .FirstOrDefault(x => x.Name == "description")?.GetEx<string>();

            var persisted = jobNode.Children
                .FirstOrDefault(x => x.Name == "persisted")?.GetEx<bool>() ?? true;

            var lambda = jobNode.Children
                .FirstOrDefault(x => x.Name == ".lambda") ?? 
                throw new ArgumentException($"No [.lambda] given to job named '{name}'");

            // Creating actual job instance.
            switch (repetitionPattern.First().Name)
            {
                case "repeat":

                    return RepeatJob.CreateJob(
                        name,
                        description,
                        lambda,
                        persisted,
                        repetitionPattern.First().GetEx<string>(),
                        jobNode);

                case "when":

                    return new WhenJob(
                        name,
                        description,
                        lambda,
                        persisted,
                        repetitionPattern.First().GetEx<DateTime>());

                case "immediate":
                    return new WhenJob(
                        name,
                        description,
                        lambda,
                        persisted,
                        DateTime.Now);

                default:
                    throw new ApplicationException("You have reached a place in your code which should have been impossible to reach!");
            }
        }

        /// <summary>
        /// Returns the node representation for this particular instance, such that
        /// it can be serialized to disc, etc.
        /// </summary>
        /// <returns></returns>
        public virtual Node GetNode()
        {
            var result = new Node(Name);

            if (!string.IsNullOrEmpty(Description))
                result.Add(new Node("description", Description));

            result.Add(
                new Node(
                    ".lambda",
                    null,
                    Lambda.Children.Select(x => x.Clone())));

            return result;
        }

        /// <summary>
        /// Calculates the next due date for the job.
        /// </summary>
        protected abstract DateTime CalculateNextDue();

        #region [ -- Interface implementations -- ]

        /// <summary>
        /// Will dispose the Timer for the job.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposable pattern implementation.
        /// </summary>
        /// <param name="disposing">If true, will dispose the instance - Otherwise
        /// it will do nothing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;
            if (disposing)
                _timer?.Dispose();
            _disposed = true;
        }

        #endregion

        #region [ -- Internal and private helper methods -- ]

        /*
         * Creates the Timer, and its timeout timeout,
         * that will invoke the specified Func at the time the job should be executed.
         */
        internal void Schedule(Func<Job, Task> callback)
        {
            Due = CalculateNextDue();
            var nextDue = Math.Max(
                250,
                Math.Min(
                    (Due - DateTime.Now).TotalMilliseconds,
                    new TimeSpan(45, 0, 0, 0).TotalMilliseconds));
            _timer?.Dispose();
            _timer = new Timer(
                async (state) =>
                {
                    // Verifying job is actually due, which might not be true, if job's repetition pattern exceeds 45 days.
                    if (Due.AddMilliseconds(-250) > DateTime.Now)
                        Schedule(callback);
                    else
                        await callback(this);
                }, 
                null, 
                (long)nextDue, 
                Timeout.Infinite);
        }

        /*
         * Stops the job from being executed.
         */
        internal void Stop()
        {
            _timer?.Dispose();
            _timer = null;
        }

        /*
         * Returns true if name of job is legal.
         */
        static bool IsLegalName(string name)
        {
            foreach (var idx in name)
            {
                if ("abcdefghijklmnopqrstuvwxyz_-1234567890".IndexOf(idx) == -1)
                    return false;
            }
            return true;
        }

        #endregion
    }
}
