using Service_Running_Status_Check.Controller;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Service_Running_Status_Check
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .UseWindowsService()
            .ConfigureAppConfiguration((hostContext, config) => {

                // Ensure that the appsettings.json files are in same folder with the executable   
                string pathToExe = Process.GetCurrentProcess().MainModule.FileName;
                string dir = Path.GetDirectoryName(pathToExe);
                config.SetBasePath(dir);

            })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<FileCheck>();
                    services.AddHostedService<Worker>();
                });
    }
}
