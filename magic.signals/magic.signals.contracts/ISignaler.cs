/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.Threading.Tasks;
using magic.node;

namespace magic.signals.contracts
{
    /// <summary>
    /// Interface allowing you to signal slots, passing ia Node as a generic argument.
    /// </summary>
    public interface ISignaler
    {
        /// <summary>
        /// Signals the slot with the name from the input node's Name property.
        /// </summary>
        /// <param name="name">Name of slot to invoke.</param>
        /// <param name="input">Input arguments to slot.</param>
        void Signal(string name, Node input);

        /// <summary>
        /// Signals the slot with the name from the input node's Name property async.
        /// </summary>
        /// <param name="name">Name of slot to invoke.</param>
        /// <param name="input">Input arguments to slot.</param>
        /// <returns>Awaitable task.</returns>
        Task SignalAsync(string name, Node input);

        /// <summary>
        /// Adds the given stack value unto the stack with the given name,
        /// and invokes functor, making sure the object is popped from the stack
        /// after the functor has been evaluated.
        /// </summary>
        /// <param name="name">Name of stack object, allowing you to retrieve it
        /// from recursively invoked slots.</param>
        /// <param name="value">Value of stack object. Use Peek to retrieve the
        /// object in recursively invoked slots.</param>
        /// <param name="functor">Callback evaluated while object is on the
        /// stack.</param>
        void Scope(string name, object value, Action functor);

        /// <summary>
        /// Adds the given stack value unto the stack with the given name,
        /// and invokes functor, making sure the object is popped from the stack
        /// after the functor has been evaluated.
        /// </summary>
        /// <param name="name">Name of stack object, allowing you to retrieve
        /// it from recursively invoked slots.</param>
        /// <param name="value">Value of stack object. Use Peek to retrieve the
        /// object in recursively invoked slots.</param>
        /// <param name="functor">Callback evaluated while object is on the
        /// stack.</param>
        /// <returns>Awaitable task.</returns>
        Task ScopeAsync(string name, object value, Func<Task> functor);

        /// <summary>
        /// Returns the last stack object previously pushed with the specified
        /// name.
        /// </summary>
        /// <typeparam name="T">Type to return object as.</typeparam>
        /// <param name="name">Name of stack object to retrieve.</param>
        /// <returns>Your stack object.</returns>
        T Peek<T>(string name) where T : class;
    }
}
