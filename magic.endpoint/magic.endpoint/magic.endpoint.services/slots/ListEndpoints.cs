/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using magic.node;
using magic.node.extensions;
using magic.signals.contracts;
using magic.endpoint.services.utilities;
using magic.node.extensions.hyperlambda;

namespace magic.endpoint.services.slots
{
    /// <summary>
    /// [system.endpoints] slot for returning all dynamica Hyperlambda endpoints
    /// for your application.
    /// </summary>
    [Slot(Name = "endpoints.list")]
    public class ListEndpoints : ISlot
    {
        /// <summary>
        /// Implementation of your slot.
        /// </summary>
        /// <param name="signaler">Signaler that invoked your slot.</param>
        /// <param name="input">Arguments to your slot.</param>
        public void Signal(ISignaler signaler, Node input)
        {
            /*
             * Retrieving user credentials before we start process, such that we can avoid
             * returning endpoints the user doesn't have access to.
             */
            var node = new Node("");
            signaler.Signal("auth.ticket.get", node);
            var roles = node.Children.Select(x => x.GetEx<string>()).ToArray();
            input.AddRange(AddCustomEndpoints(
                roles,
                Utilities.RootFolder,
                Utilities.RootFolder + "modules/").ToList());
        }

        #region [ -- Private helper methods -- ]

        /*
         * Recursively traverses your folder for any dynamic Hyperlambda
         * endpoints, and returns the result to caller.
         */
        IEnumerable<Node> AddCustomEndpoints(string[] roles, string rootFolder, string currentFolder)
        {
            // Looping through each folder inside of "currentFolder".
            var folders = Directory
                .GetDirectories(currentFolder)
                .Select(x => x.Replace("\\", "/")).ToList();
            folders.Sort();
            foreach (var idxFolder in folders)
            {
                // Making sure files within this folder is legally resolved.
                var folder = idxFolder.Substring(rootFolder.Length);
                if (Utilities.IsLegalHttpName(folder))
                {
                    // Retrieves all files inside of currently iterated folder.
                    foreach (var idxFile in GetDynamicFiles(roles, rootFolder, idxFolder))
                    {
                        yield return idxFile;
                    }

                    // Recursively retrieving inner folders of currently iterated folder.
                    foreach (var idx in AddCustomEndpoints(roles, rootFolder, idxFolder))
                    {
                        yield return idx;
                    }
                }
            }
        }

        /*
         * Returns all fildes from current folder that matches some HTTP verb.
         */
        IEnumerable<Node> GetDynamicFiles(string[] roles, string rootFolder, string folder)
        {
            /*
             * Retrieving all Hyperlambda files inside of folder, making sure we
             * substitute all "windows slashes" with forward slash.
             */
            var folderFiles = Directory.GetFiles(folder, "*.hl").Select(x => x.Replace("\\", "/")).ToList();
            folderFiles.Sort();

            // Looping through each file in currently iterated folder.
            foreach (var idxFile in folderFiles)
            {
                /*
                 * This will remove the root folder parts of the path to the file,
                 * which we're not interested in.
                 */
                var filename = idxFile.Substring(rootFolder.Length);

                /*
                 * Verifying this is an HTTP file, which implies it must
                 * have the structure of "path.HTTP-VERB.hl", for instance "foo.get.hl".
                 */
                var entities = filename.Split('.');
                if (entities.Length == 3)
                {
                    // Returning a Node representing the currently iterated file.
                    switch (entities[1])
                    {
                        case "delete":
                        case "put":
                        case "post":
                        case "get":
                            var tmp = GetPath(roles, entities[0], entities[1], idxFile);
                            if (tmp != null)
                                yield return tmp;
                            break;
                    }
                }
            }
            yield break;
        }

        /*
         * Returns a single node, representing the endpoint given
         * as verb/filename/path, and its associated meta information.
         */
        Node GetPath(string[] roles, string path, string verb, string filename)
        {
            /*
             * Creating our result node, and making sure we return path and verb.
             */
            var result = new Node("");
            result.Add(new Node("path", "magic/" + path.Replace("\\", "/"))); // Must add "Route" parts.
            result.Add(new Node("verb", verb));

            /*
             * Reading the file, to figure out what type of authorization the
             * currently traversed endpoint has.
             */
            using (var stream = File.OpenRead(filename))
            {
                var lambda = new Parser(stream).Lambda();
                foreach (var idx in lambda.Children)
                {
                    if (idx.Name == "auth.ticket.verify")
                    {
                        if (roles.Length == 0)
                            return null; // User is not authenticated at all, and endpoint requires authentication.

                        var auth = new Node("auth");
                        var hasRole = false;
                        foreach (var idxRole in idx.GetEx<string>()?.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>())
                        {
                            var role = idxRole.Trim();
                            auth.Add(new Node("", role));
                            if (!hasRole && roles.Contains(role))
                                hasRole = true;
                        }
                        if (!hasRole && auth.Children.Any())
                            return null; // Current user is not authenticated to see this endpoint!

                        result.Add(auth);
                    }
                }

                // Then figuring out the endpoints input arguments, if any.
                var args = lambda.Children.FirstOrDefault(x => x.Name == ".arguments");
                if (args != null)
                {
                    // Endpoint have declared its input arguments.
                    var argsNode = new Node("input");
                    argsNode.AddRange(args.Children.Select(x => x.Clone()));
                    result.Add(argsNode);
                }

                // Then figuring out the endpoints input description, if any.
                var descriptionNode = lambda.Children.FirstOrDefault(x => x.Name == ".description");
                if (descriptionNode != null)
                {
                    // Endpoint have a descriptive node.
                    result.Add(new Node("description", descriptionNode.GetEx<string>()));
                }

                // Then checking to see if this is a dynamically created CRUD wrapper endpoint.
                var slotNode = lambda.Children.LastOrDefault(x => x.Name == "wait.signal");
                if (slotNode != null && slotNode.Children.Any(x => x.Name == "database") && slotNode.Children.Any(x => x.Name == "table"))
                {
                    /*
                     * This is a database CRUD HTTP endpoint, now figuring out what type of endpoint it is.
                     */
                    switch (verb)
                    {
                        case "get":
                            if (slotNode.Children.Any(x => x.Name == "columns"))
                            {
                                var resultNode = new Node("returns");
                                if (slotNode.Children.First(x => x.Name == "columns").Children.Any(x => x.Name == "count(*) as count"))
                                {
                                    resultNode.Add(new Node("count", "long"));
                                    result.Add(resultNode);
                                    result.Add(new Node("array", false));
                                    result.Add(new Node("type", "crud-count"));
                                }
                                else
                                {
                                    resultNode.AddRange(slotNode.Children.First(x => x.Name == "columns").Children.Select(x => x.Clone()));
                                    if (args != null)
                                    {
                                        foreach (var idx in resultNode.Children)
                                        {
                                            // Doing lookup for [.arguments][xxx.eq] to figure out type of object.
                                            idx.Value = args.Children.FirstOrDefault(x => x.Name == idx.Name + ".eq")?.Value;
                                        }
                                    }
                                    result.Add(resultNode);
                                    result.Add(new Node("array", true));
                                    result.Add(new Node("type", "crud-read"));
                                }
                            } break;

                        case "post":
                            result.Add(new Node("type", "crud-create"));
                            break;

                        case "put":
                            result.Add(new Node("type", "crud-update"));
                            break;

                        case "delete":
                            result.Add(new Node("type", "crud-delete"));
                            break;
                    }
                }
                else
                {
                    // Checking if this is a Custom SQL type of endpoint.
                    var sqlConnectNode = lambda.Children.LastOrDefault(x => x.Name == "wait.mysql.connect" || x.Name == "wait.mssql.connect");
                    if (sqlConnectNode != null)
                    {
                        // Checking if this has a x.select type of node of some sort.
                        var sqlSelectNode = sqlConnectNode.Children.LastOrDefault(x => x.Name.EndsWith(".select"));
                        if (sqlSelectNode != null)
                        {
                            // Checking if this is a statistics type of endpoint.
                            if (lambda.Children.FirstOrDefault(x => x.Name == ".is-statistics")?.Get<bool>() ?? false)
                            {
                                // This is a Custom SQL type of endpoint of some sort.
                                result.Add(new Node("type", "crud-statistics"));
                            }
                            else
                            {
                                // This is a Custom SQL type of endpoint of some sort.
                                result.Add(new Node("type", "crud-sql"));
                            }
                        }
                    }
                }
            }

            // Returning results to caller.
            return result;
        }

        #endregion
    }
}
