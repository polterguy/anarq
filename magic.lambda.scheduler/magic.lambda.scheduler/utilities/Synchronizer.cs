/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.Threading;

namespace magic.lambda.scheduler.utilities
{
    /// <summary>
    /// Helper class to synchronize access to the shared scheduler instance.
    /// 
    /// TODO : Merge into utility library.
    /// </summary>
    /// <typeparam name="T">Type of shared object.</typeparam>
    public sealed class Synchronizer<T> : IDisposable
    {
        readonly ReaderWriterLockSlim _locker = new ReaderWriterLockSlim();
        readonly T _shared;

        /// <summary>
        /// Creates a new instance of your synchronizer.
        /// Pass in the resource you need to share between multiple threads, and need
        /// to have synchronized access to.
        /// </summary>
        /// <param name="shared">Shared resource you need synchronized access to.</param>
        public Synchronizer(T shared)
        {
            _shared = shared;
        }

        /// <summary>
        /// Acquires a read lock and executes the specified function.
        /// </summary>
        /// <param name="functor">Callback that will be invoked with a read lock around your
        /// shared resource.</param>
        public void Read(Action<T> functor)
        {
            _locker.EnterReadLock();
            try
            {
                functor(_shared);
            }
            finally
            {
                _locker.ExitReadLock();
            }
        }

        /*
         * 
         */
        /// <summary>
        /// Acquires a read lock, executes the specified function, and returns its result to caller.
        /// </summary>
        /// <typeparam name="T2">Type of resource to return from your callback.</typeparam>
        /// <param name="functor">Callback that will be invoked with a read lock around your
        /// shared resource, expected to return an instance of type T2.</param>
        /// <returns></returns>
        public T2 Read<T2>(Func<T, T2> functor)
        {
            _locker.EnterReadLock();
            try
            {
                return functor(_shared);
            }
            finally
            {
                _locker.ExitReadLock();
            }
        }

        /// <summary>
        /// Acquires a write lock, and invokes the specified Action.
        /// </summary>
        /// <param name="functor">Callback that will be evaluated with a write lock wrapping
        /// your shared resource.</param>
        public void Write(Action<T> functor)
        {
            _locker.EnterWriteLock();
            try
            {
                functor(_shared);
            }
            finally
            {
                _locker.ExitWriteLock();
            }
        }

        /// <summary>
        /// Acquires a write lock, and invokes the specified Action, returning some object to caller.
        /// </summary>
        /// <typeparam name="T2">Type to return from callback.</typeparam>
        /// <param name="functor">Callback that will be evaluated with a write lock wrapping your
        /// shared resource, expected to return an instance of type T2.</param>
        /// <returns></returns>
        public T2 ReadWrite<T2>(Func<T, T2> functor)
        {
            _locker.EnterWriteLock();
            try
            {
                return functor(_shared);
            }
            finally
            {
                _locker.ExitWriteLock();
            }
        }

        #region [ -- Interface implementations -- ]

        /// <summary>
        /// Disposes the ReaderWriterLockSlim, and its associated shared resource,
        /// if the resource implements IDisposable.
        /// </summary>
        public void Dispose()
        {
            if (_shared is IDisposable disp)
                disp.Dispose();
            _locker.Dispose();
        }

        #endregion
    }
}
