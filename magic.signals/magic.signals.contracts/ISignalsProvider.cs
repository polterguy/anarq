/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.Collections.Generic;

namespace magic.signals.contracts
{
    /// <summary>
    /// Interface responsible for feeding your signaler with signals,
    /// implying strings to types mappings.
    /// </summary>
    public interface ISignalsProvider
    {
        /// <summary>
        /// Returns a type from the specified name.
        /// </summary>
        /// <param name="name">Slot name for the type to return.</param>
        /// <returns>The underlaying type that maps to your string.</returns>
        Type GetSlot(string name);

        /// <summary>
        /// Returns all keys, implying names registered for your signals
        /// implementation.
        /// </summary>
        IEnumerable<string> Keys { get; }
    }
}
