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
    /// [io.file.save] slot for saving a file on your server.
    /// </summary>
    [Slot(Name = "io.file.save")]
    [Slot(Name = "wait.io.file.save")]
    public class SaveFile : ISlot, ISlotAsync
    {
        readonly IRootResolver _rootResolver;
        readonly IFileService _service;

        /// <summary>
        /// Constructs a new instance of your type.
        /// </summary>
        /// <param name="rootResolver">Instance used to resolve the root folder of your app.</param>
        /// <param name="service">Underlaying file service implementation.</param>
        public SaveFile(IRootResolver rootResolver, IFileService service)
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
            // Making sure we evaluate any children, to make sure any signals wanting to retrieve our source is evaluated.
            signaler.Signal("eval", input);

            // Saving file.
            _service.Save(
                PathResolver.CombinePaths(
                    _rootResolver.RootFolder,
                    input.GetEx<string>()),
                input.Children.First().GetEx<string>());
        }

        /// <summary>
        /// Implementation of slot.
        /// </summary>
        /// <param name="signaler">Signaler used to raise the signal.</param>
        /// <param name="input">Arguments to slot.</param>
        /// <returns>An awaitable task.</returns>
        public async Task SignalAsync(ISignaler signaler, Node input)
        {
            // Making sure we evaluate any children, to make sure any signals wanting to retrieve our source is evaluated.
            await signaler.SignalAsync("wait.eval", input);

            // Saving file
            await _service.SaveAsync(
                PathResolver.CombinePaths(
                    _rootResolver.RootFolder,
                    input.GetEx<string>()),
                input.Children.First().GetEx<string>());
        }
    }
}
