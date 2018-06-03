﻿using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Daylily.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var app = Microsoft.Extensions.PlatformAbstractions.PlatformServices.Default.Application;
            Console.WriteLine($"{app.ApplicationName} {app.ApplicationVersion} based on {app.RuntimeFramework}");
            Mapper.Init();
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseUrls("http://*:23333")
                .Build();
    }
}
