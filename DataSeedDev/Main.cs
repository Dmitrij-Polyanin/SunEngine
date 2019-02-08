using System;
using System.IO;
using DataSeedDev.Seeder;
using Microsoft.Extensions.Configuration;
using SunEngine.Commons.DataBase;

namespace DataSeedDev
{
    class Program
    {
        static void Main(string[] args)
        {
            SeedDataBase();
        }

        static void SeedDataBase()
        {
            string dbSettingsFile = GetDataBaseConnectionFile();
            var configuration = new ConfigurationBuilder()
                .AddJsonFile(dbSettingsFile, optional: false, reloadOnChange: true)
                .Build();


            var dataBaseConfiguration = configuration.GetSection("DataBaseConnection");
            var providerName = dataBaseConfiguration["Provider"];
            var connectionString = dataBaseConfiguration["ConnectionString"];

            using (DataBaseConnection db = new DataBaseConnection(providerName, connectionString))
            {
                var dc = new LocalSeeder().Seed();
                new DataBaseSeeder(db, dc).Seed().PostSeed();
            }
        }

        static string GetDataBaseConnectionFile()
        {
            string fileName = "Local.SunEngine.json";
            string[] dirs =  {"","../","../SunEngine/","../SunEngine/Settings/"};

            foreach (var dir in dirs)
            {
                string path = Path.GetFullPath(dir + fileName);
                if (File.Exists(path))
                    return path;
            }
            
            fileName = "SunEngine.json";
            foreach (var dir in dirs)
            {
                string path = Path.GetFullPath(dir + fileName);
                if (File.Exists(path))
                    return path;
            }
            
            throw new Exception("Can not locate Local.SunEngine.json or SunEngine.json");
        }
    }
}