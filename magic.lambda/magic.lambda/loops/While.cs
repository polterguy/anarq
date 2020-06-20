/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using magic.node;
using magic.node.extensions;
using magic.signals.contracts;

namespace magic.lambda.loops
{
    /// <summary>
    /// [while] slot that will evaluate its lambda object as long as its condition is true.
    /// </summary>
    [Slot(Name = "while")]
    [Slot(Name = "wait.while")]
    public class While : ISlot, ISlotAsync
    {
        readonly int _maxIterations;

        /// <summary>
        /// Creates an instance of your slot.
        /// </summary>
        /// <param name="configuration">Configuration for your application.</param>
        public While(IConfiguration configuration)
        {
            _maxIterations = int.Parse(configuration?["magic:lambda:while:max-iterations"] ?? "5000");
        }

        /// <summary>
        /// Implementation of signal
        /// </summary>
        /// <param name="signaler">Signaler used to signal</param>
        /// <param name="input">Parameters passed from signaler</param>
        public void Signal(ISignaler signaler, Node input)
        {
            if (input.Children.Count() != 2)
                throw new ApplicationException("Keyword [while] requires exactly two child nodes");

            // Storing termination node, to check if we should terminate early for some reasons.
            var terminate = signaler.Peek<Node>("slots.result");

            // Making sure we don't enter an infinite loop.
            int iterations = 0;

            // Evaluating lambda while condition is true.
            while (true)
            {
                // Checking if we've passed our maximum number of iterations.
                if (iterations++ == _maxIterations)
                    throw new ApplicationException($"Your [while] loop exceeded the maximum number of iterations, which are {_maxIterations}. Refactor your Hyperlambda, or increase your configuration setting.");

                // Making sure we can reset back to original nodes after every single iteration.
                var old = input.Children.Select(x => x.Clone()).ToList();

                // This will evaluate the condition.
                signaler.Signal("eval", input);

                // Verifying we're supposed to proceed into body of [while].
                if (!input.Children.First().GetEx<bool>())
                    break;

                // Retrieving [.lambda] node and doing basic sanity check.
                var lambda = input.Children.Skip(1).First();
                if (lambda.Name != ".lambda")
                    throw new ApplicationException("Keyword [while] requires its second child to be [.lambda]");

                // Evaluating "body" lambda of [while].
                signaler.Signal("eval", lambda);

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
            if (input.Children.Count() != 2)
                throw new ApplicationException("Keyword [while] requires exactly two child nodes");

            // Storing termination node, to check if we should terminate early for some reasons.
            var terminate = signaler.Peek<Node>("slots.result");

            // Making sure we don't enter an infinite loop.
            int iterations = 0;

            // Evaluating lambda while condition is true.
            while (true)
            {
                // Checking if we've passed our maximum number of iterations.
                if (iterations++ == _maxIterations)
                    throw new ApplicationException($"Your [while] loop exceeded the maximum number of iterations, which are {_maxIterations}. Refactor your Hyperlambda, or increase your configuration setting.");

                // Making sure we can reset back to original nodes after every single iteration.
                var old = input.Children.Select(x => x.Clone()).ToList();

                // This will evaluate the condition.
                await signaler.SignalAsync("wait.eval", input);

                // Verifying we're supposed to proceed into body of [while].
                if (!input.Children.First().GetEx<bool>())
                    break;

                // Retrieving [.lambda] node and doing basic sanity check.
                var lambda = input.Children.Skip(1).First();
                if (lambda.Name != ".lambda")
                    throw new ApplicationException("Keyword [while] requires its second child to be [.lambda]");

                // Evaluating "body" lambda of [while].
                await signaler.SignalAsync("wait.eval", lambda);

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
