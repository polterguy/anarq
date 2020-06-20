/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using log4net;
using Newtonsoft.Json.Linq;
using magic.io.services;
using magic.io.contracts;
using magic.http.services;
using magic.http.contracts;
using magic.signals.services;
using magic.signals.contracts;
using magic.endpoint.services;
using magic.library.internals;
using magic.endpoint.contracts;
using magic.lambda.io.contracts;
using magic.lambda.auth.services;
using magic.lambda.mime.services;
using magic.lambda.auth.contracts;
using magic.lambda.mime.contracts;
using magic.lambda.scheduler.utilities;
using magic.node.extensions.hyperlambda;
using magic.endpoint.services.utilities;

namespace magic.library
{
    /// <summary>
    /// Magic initialization class, to help you initialize Magic with
    /// some sane defaults.
    /// </summary>
    public static class Initializer
    {
        static ILog _logger;

        /// <summary>
        /// Convenience method that wires up all Magic components with their
        /// default settings.
        /// </summary>
        /// <param name="services">Your service collection.</param>
        /// <param name="configuration">The configuration for your app.</param>
        /// <param name="licenseKey">The license key associated with
        /// your server.</param>
        public static void AddMagic(
            this IServiceCollection services,
            IConfiguration configuration,
            string licenseKey = null)
        {
            services.AddMagicHttp();
            services.AddMagicLog4netServices();
            services.AddMagicSignals(licenseKey);
            services.AddMagicEndpoints(configuration);
            services.AddMagicFileServices(configuration);
            services.AddMagicAuthorization(configuration);
            services.AddMagicScheduler(configuration);
            services.AddMagicMail();
        }

        /// <summary>
        /// Adds the Magic Scheduler to your application
        /// </summary>
        /// <param name="services">Your service collection.</param>
        /// <param name="configuration">The configuration for your app.</param>
        public static void AddMagicScheduler(this IServiceCollection services, IConfiguration configuration)
        {
            var tasksPath = configuration["magic:scheduler:tasks-file"];
            if (tasksPath == null)
                tasksPath = configuration["magic:scheduler:tasks-folder"] ?? "~/tasks.hl";
            tasksPath = tasksPath.Replace("~", Directory.GetCurrentDirectory().Replace("\\", "/").TrimEnd('/'));
            var autoStart = bool.Parse(configuration["magic:scheduler:auto-start"] ?? "false");
            var maxThreads = int.Parse(configuration["magic:scheduler:max-threads"] ?? "4");
            var logger = new TaskLogger();
            services.AddSingleton(svc => new Scheduler(svc, logger, tasksPath, autoStart, maxThreads));
        }

        /// <summary>
        /// Making sure Magic is using log4net as its logger implementation.
        /// Notice, this method depends upon a "log4net.config" configuration
        /// file existing on root on your disc.
        /// </summary>
        /// <param name="services">Your service collection.</param>
        public static void AddMagicLog4netServices(this IServiceCollection services)
        {
            /*
             * Assuming "log4net.config" exists on root folder of application.
             */
            var configurationFile = string.Concat(
                AppDomain.CurrentDomain.BaseDirectory,
                "log4net.config");

            /*
             * Loading "log4net.config" as an XML file, and using it to
             * configure log4net.
             */
            var log4netConfig = new XmlDocument();
            log4netConfig.Load(File.OpenRead(configurationFile));
            var repo = LogManager.CreateRepository(
                Assembly.GetEntryAssembly(),
                typeof(log4net.Repository.Hierarchy.Hierarchy));
            log4net.Config.XmlConfigurator.Configure(repo, log4netConfig["log4net"]);

            /*
             * Logging the fact that log4net was successfully wired up.
             */
            _logger = LogManager.GetLogger(typeof(Initializer));
            _logger.Info("Initializing log4net for Magic");

            /*
             * Associating magic.lambda.logging's ILog service contract with
             * our internal "Logger" class, which is the class actually logging
             * entries, when for instance the [log.info] slot is invoked.
             */
            services.AddTransient<lambda.logging.helpers.ILog, Logger>();
        }

        /// <summary>
        /// Making sure Magic is able to invoke HTTP REST endpoints.
        /// </summary>
        /// <param name="services">Your service collection.</param>
        public static void AddMagicHttp(this IServiceCollection services)
        {
            services.AddTransient<IHttpClient, HttpClient>();
        }

        /// <summary>
        /// Wires up magic.io to use the default file service, and the
        /// AuthorizeOnlyRole authorization scheme, to only allow the "root"
        /// role to read and write files from your server using magic.io.
        /// </summary>
        /// <param name="services">Your service collection.</param>
        /// <param name="configuration">The configuration for your app.</param>
        /// <param name="ensureFolder">If true, will automatically create
        /// the Magic folder where Magic stores dynamically created files.</param>
        public static void AddMagicFileServices(
            this IServiceCollection services,
            IConfiguration configuration,
            bool ensureFolder = false)
        {
            /*
             * Logging.
             */
            _logger?.Info("Using default magic.io services, and only allowing 'root' to use magic.io");

            /*
             * Associating the IFileServices with its default implementation.
             */
            services.AddTransient<io.contracts.IFileService, FileService>();

            /*
             * Associating the IFileServices and IFolderService with its default implementation.
             */
            services.AddTransient<lambda.io.contracts.IFileService, magic.lambda.io.file.services.FileService>();
            services.AddTransient<lambda.io.contracts.IFolderService, magic.lambda.io.folder.services.FolderService>();

            /*
             * Making sure magic.io can only be used by "root" roles.
             */
            services.AddSingleton<IAuthorize>((svc) => new AuthorizeOnlyRoles("root"));

            /*
             * Associating the root folder resolver with our own internal class,
             * that resolves the root folder for magic.io to be the config
             * setting "magic:io:root-folder", or if not given "/files".
             */
            services.AddTransient<IRootResolver, RootResolver>();

            /*
             * Checking if caller wants us to automatically create folder for
             * dynamic files if it doesn't already exist.
             */
            if (ensureFolder)
            {
                // Making sure the folder for dynamic files exists on server.
                var rootFolder = (configuration["magic:io:root-folder"] ?? "~/files/")
                    .Replace("~", Directory.GetCurrentDirectory())
                    .TrimEnd('/') + "/";

                // Ensuring root folder for dynamic files exists on server.
                if (!Directory.Exists(rootFolder))
                    Directory.CreateDirectory(rootFolder);
            }
        }

        /// <summary>
        /// Configures magic.lambda.auth and turning on authentication
        /// and authorization.
        /// </summary>
        /// <param name="services">Your service collection.</param>
        /// <param name="configuration">The configuration for your app.</param>
        public static void AddMagicAuthorization(this IServiceCollection services, IConfiguration configuration)
        {
            _logger?.Info("Configures magic.lambda.auth to use its default ticket provider");

            /*
             * Configures magic.lambda.auth to use its default implementation of
             * HttpTicketFactory as its authentication and authorization
             * implementation.
             */
            services.AddTransient<ITicketProvider, HttpTicketProvider>();

            /*
             * Parts of Magic depends upon having access to
             * the IHttpContextAccessor, more speifically the above
             * HttpTicketProvider service implementation.
             */
            services.AddHttpContextAccessor();

            /*
             * Retrieving secret from configuration file, and wiring up
             * authentication to use JWT Bearer tokens.
             */
            var secret = configuration["magic:auth:secret"];
            if (string.IsNullOrEmpty(secret))
                throw new ApplicationException("Couldn't find any 'magic:auth:secret' configuration settings in your appSettings.json file. Magic can never be secure unless you provide this configuration setting.");

            /*
             * Wiring up .Net Core to use JWT Bearer tokens for auth.
             */
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    /*
                     * Notice, making sure we retrieve secret for each time it's needed.
                     * This will make it possible to change the secret without having to restart the web app.
                     */
                    IssuerSigningKeyResolver = (token, secToken, kid, valParams) =>
                    {
                        var key = Encoding.ASCII.GetBytes(configuration["magic:auth:secret"]);
                        return new List<SecurityKey>() { new SymmetricSecurityKey(key) };
                    }
                };
            });
        }

        /// <summary>
        /// Configures magic.endpoint to use its default service implementation.
        /// </summary>
        /// <param name="services">Your service collection.</param>
        /// <param name="configuration">Your apps configuration.</param>
        public static void AddMagicEndpoints(this IServiceCollection services, IConfiguration configuration)
        {
            _logger?.Info("Configures magic.endpoint to use its default executor");

            /*
             * Figuring out which folder to resolve dynamic Hyperlambda files from,
             * and making sure we configure the Hyperlambda resolver to use the correct
             * folder.
             */
            var rootFolder = configuration["magic:endpoint:root-folder"] ?? "~/files/";
            rootFolder = rootFolder
                .Replace("~", Directory.GetCurrentDirectory())
                .Replace("\\", "/");
            Utilities.RootFolder = rootFolder;

            // Configuring the default executor to execute dynamic URLs.
            services.AddTransient<IExecutorAsync>(svc => new ExecutorAsync(svc.GetRequiredService<ISignaler>(), configuration));
        }

        /// <summary>
        /// Adds Magic Mail to your application
        /// </summary>
        /// <param name="services">Your service collection.</param>
        public static void AddMagicMail(this IServiceCollection services)
        {
            services.AddTransient<ISmtpClient, SmtpClient>();
            services.AddTransient<IPop3Client, Pop3Client>();
        }

        /// <summary>
        /// Configures magic.signals such that you can signal slots.
        /// </summary>
        /// <param name="services">Your service collection.</param>
        /// <param name="licenseKey">The license key associated with
        /// your installation.</param>
        public static void AddMagicSignals(this IServiceCollection services, string licenseKey = null)
        {
            /*
             * Loading all assemblies that are not loaded up for some reasons.
             */
            var loadedPaths = AppDomain.CurrentDomain
                .GetAssemblies()
                .Select(x => x.Location);

            var assemblyPaths = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll");
            foreach (var idx in assemblyPaths.Where(x => !loadedPaths.Contains(x, StringComparer.InvariantCultureIgnoreCase)))
            {
                _logger?.Info($"Dynamically loading assembly '{Path.GetFileName(idx)}'");
                AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(idx));
            }

            /*
             * Using the default ISignalsProvider and the default ISignals
             * implementation.
             */
            _logger?.Info("Configuring magic.signals to use its default service implementations");
            services.AddTransient<ISignaler, Signaler>();
            services.AddSingleton<ISignalsProvider>(new SignalsProvider(Slots(services)));

            /*
             * Checking if caller supplied a license key.
             */
            if (!string.IsNullOrEmpty(licenseKey) && licenseKey != "SUPPLY-YOUR-LICENSE-KEY-HERE") // Default value in config file ...
                Signaler.SetLicenseKey(licenseKey);
        }

        /// <summary>
        /// Convenience method to make sure you use all Magic features.
        /// </summary>
        /// <param name="app">The application builder of your app.</param>
        /// <param name="configuration">The configuration for your app.</param>
        public static void UseMagic(
            this IApplicationBuilder app,
            IConfiguration configuration)
        {
            app.UseMagicExceptions();
            app.UseMagicStartupFiles(configuration);
        }

        /// <summary>
        /// Traps all unhandled exceptions, logs them to your log files using
        /// log4net (if configured), and returns the exception message back
        /// to the client as a JSON response.
        /// </summary>
        /// <param name="app">The application builder of your app.</param>
        public static void UseMagicExceptions(this IApplicationBuilder app)
        {
            /*
             * Making sure we're storing errors into our log file, and that
             * we're able to return the exception message to client.
             */
            app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
            {
                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";
                var ex = context.Features.Get<IExceptionHandlerPathFeature>();
                if (ex != null)
                {
                    var msg = ex.Error.Message ?? ex.GetType().FullName;
                    if (_logger != null)
                    {
                        _logger.Error("At path: " + ex.Path);
                        _logger.Error(msg, ex.Error);
                    }
#if DEBUG
                    var response = new JObject
                    {
                        ["message"] = msg,
                        ["stack-trace"] = ex.Error.StackTrace,
                    };
#else
                    var response = new JObject
                    {
                        ["message"] = "Guru meditation, come back when Universe is in order!"
                    };
#endif
                    await context.Response.WriteAsync(
                        response.ToString(Newtonsoft.Json.Formatting.Indented));
                }
                else
                {
                    var response = new JObject
                    {
                        ["message"] = "Unknown error",
                    };
                    await context.Response.WriteAsync(
                        response.ToString(Newtonsoft.Json.Formatting.Indented));
                }
            }));
        }

        /// <summary>
        /// Evaluates all Magic Hyperlambda module startup files, that are
        /// inside Magic's dynamic "modules/xxx/magic.startup/" folder, 
        /// </summary>
        /// <param name="app">Your application builder.</param>
        /// <param name="configuration">Your app's configuration.</param>
        public static void UseMagicStartupFiles(
            this IApplicationBuilder app,
            IConfiguration configuration)
        {
            // Creating a signaler and figuring out root path for dynamic Hyperlambda files.
            var signaler = app.ApplicationServices.GetService<ISignaler>();
            var rootFolder = (configuration["magic:io:root-folder"] ?? "~/files/")
                .Replace("~", Directory.GetCurrentDirectory())
                .TrimEnd('/') + "/";

            // Retrieving all folders inside of our "/modules/" folder.
            foreach (var idxModules in Directory.GetDirectories(rootFolder + "modules/"))
            {
                // Finding all folders inside of the currently iterated folder inside of "/modules/".
                foreach (var idxModuleFolder in Directory.GetDirectories(idxModules))
                {
                    /*
                     * Checking if this is a "startup folder", at which point we
                     * execute all Hyperlambda files (recursively) inside of it.
                     */
                    var folder = new DirectoryInfo(idxModuleFolder);
                    if (folder.Name == "magic.startup")
                        ExecuteStartupFiles(signaler, idxModuleFolder);
                }
            }
        }

#region [ -- Private helper methods -- ]

        /*
         * Will recursively execute every single Hyperlambda file inside of
         * the specified folder.
         */
        static void ExecuteStartupFiles(ISignaler signaler, string folder)
        {
            // Startup folder, now executing all Hyperlambda files inside of it.
            foreach (var idxFile in Directory.GetFiles(folder, "*.hl"))
            {
                using (var stream = File.OpenRead(idxFile))
                {
                    var lambda = new Parser(stream).Lambda();
                    signaler.Signal("eval", lambda);
                }
            }

            // Recursively checking sub folders.
            foreach (var idxFolder in Directory.GetDirectories(folder))
            {
                ExecuteStartupFiles(signaler, idxFolder);
            }
        }

        /*
         * Finds all types in AppDomain that implements ISlot and that is not
         * abstract. Adds all these as transient services, and returns all of
         * these types to caller.
         */
        static IEnumerable<Type> Slots(IServiceCollection services)
        {
            var type1 = typeof(ISlot);
            var type2 = typeof(ISlotAsync);
            var result = AppDomain.CurrentDomain.GetAssemblies()
                .Where(x => !x.IsDynamic && !x.FullName.StartsWith("Microsoft", StringComparison.InvariantCulture))
                .SelectMany(s => s.GetTypes())
                .Where(p => (type1.IsAssignableFrom(p) || type2.IsAssignableFrom(p)) && !p.IsInterface && !p.IsAbstract);

            foreach (var idx in result)
            {
                services.AddTransient(idx);
            }
            return result;
        }

#endregion
    }
}
