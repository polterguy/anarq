/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System.Linq;
using magic.node;
using magic.node.extensions;
using magic.node.expressions;
using magic.signals.contracts;

namespace magic.lambda.logging.helpers
{
    /*
     * Internal helper class to create a string out of parameters supplied to any one of our logging methods.
     */
    internal static class Helper
    {
        internal static string GetLogInfo(ISignaler signaler, Node input)
        {
            if (!(input.Value is Expression))
                return input.GetEx<string>();
            var xResult = input.Evaluate();
            if (xResult.Count() == 0)
                return "";
            if (xResult.Count() == 1 && xResult.First().Value != null)
                return xResult.First().GetEx<string>();
            else
            {
                var tmpNode = new Node("", null, xResult.Select(x => x.Clone()));
                signaler.Signal("lambda2hyper", tmpNode);
                return tmpNode.GetEx<string>();
            }
        }
    }
}
