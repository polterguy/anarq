/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System.Linq;
using System.Threading.Tasks;
using magic.node;
using magic.node.extensions;
using magic.signals.contracts;

namespace magic.lambda.loops
{
    /// <summary>
    /// [for-each] slot allowing you to iterate through a list of node, resulting from the evaluation of an expression.
    /// </summary>
    [Slot(Name = "for-each")]
    [Slot(Name = "wait.for-each")]
    public class ForEach : ISlot, ISlotAsync
    {
        /// <summary>
        /// Implementation of signal
        /// </summary>
        /// <param name="signaler">Signaler used to signal</param>
        /// <param name="input">Parameters passed from signaler</param>
        public void Signal(ISignaler signaler, Node input)
        {
            // Making sure we can reset back to original nodes after every single iteration.
            var old = input.Children.Select(x => x.Clone()).ToList();

            // Storing termination node, to check if we should terminate early for some reasons.
            var terminate = signaler.Peek<Node>("slots.result");

            foreach (var idx in input.Evaluate().ToList())
            {
                // Inserting "data pointer".
                input.Insert(0, new Node(".dp", idx));

                // Evaluating "body" lambda of [for-each]
                signaler.Signal("eval", input);

                // Resetting back to original nodes.
                input.Clear();

                // Notice, cloning in case we've got another iteration, to avoid changing original nodes' values.
                input.AddRange(old.Select(x => x.Clone()));

                // Checking if execution for some reasons was terminated.
                if (terminate != null && (terminate.Value != null || terminate.Children.Any()))
                    return;
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
            // Making sure we can reset back to original nodes after every single iteration.
            var old = input.Children.Select(x => x.Clone()).ToList();

            // Storing termination node, to check if we should terminate early for some reasons.
            var terminate = signaler.Peek<Node>("slots.result");

            foreach (var idx in input.Evaluate())
            {
                // Inserting "data pointer".
                input.Insert(0, new Node(".dp", idx));

                // Evaluating "body" lambda of [for-each]
                await signaler.SignalAsync("wait.eval", input);

                // Resetting back to original nodes.
                input.Clear();

                // Notice, cloning in case we've got another iteration, to avoid changing original nodes' values.
                input.AddRange(old.Select(x => x.Clone()));

                // Checking if execution for some reasons was terminated.
                if (terminate != null && (terminate.Value != null || terminate.Children.Any()))
                    return;
            }
        }
    }
}
