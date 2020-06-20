/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using magic.node;

namespace magic.lambda.json.utilities
{
    internal static class Transformer
    {
        public static void TransformToNode(Node node, JContainer container)
        {
            HandleToken(node, container);
        }

        public static JContainer TransformToJSON(Node node)
        {
            return Handle(node) as JContainer;
        }

        #region [ -- Private helper methods -- ]

        #region [ -- Transforming from lambda to JSON helpers -- ]

        static JToken Handle(Node root)
        {
            JToken result = null;
            if (root.Children.Any(x => x.Name.Length > 0 && x.Name != "."))
            {
                // Complex object.
                var resObj = new JObject();
                foreach (var idx in root.Children)
                {
                    resObj.Add(HandleProperty(idx));
                }
                result = resObj;
            }
            else if (root.Children.Any())
            {
                // Array.
                var resArr = new JArray();
                foreach (var idx in root.Children)
                {
                    resArr.Add(HandleArray(idx));
                }
                result = resArr;
            }
            else
            {
                result = new JObject();
            }

            return result;
        }

        static JToken HandleArray(Node idx)
        {
            if (idx.Children.Any())
            {
                return Handle(idx);
            }
            else
            {
                var value = idx.Value;
                if (value is DateTime dateValue)
                    value = new DateTimeOffset(dateValue);
                return new JValue(value);
            }
        }

        static JProperty HandleProperty(Node idx)
        {
            if (idx.Children.Any())
                return new JProperty(idx.Name, Handle(idx));
            var value = idx.Value;

            // Notice, for JSON we want to return dates as Zulu!
            if (value is DateTime dateValue)
                value = dateValue.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");
            return new JProperty(idx.Name, value);
        }

        #endregion

        #region [ -- Transforming from JSON to Node helpers -- ]

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
                var cur = new Node();
                node.Add(cur);
                HandleToken(cur, idx);
            }
        }

        #endregion

        #endregion
    }
}
