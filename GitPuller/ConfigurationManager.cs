using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace GitPuller
{
    public class ConfigurationManager
    {
        private readonly IConfigurationRoot _config;

        public ConfigurationManager()
        {
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var projectPath = basePath.Substring(0, basePath.IndexOf("bin", StringComparison.Ordinal));

            _config = new ConfigurationBuilder()
                .SetBasePath(projectPath)
                .AddJsonFile("appsettings.json")
                .Build();
        }

        public string GetAccessToken()
        {
            try
            {
                string accessToken = _config.GetSection("Git")["AccessToken"];

                if (string.IsNullOrEmpty(accessToken))
                {
                    Console.Write("Enter Git Access Token: ");
                    accessToken = Console.ReadLine();
                }

                return accessToken;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading access token: {ex.Message}");
                return null; // veya uygun bir hata durumu işaretleyebilirsiniz
            }
        }

        public List<string> GetRepositoryPaths()
        {
            try
            {
                var repositoryPaths = _config
                    .GetSection("RepositoryPaths")
                    .AsEnumerable()
                    .Select(x => x.Value)
                    .Where(n => n != null)
                    .ToList();

                return repositoryPaths;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading repository paths: {ex.Message}");
                return new List<string>(); // veya uygun bir hata durumu işaretleyebilirsiniz
            }
        }
    }
}