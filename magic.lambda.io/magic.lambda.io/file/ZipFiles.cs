/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System.IO;
using System.Linq;
using System.IO.Compression;
using magic.node;
using magic.node.extensions;
using magic.signals.contracts;

namespace magic.lambda.io.file
{
    // TODO: Create [io.file.zip] slot
    /// <summary>
    /// [io.content.zip-stream] slot for zipping a bunch of files into a specified stream.
    /// </summary>
    [Slot(Name = "io.content.zip-stream")]
    public class ZipContent : ISlot
    {
        /// <summary>
        /// Implementation of slot.
        /// </summary>
        /// <param name="signaler">Signaler used to raise the signal.</param>
        /// <param name="input">Arguments to slot.</param>
        public void Signal(ISignaler signaler, Node input)
        {
            // Evaluating file paths node(s).
            signaler.Signal("eval", input);

            // Resulting stream, returned to caller as Value.
            var mem = new MemoryStream();

            /*
             * Creating our ZIP archive, making sure memory stream is not disposed
             * when we finish with it.
             */
            using (var archive = new ZipArchive(mem, ZipArchiveMode.Create, true))
            {
                // Creating one ZIP entry for each argument supplied as child of input.
                foreach (var idx in input.Children)
                {
                    // Evaluating content node.
                    signaler.Signal("eval", idx);

                    // Creating zip entry.
                    var idxEntry = archive.CreateEntry(idx.GetEx<string>());
                    using (var entryStream = idxEntry.Open())
                    {
                        using (var writer = new StreamWriter(entryStream))
                        {
                            /*
                             * Writing the first child's value of currently iterated
                             * input child as content to archive.
                             */
                            writer.Write(idx.Children.FirstOrDefault()?.GetEx<string>() ?? "");
                        }
                    }
                }
            }

            // Cleaning up, and returning MemoryStream to caller.
            input.Clear();
            mem.Seek(0, SeekOrigin.Begin);
            input.Value = mem;
        }
    }
}
