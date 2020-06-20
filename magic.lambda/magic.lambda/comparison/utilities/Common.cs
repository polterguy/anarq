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

namespace magic.lambda.comparison.utilities
{
    /*
     * Helper class containing commonalities for comparison slots.
     */
    internal static class Common
    {
        internal static void Compare(
            ISignaler signaler,
            Node input,
            Func<object, object, bool> functor)
        {
            if (input.Children.Count() != 2)
                throw new ApplicationException($"Comparison operation [{input.Name}] requires exactly two operands");

            signaler.Signal("eval", input);

            input.Value = functor(
                input.Children.First().GetEx<object>(),
                input.Children.Skip(1).First().GetEx<object>());
        }

        internal async static Task CompareAsync(
            ISignaler signaler,
            Node input,
            Func<object, object, bool> functor)
        {
            if (input.Children.Count() != 2)
                throw new ApplicationException($"Comparison operation [{input.Name}] requires exactly two operands");

            await signaler.SignalAsync("wait.eval", input);

            input.Value = functor(
                input.Children.First().GetEx<object>(),
                input.Children.Skip(1).First().GetEx<object>());
        }
    }
}
