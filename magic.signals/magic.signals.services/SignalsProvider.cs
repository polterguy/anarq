/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.Linq;
using System.Collections.Generic;
using magic.signals.contracts;

namespace magic.signals.services
{
    /// <summary>
    /// Default implementation service class for the ISignalsProvider contract/interface.
    /// </summary>
    public class SignalsProvider : ISignalsProvider
    {
        readonly Dictionary<string, Type> _slots = new Dictionary<string, Type>();

        #region [ -- Interface implementation -- ]

        /// <summary>
        /// Creates an instance of the signals provider class. Notice, this should normally be associated
        /// with your IoC container as a Singleton instance somehow.
        /// </summary>
        /// <param name="types">Types to initially use for resolving slots.
        /// Notice, each type has to have at least one Slot attribute, declaring
        /// the name of the slot, and also implement either ISlot or ISlotAsync.</param>
        public SignalsProvider(IEnumerable<Type> types)
        {
            foreach (var idxType in types)
            {
                foreach (var idxAtr in idxType.GetCustomAttributes(true).OfType<SlotAttribute>())
                {
                    // Some basic sanity checking.
                    if (string.IsNullOrEmpty(idxAtr.Name))
                        throw new ArgumentNullException($"No name specified for type '{idxType}' in Slot attribute");

                    if (!typeof(ISlotAsync).IsAssignableFrom(idxType) && !typeof(ISlot).IsAssignableFrom(idxType))
                        throw new ApplicationException($"{idxType.FullName} is marked as a slot, but does not implement neither {nameof(ISlotAsync)} nor {nameof(ISlot)}");

                    if (_slots.ContainsKey(idxAtr.Name))
                        throw new ApplicationException($"Slot [{idxAtr.Name}] attempted registered by {idxType.FullName} is already registered by {_slots[idxAtr.Name].FullName}.");

                    _slots[idxAtr.Name] = idxType;
                }
            }
        }

        /// <summary>
        /// Returns all slots, or rather all slot names to be specific.
        /// </summary>
        public IEnumerable<string> Keys => _slots.Keys;

        /// <summary>
        /// Returns the slot with the specified name.
        /// </summary>
        /// <param name="name">Name for slot to retrieve.</param>
        /// <returns>Type implementing slot.</returns>
        public Type GetSlot(string name)
        {
            _slots.TryGetValue(name, out Type result);
            return result;
        }

        #endregion
    }
}
