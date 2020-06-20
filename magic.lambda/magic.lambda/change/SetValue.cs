/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using magic.node;
using magic.node.extensions;
using magic.signals.contracts;

namespace magic.lambda.change
{
    /// <summary>
    /// [set-value] and [set-x] slots allowing you to change the values of nodes in your lambda graph object.
    /// If you use [set-x] any expresions in your source will not be evaluated, allowing you to set the values
    /// of nodes to become expressions.
    /// </summary>
    [Slot(Name = "set-x")]
    [Slot(Name = "set-value")]
    [Slot(Name = "wait.set-x")]
    [Slot(Name = "wait.set-value")]
    public class SetValue : ISlot, ISlotAsync
    {
        /// <summary>
        /// Implementation of signal
        /// </summary>
        /// <param name="signaler">Signaler used to signal</param>
        /// <param name="input">Parameters passed from signaler</param>
        public void Signal(ISignaler signaler, Node input)
        {
            if (input.Children.Count() > 1)
                throw new ApplicationException("[set-value] can have maximum one child node");

            signaler.Signal("eval", input);

            SetValueToSource(input, input.Evaluate().ToList());
        }

        /// <summary>
        /// Implementation of signal
        /// </summary>
        /// <param name="signaler">Signaler used to signal</param>
        /// <param name="input">Parameters passed from signaler</param>
        /// <returns>An awaitable task.</returns>
        public async Task SignalAsync(ISignaler signaler, Node input)
        {
            if (input.Children.Count() > 1)
                throw new ApplicationException("[set-value] can have maximum one child node");

            await signaler.SignalAsync("wait.eval", input);
            SetValueToSource(input, input.Evaluate().ToList());
        }

        #region [ -- Private helper methods -- ]

        private static void SetValueToSource(Node input, IEnumerable<Node> destinations)
        {
            var source = input.Name.EndsWith("set-value", StringComparison.InvariantCulture) ? input.Children.FirstOrDefault()?.GetEx<object>() : input.Children.FirstOrDefault()?.Get<object>();
            foreach (var idx in destinations)
            {
                idx.Value = source;
            }
        }

        #endregion
    }
}
