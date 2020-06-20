/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using magic.node;
using magic.signals.contracts;

namespace magic.lambda.json.internals
{
    // TODO: Sanity check. Not entirely sure it actually works for all possible permutations.
    /// <summary>
    /// [.from-json-raw] slot for transforming from a raw Newtonsoft JSON object to a lambda structure,
    /// without having to transform it to a string first.
    /// </summary>
    [Slot(Name = ".json2lambda-raw")]
    public class Json2LambdaRaw : ISlot
    {
        /// <summary>
        /// Slot implementation.
        /// </summary>
        /// <param name="signaler">Signaler that raised signal.</param>
        /// <param name="input">Arguments to slot.</param>
        public void Signal(ISignaler signaler, Node input)
        {
            HandleToken(input, input.Value as JToken);
            input.Value = null;
        }

        #region [ -- Private helper methods -- ]

        static void HandleToken(Node node, JToken token)
        {
            if (token is JArray)
                HandleArray(node, token as JArray);
            else if (token is JObject)
                HandleObject(node, token as JObject);
            else if (token is JValue)
                node.Value = (token as JValue).Value;
        }

        static void HandleObject(Node node, JObject obj)
        {
            foreach (var idx in obj)
            {
                var cur = new Node(idx.Key);
                node.Add(cur);
                HandleToken(cur, idx.Value);
            }
        }

        static void HandleArray(Node node, JArray arr)
        {
            foreach (var idx in arr)
            {
                // Special treatment for JObjects with only one property.
                if (idx is JObject)
                {
                    // Checking if object has only one property.
                    var obj = idx as JObject;
                    if (obj.Count == 1 && obj.First is JProperty jProp)
                    {
                        if (jProp.Value is JValue)
                        {
                            // Object is a simple object with a single value.
                            var prop = obj.First as JProperty;
                            node.Add(new Node(prop.Name, (prop.Value as JValue).Value));
                            continue;
                        }
                        else
                        {
                            // Object is a simple object with multiple properties.
                            var prop = obj.First as JProperty;
                            var tmp = new Node(prop.Name);
                            node.Add(tmp);
                            HandleToken(tmp, prop.Value as JToken);
                            continue;
                        }
                    }
                }
                var cur = new Node(".");
                node.Add(cur);
                HandleToken(cur, idx);
            }
        }
        #endregion
    }
}
