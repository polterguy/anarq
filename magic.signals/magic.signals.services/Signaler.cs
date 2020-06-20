/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Cryptography;
using magic.node;
using magic.signals.contracts;

namespace magic.signals.services
{
    /// <summary>
    /// Default implementation service class for the ISignaler contract/interface.
    /// </summary>
    public class Signaler : ISignaler
    {
        static bool _validLicense;
        static readonly DateTime _stopTime = DateTime.Now.AddHours(5);

        readonly IServiceProvider _provider;
        readonly ISignalsProvider _signals;
        readonly List<Tuple<string, object>> _stack = new List<Tuple<string, object>>();

        /// <summary>
        /// Creates a new instance of the default ISignaler service class.
        /// </summary>
        /// <param name="provider">Service provider to use for retrieving services.</param>
        /// <param name="signals">Implementation class to use for retrieving
        /// types from their string representations.</param>
        public Signaler(IServiceProvider provider, ISignalsProvider signals)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _signals = signals ?? throw new ArgumentNullException(nameof(signals));
        }

        /// <summary>
        /// Sets your license key for current installation.
        ///
        /// Notice, without a valid license key, Magic will stop functioning
        /// after 5 hours.
        /// </summary>
        /// <param name="key">Your license key, as obtained from Server Gardens.</param>
        static public void SetLicenseKey(string key)
        {
            // Checking if license key is valid.
            var licenseEntities = key.Split(':');
            if (licenseEntities.Length != 2)
                throw new ApplicationException("Your license must contain your domain (hostname/DNS entry) and your actual key, separated by ':', e.g. 'api.some-website.com:xxxxxxx'.");

            /*
             * Salting hostname parts of license key, hashing it, and
             * comparing it to the license key of the license key.
             */
            using (var sha = SHA256.Create())
            {
                var hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(licenseEntities[0] + "thomas hansen is cool"));
                var hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                if (hash == licenseEntities[1])
                    _validLicense = true; // License is valid!
                else
                    throw new ApplicationException("Your license is not valid, it must contain your domain (hostname/DNS entry) and your actual key, separated by ':', e.g. 'api.some-website.com:xxxxxxx', and it mist have been obtained from https://servergardens.com/buy/");
            }
        }

        #region [ -- Interface implementation -- ]

        /// <summary>
        /// Invokes the slot with the specified name,
        /// passing in the node itself as arguments to the slot.
        /// </summary>
        /// <param name="name">Name of slot to invoke.</param>
        /// <param name="input">Arguments being passed in to slot.</param>
        public void Signal(string name, Node input)
        {
            var type = _signals.GetSlot(name) ?? throw new ApplicationException($"No slot exists for [{name}]");
            var raw = _provider.GetService(type);

            // Basic sanity checking.
            if (!(raw is ISlot slot))
            {
                if (raw is ISlotAsync)
                    throw new ApplicationException($"The [{name}] slot is an async slot, and you tried to invoke it synchronously. Please invoke it using SignalAsync instead.");
                throw new ApplicationException($"I couldn't find the [{name}] slot, have you registered it?");
            }

            slot.Signal(this, input);
        }

        /// <summary>
        /// Invokes the slot with the specified name,
        /// passing in the node itself as arguments to the slot.
        /// Notice, the ISlotAsync interface must have been implemented on your type
        /// to signal it using the async Signal method.
        /// </summary>
        /// <param name="name">Name of slot to invoke.</param>
        /// <param name="input">Arguments being passed in to slot.</param>
        /// <returns>An awaitable task.</returns>
        public async Task SignalAsync(string name, Node input)
        {
            var type = _signals.GetSlot(name) ?? throw new ApplicationException($"No slot exists for [{name}]");
            var raw = _provider.GetService(type);

            // Basic sanity checking.
            if (!(raw is ISlotAsync asyncSlot))
            {
                if (raw is ISlot)
                    throw new ApplicationException($"The [{name}] slot is not an async slot, and you tried to invoke it as such. Please invoke it synchronously.");
                throw new ApplicationException($"I couldn't find the [{name}] slot, have you registered it?");
            }

            await asyncSlot.SignalAsync(this, input);
        }

        /// <summary>
        /// Pushes the specified object unto the stack with the given key name,
        /// for then to evaluate the given functor. Useful for evaluating some piece of code
        /// making sure the evaluation has access to some stack object during its evaluation process.
        /// </summary>
        /// <param name="name">Name to push value unto the stack as.</param>
        /// <param name="value">Actual object to push unto the stack. Notice, object will be automatically disposed at
        /// the end of the scope if the object implements IDisposable.</param>
        /// <param name="functor">Callback evaluated while stack object is on the stack.</param>
        public void Scope(string name, object value, Action functor)
        {
            _stack.Add(new Tuple<string, object>(name, value));
            try
            {
                functor();
            }
            finally
            {
                var obj = _stack[_stack.Count - 1];
                _stack.Remove(obj);
                if (obj is IDisposable disp)
                    disp.Dispose();
            }
        }

        /// <summary>
        /// Pushes the specified object unto the stack with the given key name,
        /// for then to evaluate the given functor. Useful for evaluating some
        /// piece of code making sure the evaluation has access to some stack
        /// object during its evaluation process.
        /// </summary>
        /// <param name="name">Name to push value unto the stack as.</param>
        /// <param name="value">Actual object to push unto the stack. Notice,
        /// object will be automatically disposed at the end of the scope if
        /// the object implements IDisposable.</param>
        /// <param name="functor">Callback evaluated while stack object is on
        /// the stack.</param>
        /// <returns>An awaitable task.</returns>
        public async Task ScopeAsync(string name, object value, Func<Task> functor)
        {
            _stack.Add(new Tuple<string, object>(name, value));
            try
            {
                await functor();
            }
            finally
            {
                var obj = _stack[_stack.Count - 1];
                _stack.Remove(obj);
                if (obj is IDisposable disp)
                    disp.Dispose();
            }
        }

        /// <summary>
        /// Retrieves the last stack object pushed unto the stack with the
        /// specified name.
        /// </summary>
        /// <typeparam name="T">Type to return stack object as. Notice, no
        /// conversion will be attempted. Make sure you use the correct type
        /// when retrieving your stack object.</typeparam>
        /// <param name="name">Name stack object was pushed as.</param>
        /// <returns>The first stack object with the specified name, or null if
        /// none are found.</returns>
        public T Peek<T>(string name) where T : class
        {
            return _stack.AsEnumerable().Reverse().FirstOrDefault(x => x.Item1 == name)?.Item2 as T;
        }

        #endregion
    }
}
