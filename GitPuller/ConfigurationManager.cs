using System;
using System.IO;
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
            string accessToken = _config.GetSection("Git")["AccessToken"];

            if (string.IsNullOrEmpty(accessToken))
            {
                Console.Write("Enter Git Access Token: ");
                accessToken = Console.ReadLine();
            }

            return accessToken;
        }
    }
}
