/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.IO;
using System.Linq;
using magic.node;
using magic.node.extensions;
using magic.signals.contracts;
using magic.lambda.io.contracts;
using magic.lambda.io.utilities;

namespace magic.lambda.io.folder
{
    /// <summary>
    /// [io.folder.list] slot for listing folders on server.
    /// </summary>
    [Slot(Name = "io.folder.list")]
    public class ListFolders : ISlot
    {
        readonly IRootResolver _rootResolver;
        readonly IFolderService _service;

        /// <summary>
        /// Constructs a new instance of your type.
        /// </summary>
        /// <param name="rootResolver">Instance used to resolve the root folder of your app.</param>
        /// <param name="service">Underlaying file service implementation.</param>
        public ListFolders(IRootResolver rootResolver, IFolderService service)
        {
            _rootResolver = rootResolver ?? throw new ArgumentNullException(nameof(rootResolver));
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        /// <summary>
        /// Implementation of slot.
        /// </summary>
        /// <param name="signaler">Signaler used to raise the signal.</param>
        /// <param name="input">Arguments to slot.</param>
        public void Signal(ISignaler signaler, Node input)
        {
            var displayHiddenFolders = input.Children
                .FirstOrDefault(x => x.Name == "display-hidden")?
                .GetEx<bool>() ?? false;
            var root = PathResolver.Normalize(_rootResolver.RootFolder);
            var folder = input.GetEx<string>();
            input.Clear();
            var folders = _service
                .ListFolders(
                    PathResolver.CombinePaths(
                        _rootResolver.RootFolder,
                        folder))
                .ToList();
            folders.Sort();
            foreach (var idx in folders)
            {
                // Making sure we don't show hidden operating system folders by default.
                if (!displayHiddenFolders && !idx.StartsWith("."))
                    input.Add(new Node("", idx.Substring(root.Length).TrimEnd('/') + "/"));
            }
        }
    }
}
