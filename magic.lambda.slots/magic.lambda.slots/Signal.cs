/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System.Linq;
using System.Threading.Tasks;
using magic.node;
using magic.node.extensions;
using magic.signals.contracts;

namespace magic.lambda.slots
{
    /// <summary>
    /// [signal] slot for invoking dynamically created slots, that have been created with the [slots.create] slot.
    /// </summary>
    [Slot(Name = "signal")]
    [Slot(Name = "wait.signal")]
    public class SignalSlot : ISlot, ISlotAsync
    {
        /// <summary>
        /// Slot implementation.
        /// </summary>
        /// <param name="signaler">Signaler that raised signal.</param>
        /// <param name="input">Arguments to slot.</param>
        public void Signal(ISignaler signaler, Node input)
        {
            // Making sure we're able to handle returned values and nodes from slot implementation.
            var result = new Node();
            signaler.Scope("slots.result", result, () =>
            {
                // Retrieving slot's lambda, no reasons to clone, GetSlot will clone.
                var lambda = Create.GetSlot(input.GetEx<string>());

                // Preparing arguments, if there are any.
                if (input.Children.Any())
                    lambda.Insert(0, new Node(".arguments", null, input.Children.ToList()));

                // Evaluating lambda of slot.
                signaler.Signal("eval", lambda);

                // Clearing Children collection, since it might contain input parameters.
                input.Clear();

                /*
                * Simply setting value and children to "slots.result" stack object, since its value
                * was initially set to its old value during startup of method.
                */
                input.Value = result.Value;
                input.AddRange(result.Children.ToList());
            });
        }

        /// <summary>
        /// Slot implementation.
        /// </summary>
        /// <param name="signaler">Signaler that raised signal.</param>
        /// <param name="input">Arguments to slot.</param>
        /// <result>Arguments to slot.</result>
        public async Task SignalAsync(ISignaler signaler, Node input)
        {
            // Making sure we're able to handle returned values and nodes from slot implementation.
            var result = new Node();
            await signaler.ScopeAsync("slots.result", result, async () =>
            {
                // Retrieving slot's lambda, no reasons to clone, GetSlot will clone.
                var lambda = Create.GetSlot(input.GetEx<string>());

                // Preparing arguments, if there are any.
                if (input.Children.Any())
                    lambda.Insert(0, new Node(".arguments", null, input.Children.ToList()));

                // Evaluating lambda of slot.
                await signaler.SignalAsync("wait.eval", lambda);

                // Clearing Children collection, since it might contain input parameters.
                input.Clear();

                /*
                * Simply setting value and children to "slots.result" stack object, since its value
                * was initially set to its old value during startup of method.
                */
                input.Value = result.Value;
                input.AddRange(result.Children.ToList());
            });
        }
    }
}
