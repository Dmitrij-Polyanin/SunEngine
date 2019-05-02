using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using SunEngine.Core.DataBase;
using SunEngine.Core.Models;

namespace SunEngine.DataSeed
{
    /// <summary>
    /// Class to seed database with initial data with 2 modes
    /// "init" (SeedInitialize) - seed roles, users, categories
    /// "seed" (SeedAddTestData) - seed test materials and comments 
    /// </summary>
    public class MainSeeder
    {
        public const string SeedCommand = "seed";


        private readonly string providerName;
        private readonly string connectionString;
        private readonly string configDirPath;

        public MainSeeder(string configDirPath = "Config")
        {
            this.configDirPath = configDirPath;
            string dbSettingsFile = Path.GetFullPath(Path.Combine(configDirPath, "DataBaseConnection.json"));
            var configuration = new ConfigurationBuilder()
                .AddJsonFile(dbSettingsFile, optional: false, reloadOnChange: true)
                .Build();

            var dataBaseConfiguration = configuration.GetSection("DataBaseConnection");
            providerName = dataBaseConfiguration["Linq2dbProvider"];
            connectionString = dataBaseConfiguration["ConnectionString"];
        }

        /// <summary>
        /// Initialize database with roles, users, categories from config direcory
        /// </summary>
        public void SeedInitialize()
        {
            using (DataBaseConnection db = new DataBaseConnection(providerName, connectionString))
            {
                DataContainer dataContainer = new InitialSeeder(configDirPath).Seed();
                new DataBaseSeeder(db, dataContainer).SeedInitial();
            }
        }

        /// <summary>
        /// Seed database with materials and comment for testing purposes
        /// </summary>
        public void SeedAddTestData(IList<string> catTokens, bool titleAppendCategoryName = false)
        {
            string seedCommandDots = SeedCommand + ":";
            if (catTokens.Contains(SeedCommand)) catTokens[catTokens.IndexOf(SeedCommand)] = seedCommandDots + Category.RootName;
            catTokens = catTokens.Select(x => x.Substring(seedCommandDots.Length)).ToList();

            using (DataBaseConnection db = new DataBaseConnection(providerName, connectionString))
            {
                DataContainer dataContainer = new DataContainer
                {
                    Categories = db.Categories.ToList(),
                    Users = db.Users.ToList(),
                    currentMaterialId = db.Materials.Any() ? db.Materials.Max(x => x.Id) + 1 : 1,
                    currentCommentId = db.Comments.Any() ? db.Comments.Max(x => x.Id) + 1 : 1
                };

                MaterialsSeeder materialsSeeder = new MaterialsSeeder(dataContainer);

                foreach (var catToken in catTokens)
                {
                    var parts = catToken.Split(":");
                    var categoryName = parts[0];
                    int? materialsCount = null;
                    if (parts.Length > 1)
                        materialsCount = int.Parse(parts[1]);
                    int? commentsCount = null;
                    if (parts.Length > 2)
                        commentsCount = int.Parse(parts[2]);

                    if (materialsCount.HasValue)
                        materialsSeeder.MinMaterialCount = materialsSeeder.MaxMaterialCount = materialsCount.Value;

                    if (commentsCount.HasValue)
                        materialsSeeder.CommentsCount = commentsCount.Value;

                    materialsSeeder.TitleAppendCategoryName = titleAppendCategoryName;

                    materialsSeeder.SeedCategoryAndSub(categoryName);
                }

                new DataBaseSeeder(db, dataContainer).SeedMaterials().PostSeedMaterials();
            }
        }
    }
}