/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using magic.node;
using magic.node.extensions;
using magic.signals.contracts;
using magic.lambda.io.contracts;
using magic.lambda.io.utilities;

namespace magic.lambda.io.file
{
    /// <summary>
    /// [io.file.move] slot for moving a file on your server.
    /// </summary>
    [Slot(Name = "io.file.move")]
    public class MoveFile : ISlot, ISlotAsync
    {
        readonly IRootResolver _rootResolver;
        readonly IFileService _service;

        /// <summary>
        /// Constructs a new instance of your type.
        /// </summary>
        /// <param name="rootResolver">Instance used to resolve the root folder of your app.</param>
        /// <param name="service">Underlaying file service implementation.</param>
        public MoveFile(IRootResolver rootResolver, IFileService service)
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
            // Sanity checking invocation.
            if (!input.Children.Any())
                throw new ArgumentException("No destination provided to [io.file.move]");

            // Making sure we evaluate any children, to make sure any signals wanting to retrieve our destination is evaluated.
            signaler.Signal("eval", input);

            // Retrieving source path.
            string sourcePath = PathResolver.CombinePaths(
                _rootResolver.RootFolder,
                input.GetEx<string>());

            // Retrieving destination path.
            var destinationPath = PathResolver
                .CombinePaths(
                    _rootResolver.RootFolder,
                    input.Children.First().GetEx<string>());

            // Defaulting detination folder to be the same as source folder, unless a different folder is explicitly given.
            if (destinationPath.EndsWith("/", StringComparison.InvariantCultureIgnoreCase))
                destinationPath += Path.GetFileName(sourcePath);

            // Sanity checking arguments.
            if (sourcePath == destinationPath)
                throw new ArgumentException("You cannot copy a file using the same source and destination path");

            // For simplicity, we're deleting any existing files with the path of the destination file.
            if (_service.Exists(destinationPath))
                _service.Delete(destinationPath);

            _service.Move(sourcePath, destinationPath);
        }

        /// <summary>
        /// Implementation of slot.
        /// </summary>
        /// <param name="signaler">Signaler used to raise the signal.</param>
        /// <param name="input">Arguments to slot.</param>
        public async Task SignalAsync(ISignaler signaler, Node input)
        {
            // Sanity checking invocation.
            if (!input.Children.Any())
                throw new ArgumentException("No destination provided to [io.file.move]");

            // Making sure we evaluate any children, to make sure any signals wanting to retrieve our destination is evaluated.
            await signaler.SignalAsync("wait.eval", input);

            // Retrieving source path.
            string sourcePath = PathResolver.CombinePaths(
                _rootResolver.RootFolder,
                input.GetEx<string>());

            // Retrieving destination path.
            var destinationPath = PathResolver
                .CombinePaths(
                    _rootResolver.RootFolder,
                    input.Children.First().GetEx<string>());

            // Defaulting detination folder to be the same as source folder, unless a different folder is explicitly given.
            if (destinationPath.EndsWith("/", StringComparison.InvariantCultureIgnoreCase))
                destinationPath += Path.GetFileName(sourcePath);

            // Sanity checking arguments.
            if (sourcePath == destinationPath)
                throw new ArgumentException("You cannot copy a file using the same source and destination path");

            // For simplicity, we're deleting any existing files with the path of the destination file.
            if (_service.Exists(destinationPath))
                _service.Delete(destinationPath);

            _service.Move(sourcePath, destinationPath);
        }
    }
}
