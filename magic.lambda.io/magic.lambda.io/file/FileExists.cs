/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.IO;
using magic.node;
using magic.node.extensions;
using magic.signals.contracts;
using magic.lambda.io.contracts;
using magic.lambda.io.utilities;

namespace magic.lambda.io.file
{
    /// <summary>
    /// [io.file.exists] slot for checking if a file already exists from before or not.
    /// </summary>
    [Slot(Name = "io.file.exists")]
    public class FileExists : ISlot
    {
        readonly IRootResolver _rootResolver;
        readonly IFileService _service;

        /// <summary>
        /// Constructs a new instance of your type.
        /// </summary>
        /// <param name="rootResolver">Instance used to resolve the root folder of your app.</param>
        /// <param name="service">Underlaying file service implementation.</param>
        public FileExists(IRootResolver rootResolver, IFileService service)
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
            input.Value = _service.Exists(
                PathResolver.CombinePaths(
                    _rootResolver.RootFolder,
                    input.GetEx<string>()));
        }
    }
}
