/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace magic.backend
{
    public class Program
    {
        /*
         * To use Development environment during launch, make sure you start
         * dotnet using the following command.
         *
         * dotnet run --environment Development
         *
         * The default environment is Production, meaning the
         * appsettings.Production.json configuration file will
         * be applied.
         */
        public static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()  
                .AddCommandLine(args)
                .Build();
            CreateWebHostBuilder(args)
                .UseConfiguration(config)
                .Build()
                .Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
