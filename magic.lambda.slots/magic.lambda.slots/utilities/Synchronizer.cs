/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.Threading;

// TODO: Consider moving class into its own utilities project for thread helpers.
namespace magic.lambda.slots.utilities
{
    /*
     * Helper class to synchronize access to some shared resource, potentially shared among
     * multiple threads.
     */
    internal class Synchronizer<TImpl, TIRead, TIWrite> where TImpl : TIWrite, TIRead
    {
        readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        readonly TImpl _shared;

        public Synchronizer(TImpl shared)
        {
            _shared = shared;
        }

        /*
         * Allows read only access inside of your lambda functor.
         */
        public void Read(Action<TIRead> functor)
        {
            _lock.EnterReadLock();
            try
            {
                functor(_shared);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /*
         * Allows read only access inside of your lambda functor, while
         * expecting you to return some object during invocation.
         */
        public T Read<T>(Func<TIRead, T> functor)
        {
            _lock.EnterReadLock();
            try
            {
                return functor(_shared);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /*
         * Allows read and write access inside of your lambda functor.
         */
        public void Write(Action<TIWrite> functor)
        {
            _lock.EnterWriteLock();
            try
            {
                functor(_shared);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
    }

    internal class Synchronizer<TImpl> : Synchronizer<TImpl, TImpl, TImpl>
    {
        public Synchronizer(TImpl shared)
            : base(shared)
        { }
    }
}