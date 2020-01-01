using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace SunEngine.Cli
{
    public static class InfoPrinter
    {
        /// <summary>
        /// Print this info if "dotnet SunEngine.dll" starts with no arguments.
        /// </summary>
        public static void PrintNoArgumentsInfo()
        {
            Console.WriteLine("Valid startup arguments was not provided.\nTo list available arguments run with 'help' command.\n> dotnet SunEngine.dll help\n");
        }

        public static void PrintStart()
        {
            PrintSunEngineLogo();
            Console.WriteLine();
        }
        
        public static void PrintSunEngineLogo()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            string v = fileVersionInfo.ProductVersion;
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($@"
    ______                _____                         
   /  ____)              |  ___)               (o)           
  (  (___   _   _  ____  | |___   ____    ____  _  ____   ____ 
   \___  \ | | | ||  _ \ |  ___) |  _ \  / _  || ||  _ \ |  __)
   ____)  )| |_| || | | || |____ | | | |( (_| || || | | || |__)
  (______/ |_____/|_| |_||______)|_| |_| \___ ||_||_| |_||_____)
                                          __| |
            Version: {v,-6}              (____| ".TrimStart('\n'));
            
            Console.ResetColor();
        }

        /// <summary>
        /// Print help on dotnet "dotnet SunEngine.dll help"
        /// </summary>
        public static void PrintHelp()
        {
            const int padding = -36;
            string helpText = $@"
  Commands
     {StartupConfiguration.ServerCommand,padding} Host server api with kestrel
     {StartupConfiguration.ConfigArgumentName + ":<Path>",padding} Path to config directory, if none ""Config"" is default, "".Config"" suffix at the end of the path can be skipped
     {StartupConfiguration.MigrateCommand,padding} Make initial database table structure and migrations in existing database
     {StartupConfiguration.InitCommand,padding} Initialize users, roles and categories tables from config directory
     {StartupConfiguration.TestDatabaseConnection,padding} Check is data base connection is working                     
     {StartupConfiguration.VersionCommand,padding} Print SunEngine version
     {StartupConfiguration.HelpCommand,padding} Show this help   
    
  Seed test data commands    
     {StartupConfiguration.SeedCommand}:<CategoryName>:<MaterialsCount>:<CommentsCount>      
     {"",padding} Seed category and all subcategories with materials and comments
     {"",padding} MaterialsCount and CommentsCount - default if skipped
     {"",padding} Example - seed:SomeCategory:20:10
                                
     {StartupConfiguration.AppendCategoriesNamesCommand,padding} Add category name to material titles on ""{StartupConfiguration.SeedCommand}""

  Examples
     dotnet SunEngine.dll {StartupConfiguration.ServerCommand}
     dotnet SunEngine.dll {StartupConfiguration.ServerCommand} {StartupConfiguration.ConfigArgumentName}:local.MySite
     dotnet SunEngine.dll {StartupConfiguration.MigrateCommand} {StartupConfiguration.InitCommand} {StartupConfiguration.SeedCommand}
     dotnet SunEngine.dll {StartupConfiguration.MigrateCommand} {StartupConfiguration.InitCommand} {StartupConfiguration.SeedCommand} {StartupConfiguration.ConfigArgumentName}:local.MySite
     dotnet SunEngine.dll {StartupConfiguration.SeedCommand}:Forum:10:10
";

            Console.WriteLine(helpText);
        }

        /// <summary>
        /// Print version of SunEngine
        /// </summary>
        public static void PrintVersion()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fileVersionInfo.ProductVersion;
            Console.WriteLine($"SunEngine version: {version}");
        }
    }
}
