/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.Threading.Tasks;
using magic.node;
using magic.node.extensions;
using magic.signals.contracts;
using magic.lambda.io.contracts;
using magic.lambda.io.utilities;
using magic.node.extensions.hyperlambda;
using System.Linq;

namespace magic.lambda.io.file
{
    /// <summary>
    /// [io.file.execute] slot for executing a Hyperlambda file on your server.
    /// </summary>
    [Slot(Name = "io.file.execute")]
    [Slot(Name = "wait.io.file.execute")]
    [Slot(Name = "io.file.eval")]
    [Slot(Name = "wait.io.file.eval")]
    public class ExecuteFile : ISlot, ISlotAsync
    {
        readonly IRootResolver _rootResolver;
        readonly IFileService _service;

        /// <summary>
        /// Constructs a new instance of your type.
        /// </summary>
        /// <param name="rootResolver">Instance used to resolve the root folder of your app.</param>
        /// <param name="service">Underlaying file service implementation.</param>
        public ExecuteFile(IRootResolver rootResolver, IFileService service)
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
            // Making sure we're able to handle returned values and nodes from slot implementation.
            var result = new Node();
            signaler.Scope("slots.result", result, () =>
            {
                // Loading file and converting its content to lambda.
                var hyperlambda = _service
                    .Load(
                        PathResolver.CombinePaths(
                            _rootResolver.RootFolder,
                            input.GetEx<string>()));
                var lambda = new Parser(hyperlambda).Lambda();

                // Preparing arguments, if there are any, making sure we remove any declarative [.arguments] first.
                lambda.Children
                    .FirstOrDefault(x => x.Name == ".arguments")?
                    .UnTie();
                if (input.Children.Any())
                    lambda.Insert(0, new Node(".arguments", null, input.Children.ToList()));

                // Evaluating lambda of slot.
                signaler.Signal("eval", lambda);

                // Clearing Children collection, since it might contain input parameters.
                input.Clear();

                /*
                * Simply setting value and children to "slots.result" stack object, since its value
                * was initially set to its old value during startup of method.
                */
                input.Value = result.Value;
                input.AddRange(result.Children.ToList());
            });
        }

        /// <summary>
        /// Implementation of slot.
        /// </summary>
        /// <param name="signaler">Signaler used to raise the signal.</param>
        /// <param name="input">Arguments to slot.</param>
        /// <returns>An awaitable task.</returns>
        public async Task SignalAsync(ISignaler signaler, Node input)
        {
            // Making sure we're able to handle returned values and nodes from slot implementation.
            var result = new Node();
            await signaler.ScopeAsync("slots.result", result, async () =>
            {
                // Loading file and converting its content to lambda.
                var hyperlambda = await _service.LoadAsync(
                    PathResolver.CombinePaths(
                        _rootResolver.RootFolder,
                        input.GetEx<string>()));
                var lambda = new Parser(hyperlambda).Lambda();

                // Preparing arguments, if there are any.
                if (input.Children.Any())
                    lambda.Insert(0, new Node(".arguments", null, input.Children.ToList()));

                // Evaluating lambda of slot.
                await signaler.SignalAsync("wait.eval", lambda);

                // Clearing Children collection, since it might contain input parameters.
                input.Clear();

                /*
                * Simply setting value and children to "slots.result" stack object, since its value
                * was initially set to its old value during startup of method.
                */
                input.Value = result.Value;
                input.AddRange(result.Children.ToList());
            });
        }
    }
}
