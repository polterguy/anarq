/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using magic.node;
using magic.node.extensions;
using magic.signals.contracts;

namespace magic.lambda.branching
{
    /// <summary>
    /// [if] slot, allowing you to do branching in your code.
    /// </summary>
    [Slot(Name = "if")]
    [Slot(Name = "wait.if")]
    public class If : ISlot, ISlotAsync
    {
        /// <summary>
        /// Implementation of signal
        /// </summary>
        /// <param name="signaler">Signaler used to signal</param>
        /// <param name="input">Parameters passed from signaler</param>
        public void Signal(ISignaler signaler, Node input)
        {
            // Evaluating condition.
            signaler.Signal("eval", input);

            // Checking if condition evaluated to true.
            if (input.Children.First().GetEx<bool>())
            {
                // Retrieving and evaluating lambda node.
                signaler.Signal("eval", GetLambdaNode(input));
            }
        }

        /// <summary>
        /// Implementation of signal
        /// </summary>
        /// <param name="signaler">Signaler used to signal</param>
        /// <param name="input">Parameters passed from signaler</param>
        /// <returns>An awaitable task.</returns>
        public async Task SignalAsync(ISignaler signaler, Node input)
        {
            // Evaluating condition.
            await signaler.SignalAsync("wait.eval", input);

            // Checking if condition evaluated to true.
            if (input.Children.First().GetEx<bool>())
            {
                // Retrieving and evaluating lambda node.
                await signaler.SignalAsync("wait.eval", GetLambdaNode(input));
            }
        }

        #region [ -- Private helper methods -- ]

        /*
         * Returns the lambda execution node for the above two methods,
         * and does some basic sanity checking of invocation.
         */
        Node GetLambdaNode(Node input)
        {
            // Sanity checking invocation.
            if (input.Children.Count() != 2)
                throw new ApplicationException("Keyword [if] requires exactly two child nodes, one comparer node and one [.lambda] node, in that sequence");

            // Retrieving lambda node, and sanity checking it.
            var lambda = input.Children.Skip(1).First();
            if (lambda.Name != ".lambda")
                throw new ApplicationException("Keyword [if] requires its second child to be [.lambda]");

            return lambda;
        }

        #endregion
    }
}
