/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System.Linq;
using System.Collections.Generic;
using magic.node;
using magic.node.extensions;
using magic.signals.contracts;
using magic.lambda.slots.utilities;

namespace magic.lambda.slots
{
    /// <summary>
    /// [slots.create] slot that creates a dynamic slot, that can be invoked using the [signal] slot.
    /// </summary>
    [Slot(Name = "slots.create")]
    public class Create : ISlot
    {
        readonly static Synchronizer<Dictionary<string, Node>> _slots = new Synchronizer<Dictionary<string, Node>>(new Dictionary<string, Node>());

        /// <summary>
        /// Slot implementation.
        /// </summary>
        /// <param name="signaler">Signaler that raised signal.</param>
        /// <param name="input">Arguments to slot.</param>
        public void Signal(ISignaler signaler, Node input)
        {
            _slots.Write((slots) => slots[input.GetEx<string>()] = input.Clone());
        }

        #region [ -- Private and internal helper methods -- ]

        /*
         * Returns the named slot to caller.
         */
        internal static Node GetSlot(string name)
        {
            return _slots.Read((slots) => slots[name].Clone());
        }

        /*
         * Returns true to caller if the named slot exists.
         */
        internal static bool SlotExists(string name)
        {
            return _slots.Read((slots) => slots.ContainsKey(name));
        }

        /*
         * Returns the names of all slots that exists in the system.
         */
        internal static IEnumerable<string> Slots()
        {
            return _slots.Read((slots) => slots.Keys.ToList());
        }

        /*
         * Deletes the named slot.
         */
        internal static void DeleteSlot(string name)
        {
            _slots.Write((slots) => slots.Remove(name));
        }

        #endregion
    }
}
