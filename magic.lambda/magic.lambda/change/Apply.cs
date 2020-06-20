/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.Linq;
using System.Collections.Generic;
using magic.node;
using magic.node.extensions;
using magic.signals.contracts;

namespace magic.lambda.change
{
    /// <summary>
    /// [apply-file] slot allowing you to use a Hyperlambda file as a template for braiding together
    /// with variables of your own choosing.
    /// </summary>
    [Slot(Name = "apply")]
    public class ApplyFile : ISlot
    {
        /// <summary>
        /// Implementation of signal
        /// </summary>
        /// <param name="signaler">Signaler used to signal</param>
        /// <param name="input">Parameters passed from signaler</param>
        public void Signal(ISignaler signaler, Node input)
        {
            var args = input.Children.ToList();
            input.Clear();
            foreach (var idxDest in input.Evaluate())
            {
                var destination = idxDest.Clone();
                Apply(args, destination.Children);

                // Returning transformed template to caller.
                input.AddRange(destination.Children.ToList());
            }
            input.Value = null;
        }

        #region [ -- Private helper methods -- ]

        /*
         * Actual implementation that applies lambda object to destination.
         */
        void Apply(IEnumerable<Node> args, IEnumerable<Node> templateNodes)
        {
            foreach (var idx in templateNodes)
            {
                if (idx.Value is string strValue &&
                    strValue.StartsWith("{", StringComparison.InvariantCulture) &&
                    strValue.EndsWith("}", StringComparison.InvariantCulture))
                {
                    // Template variable, finding relevant node from args and applying
                    var templateName = strValue.Substring(1, strValue.Length - 2);
                    var argNode = args.FirstOrDefault(x => x.Name == templateName);
                    if (argNode == null)
                        throw new ApplicationException($"[template] file expected argument named [{templateName}] which was not given");

                    idx.Value = argNode.Value;
                    idx.AddRange(argNode.Children.Select(x => x.Clone()));
                }

                // Recursively invoking self
                Apply(args, idx.Children);
            }
        }

        #endregion
    }
}
