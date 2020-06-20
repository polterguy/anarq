/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using magic.node;
using magic.node.extensions.hyperlambda;
using magic.lambda.scheduler.utilities.jobs;

namespace magic.lambda.scheduler.utilities
{
    /// <summary>
    /// Internal class to keep track of upcoming jobs.
    /// 
    /// Also responsible for loading and saving jobs to disc, etc.
    /// </summary>
    public sealed class Jobs : IDisposable
    {
        readonly List<Job> _jobs = new List<Job>();
        readonly string _pathToJobs;
        readonly bool _isFolderPath;

        /// <summary>
        /// Creates a new list of jobs, by loading serialized jobs from the
        /// given file or folder.
        /// </summary>
        /// <param name="pathToJobs">Path to a single file, or a folder,
        /// containing all serialized jobs.</param>
        public Jobs(string pathToJobs)
        {
            _pathToJobs = pathToJobs ?? throw new ArgumentNullException(nameof(pathToJobs));
            _isFolderPath = !_pathToJobs.EndsWith(".hl", StringComparison.InvariantCulture);
            if (_isFolderPath)
                _pathToJobs = _pathToJobs.Replace("\\", "/").TrimEnd('/') + "/";
            LoadJobs();
        }

        /// <summary>
        /// Adds a new job to the internal list of jobs, and saves all jobs into the job file.
        /// 
        /// Notice, will remove any jobs it has from before, having the same name as the name
        /// of your new job.
        /// </summary>
        /// <param name="job">Job you wish to add to this instance.</param>
        public void Add(Job job)
        {
            var old = _jobs.FirstOrDefault(x => x.Name == job.Name);
            if (old != null)
            {
                old.Stop();
                _jobs.Remove(old);
            }
            _jobs.Add(job);
            if (job.Persisted)
            {
                if (_isFolderPath)
                    SaveJob(job);
                else
                    SaveJobs();
            }
        }

        /// <summary>
        /// Deletes the job with the specified name, if any.
        /// 
        /// Will also specifically stop the job, to avoid that it is executed in the future,
        /// and discards the job's timer instance.
        /// </summary>
        /// <param name="job">Job to delete.</param>
        public void Delete(Job job)
        {
            job.Stop();
            _jobs.Remove(job);
            if (job.Persisted)
            {
                if (_isFolderPath)
                    File.Delete(_pathToJobs + job.Name + ".hl");
                else
                    SaveJobs();
            }
        }

        /// <summary>
        /// Will return the job with the specified name, if any.
        /// </summary>
        /// <param name="jobName">Name of job to retrieve.</param>
        /// <returns></returns>
        public Job Get(string jobName)
        {
            return _jobs.FirstOrDefault(x => x.Name == jobName);
        }

        /// <summary>
        /// Lists all jobs in this instance.
        /// </summary>
        /// <returns>List of all jobs in this instance, in no particular order.</returns>
        public IEnumerable<Job> List()
        {
            return _jobs;
        }

        #region [ -- Interface implementations -- ]

        public void Dispose()
        {
            // Notice, all jobs needs to be disposed, to dispose their System.Threading.Timer instances.
            foreach (var idx in _jobs)
            {
                idx.Dispose();
            }
        }

        #endregion

        #region [ -- Private and internal helper methods -- ]

        /*
         * Returns true if jobs are stored as multiple files inside a folder.
         */
        internal bool IsFolderPath => _isFolderPath;

        /*
         * Loads jobs from disc.
         */
        void LoadJobs()
        {
            if (_isFolderPath)
            {
                if (Directory.Exists(_pathToJobs))
                {
                    foreach (var idxFile in Directory.GetFiles(_pathToJobs, "*.hl"))
                    {
                        using (var stream = File.OpenRead(idxFile))
                        {
                            var lambda = new Parser(stream).Lambda();
                            foreach (var idx in lambda.Children)
                            {
                                _jobs.Add(Job.CreateJob(idx, true));
                            }
                        }
                    }
                }
                else
                {
                    Directory.CreateDirectory(_pathToJobs);
                }
            }
            else if (File.Exists(_pathToJobs))
            {
                using (var stream = File.OpenRead(_pathToJobs))
                {
                    var lambda = new Parser(stream).Lambda();
                    foreach (var idx in lambda.Children)
                    {
                        _jobs.Add(Job.CreateJob(idx, true));
                    }
                }
            }
        }

        /*
         * Saves single job to disc.
         */
        void SaveJob(Job job)
        {
            if (!job.Persisted)
                return;
            var hyper = Generator.GetHyper(new Node[] { job.GetNode() });
            using (var stream = File.CreateText(_pathToJobs + job.Name + ".hl"))
            {
                stream.Write(hyper);
            }
        }

        /*
         * Saves jobs to disc.
         */
        void SaveJobs()
        {
            var hyper = Generator.GetHyper(_jobs.Where(x => x.Persisted).Select(x => x.GetNode()));
            using (var stream = File.CreateText(_pathToJobs))
            {
                stream.Write(hyper);
            }
        }

        #endregion
    }
}
