/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using magic.node;
using magic.signals.contracts;
using magic.lambda.scheduler.utilities.jobs;

namespace magic.lambda.scheduler.utilities
{
    /// <summary>
    /// The background service responsible for scheduling and executing jobs.
    ///
    /// Notice, you should make sure you resolve this as a singleton if you are
    /// using an IoC container. Also notice that no jobs will execute before
    /// you explicitly somehow invoke Start on your instance, which you can do
    /// automatically by instantiating the class with autoStart set to true.
    /// 
    /// The class is thread safe, and all operations towards its internal
    /// list of jobs is synchronized
    /// </summary>
    public sealed class Scheduler : IDisposable
    {
        readonly SemaphoreSlim _sempahore;
        readonly IServiceProvider _services;
        readonly ILogger _logger;
        readonly Synchronizer<Jobs> _jobSynchroniser;
        bool _running;

        /// <summary>
        /// Creates a new scheduler, responsible for scheduling and
        /// executing jobs that have been scheduled for future execution.
        /// </summary>
        /// <param name="services">Service provider to resolve ISignaler.</param>
        /// <param name="logger">Logging provider necessary to be able to log jobs that are
        /// not executed successfully.</param>
        /// <param name="jobsPath">The path to your job file,
        /// declaring what jobs your application has scheduled for future
        /// execution. Jobs will be serialized into this file, such that if the
        /// process for some reasons is taken down, the jobs will be reloaded the next
        /// time the scheduler is instantiated again.</param>
        /// <param name="autoStart">If true, will start service immediately automatically.</param>
        /// <param name="maxThreads">Maximum number of concurrent jobs to execute simultaneously.</param>
        public Scheduler(
            IServiceProvider services,
            ILogger logger,
            string jobsPath,
            bool autoStart,
            int maxThreads)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _logger = logger;
            _sempahore = new SemaphoreSlim(maxThreads);
            _jobSynchroniser = new Synchronizer<Jobs>(new Jobs(jobsPath));
            if (autoStart)
                Start();
        }

        /// <summary>
        /// Returns true if scheduler is running.
        /// </summary>
        public bool Running
        {
            get { return _jobSynchroniser.Read(jobs => { return _running; }); }
        }

        /// <summary>
        /// Starts your scheduler. You must invoke this method in order to
        /// start your scheduler, or instantiate the class with autoStart set
        /// to true.
        /// </summary>
        public void Start()
        {
            _jobSynchroniser.Write(jobs =>
            {
                _running = true;
                foreach (var idx in jobs.List())
                {
                    idx.Schedule(async (x) => await Execute(x));
                }
            });
        }

        /// <summary>
        /// Stops your scheduler, such that no more jobs will be executed, before it
        /// is explicitly started again.
        /// </summary>
        public void Stop()
        {
            _jobSynchroniser.Write(jobs =>
            {
                _running = false;
                foreach (var idx in jobs.List())
                {
                    idx.Stop();
                }
            });
        }

        /// <summary>
        /// Returns all jobs in the scheduler to caller.
        /// </summary>
        /// <returns>All jobs registered in the scheduler.</returns>
        public List<Job> List()
        {
            return _jobSynchroniser.Read(jobs => jobs.List().ToList());
        }

        /// <summary>
        /// Returns a previously created job to caller.
        /// </summary>
        /// <param name="jobName">Name of job you wish to retrieve.</param>
        /// <returns>A node representing your job.</returns>
        public Job Get(string jobName)
        {
            // Getting job with specified name.
            return _jobSynchroniser.Read(jobs => jobs.Get(jobName));
        }

        /// <summary>
        /// Creates a new job, and adds it to the schweduler's list of jobs.
        ///
        /// If scheduler is running, job will be automatically started.
        /// </summary>
        /// <param name="node">Declaration of your job.</param>
        public void Create(Node node)
        {
            _jobSynchroniser.Write(jobs =>
            {
                var job = Job.CreateJob(node, false, jobs.IsFolderPath);
                jobs.Add(job);
                if (_running)
                    job.Schedule(async (x) => await Execute(x));
            });
        }

        /// <summary>
        /// Deletes an existing job from your scheduler.
        /// </summary>
        /// <param name="jobName">Name of job to delete.</param>
        public void Delete(string jobName)
        {
            _jobSynchroniser.Write(jobs =>
            {
                var job = jobs.Get(jobName);
                if (job != null)
                    jobs.Delete(job);
            });
        }

        #region [ -- Interface implementations -- ]

        /// <summary>
        /// Disposes the scheduler.
        /// </summary>
        public void Dispose()
        {
            _jobSynchroniser.Dispose();
        }

        #endregion

        #region [ -- Private helper methods -- ]

        /*
         * Callback method that is executed when a job is due.
         * 
         * Simply evaluates the lambda associated with the job, and recalculates
         * when the job is due again, if the job is repeating - Otherwise, it'll
         * delete the job from the list of jobs after having executed it.
         */
        async Task Execute(Job job)
        {
            // Making sure no more than "maxThreads" are executed simultaneously.
            await _sempahore.WaitAsync();

            try
            {
                _logger?.LogInfo($"Job with name of '{job.Name}' started executing.");
                var lambda = job.Lambda.Clone();
                var signaler = _services.GetService(typeof(ISignaler)) as ISignaler;
                var timer = Stopwatch.StartNew();
                await signaler.SignalAsync("wait.eval", lambda);
                var time = timer.ElapsedMilliseconds;
                _logger?.LogInfo($"Job with name of '{job.Name}' executed successfully in {time} milliseconds.");
            }
            catch (Exception err)
            {
                _logger?.LogError(job.Name, err);
            }
            finally
            {
                if (job is RepeatJob)
                {
                    job.Schedule(async (x) => await Execute(x));
                }
                else
                {
                    _jobSynchroniser.Write(jobs =>
                    {
                        job.Stop();
                        jobs.Delete(job);
                    });
                }
                _sempahore.Release();
            }
        }

        #endregion
    }
}
