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
    /// [switch] slot allowing you to do branching in your code.
    /// </summary>
    [Slot(Name = "switch")]
    [Slot(Name = "wait.switch")]
    public class Switch : ISlot, ISlotAsync
    {
        /// <summary>
        /// Implementation of signal
        /// </summary>
        /// <param name="signaler">Signaler used to signal</param>
        /// <param name="input">Parameters passed from signaler</param>
        public void Signal(ISignaler signaler, Node input)
        {
            var executionNode = GetExecutionNode(input);
            if (executionNode != null)
                signaler.Signal(executionNode.Name, executionNode);
        }

        /// <summary>
        /// Implementation of signal
        /// </summary>
        /// <param name="signaler">Signaler used to signal</param>
        /// <param name="input">Parameters passed from signaler</param>
        /// <returns>An awaitable task.</returns>
        public async Task SignalAsync(ISignaler signaler, Node input)
        {
            var executionNode = GetExecutionNode(input);
            if (executionNode != null)
                await signaler.SignalAsync(executionNode.Name, executionNode);
        }

        #region [ -- Private helper methods -- ]

        /*
         * Returns the node to execute, if any.
         */
        Node GetExecutionNode(Node input)
        {
            if (!input.Children.Any(x => x.Name == "case"))
                throw new ApplicationException("[switch] must have one at least one [case] child");

            if (input.Children.Any(x => x.Name != "case" && x.Name != "default"))
                throw new ApplicationException("[switch] can only handle [case] and [default] children");

            if (input.Children.Any(x => x.Name == "case" && x.Value == null))
                throw new ApplicationException("[case] with null value found");

            if (input.Children.Any(x => x.Name == "default" && x.Value != null))
                throw new ApplicationException("[default] with non-null value found");

            var result = input.GetEx<object>();

            var executionNode = input.Children
                .FirstOrDefault(x => x.Name == "case" && x.Value.Equals(result)) ??
                input.Children
                    .FirstOrDefault(x => x.Name == "default");

            if (executionNode != null)
            {
                while (executionNode != null && !executionNode.Children.Any() && executionNode.Name != "default")
                {
                    executionNode = executionNode.Next;
                }
                if (executionNode != null && !executionNode.Children.Any())
                    throw new ApplicationException("No lambda object found for [case]");

                return executionNode;
            }
            return null;
        }

        #endregion
    }
}
