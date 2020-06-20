/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.Threading.Tasks;
using magic.node;
using magic.signals.contracts;
using magic.lambda.comparison.utilities;

namespace magic.lambda.comparison
{
    /// <summary>
    /// [mtw] slot returning true if its first child's value is "more than or equals" to its second child's value.
    /// </summary>
    [Slot(Name = "mte")]
    [Slot(Name = "wait.mte")]
    public class Mte : ISlot, ISlotAsync
    {
        /// <summary>
        /// Implementation of signal
        /// </summary>
        /// <param name="signaler">Signaler used to signal</param>
        /// <param name="input">Parameters passed from signaler</param>
        public void Signal(ISignaler signaler, Node input)
        {
            Common.Compare(signaler, input, (lhs, rhs) => Compare(lhs, rhs));
        }

        /// <summary>
        /// Implementation of signal
        /// </summary>
        /// <param name="signaler">Signaler used to signal</param>
        /// <param name="input">Parameters passed from signaler</param>
        /// <returns>An awaitable task.</returns>
        public async Task SignalAsync(ISignaler signaler, Node input)
        {
            await Common.CompareAsync(signaler, input, (lhs, rhs) => Compare(lhs, rhs));
        }

        #region [ -- Private helper methods -- ]

        bool Compare(object lhs, object rhs)
        {
            if (lhs == null && rhs == null)
                return true;
            else if (lhs != null && rhs == null)
                return true;
            else if (lhs == null && rhs != null)
                return false;
            else if (lhs.GetType() != rhs.GetType())
                return false;
            return ((IComparable)lhs).CompareTo(rhs) >= 0;
        }

        #endregion
    }
}
