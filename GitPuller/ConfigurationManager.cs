using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace GitPuller
{
    public class ConfigurationManager
    {
        private readonly IConfigurationRoot _config;
        private readonly string _filePath;
        private readonly Form1 _form;

        public ConfigurationManager(Form1 form)
        {
            _form = form; 
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var projectPath = basePath.Substring(0, basePath.IndexOf("bin", StringComparison.Ordinal));

            _config = new ConfigurationBuilder()
                .SetBasePath(projectPath)
                .AddJsonFile("appsettings.json")
                .Build();

            _filePath = Path.Combine(projectPath, "appsettings.json");
        }

        public string GetAccessToken()
        {
            try
            {
                string accessToken = _config.GetSection("Git")["AccessToken"];

                if (string.IsNullOrEmpty(accessToken))
                {
                    accessToken = _form.GetAccessTokenFromTextBox();

                    _config.GetSection("Git")["AccessToken"] = accessToken;

                    SaveConfigurationToFile();
                }

                return accessToken;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading/accessing token: {ex.Message}");
                return null;
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
                return new List<string>();
            }
        }

        public void SetToken(string newToken)
        {
            try
            {
                _config.GetSection("Git")["AccessToken"] = newToken;

                SaveConfigurationToFile();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting access token: {ex.Message}");
            }
        }

        private void SaveConfigurationToFile()
        {
            try
            {
                var json = _config.GetSection("Git")["AccessToken"];
                var currentJson = File.ReadAllText(_filePath);
                dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(currentJson);

                jsonObj["Git"]["AccessToken"] = json;

                string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(_filePath, output);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving configuration: {ex.Message}");
            }
        }

        public void SaveFilePaths(string paths)
        {
            try
            {
                AddNewPath(paths);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting access token: {ex.Message}");
            }
        }

        public void AddNewPath(string newPath)
        {
            try
            {
                var currentJson = File.ReadAllText(_filePath);
                dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(currentJson);

                // Get the existing RepositoryPaths 
                var repositoryPaths = jsonObj["RepositoryPaths"] ?? new JObject();
                var localPaths = repositoryPaths["LocalPaths"] ?? new JArray();

                // Add the new path to the localPaths array
                ((JArray)localPaths).Add(newPath);

                // Update the RepositoryPaths 
                repositoryPaths["LocalPaths"] = localPaths;
                jsonObj["RepositoryPaths"] = repositoryPaths;

                // Preserve the AccessToken 
                jsonObj["Git"]["AccessToken"] = _config.GetSection("Git")["AccessToken"];

                // Serialize 
                string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(_filePath, output);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding new path: {ex.Message}");
            }
        }
    }
}