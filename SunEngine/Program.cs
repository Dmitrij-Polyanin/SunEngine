﻿using System;
using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using SunEngine.Configuration;
using SunEngine.Utils;

namespace SunEngine
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var webHost = CreateWebHostBuilder(args).Build();

            webHost.Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseKestrel()
                .UseStartup<Startup>()
                .ConfigureAppConfiguration((builderContext, config) =>
                {
                    IHostingEnvironment env = builderContext.HostingEnvironment;
                    string mainSettingsFile = SettingsFileLocator.GetSettingFilePath("SunEngine.json");
                    string logSettingsFile = SettingsFileLocator.GetSettingFilePath("LogConfig.json");
                    string logSettingsFileEnv =
                        SettingsFileLocator.GetSettingFilePath($"LogConfig.{GetEnvSuffix(env)}.json",
                            true);

                    config.AddJsonFile(logSettingsFile, optional: false, reloadOnChange: false);
                    if (logSettingsFileEnv != null)
                        config.AddJsonFile(logSettingsFileEnv, optional: true, reloadOnChange: false);
                    
                    config.AddJsonFile(mainSettingsFile, optional: false, reloadOnChange: false);
                });

        private static string GetEnvSuffix(IHostingEnvironment env)
        {
            if (env.IsDevelopment()) return "dev";
            if (env.IsProduction()) return "prod";
            return env.EnvironmentName.ToLower();
        }
    }
} 