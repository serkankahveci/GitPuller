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
                // Read directly from file to get the most current value
                var currentJson = File.ReadAllText(_filePath);
                dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(currentJson);

                string accessToken = jsonObj["Git"]["AccessToken"];
                return accessToken;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading/accessing token: {ex.Message}");
                return null;
            }
        }

        // Remove the ReloadConfiguration method as we don't need it anymore

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
                var currentJson = File.ReadAllText(_filePath);
                dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(currentJson);

                jsonObj["Git"]["AccessToken"] = newToken;

                string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(_filePath, output);

                Console.WriteLine("Access token saved successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting access token: {ex.Message}");
            }
        }

        // NEW METHOD: Remove access token
        public void RemoveAccessToken()
        {
            try
            {
                var currentJson = File.ReadAllText(_filePath);
                dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(currentJson);

                // Clear the access token
                jsonObj["Git"]["AccessToken"] = "";

                string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(_filePath, output);

                Console.WriteLine("Access token removed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing access token: {ex.Message}");
            }
        }

        private void SaveConfigurationToFile()
        {
            try
            {
                // This method is now simplified since we handle JSON directly in other methods
                var currentJson = File.ReadAllText(_filePath);
                dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(currentJson);

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

                // Check if path already exists
                bool pathExists = false;
                foreach (var path in localPaths)
                {
                    if (path.ToString().Equals(newPath, StringComparison.OrdinalIgnoreCase))
                    {
                        pathExists = true;
                        break;
                    }
                }

                if (!pathExists)
                {
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

                    Console.WriteLine($"Path '{newPath}' added successfully.");
                }
                else
                {
                    Console.WriteLine($"Path '{newPath}' already exists.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding new path: {ex.Message}");
            }
        }

        // NEW METHOD: Remove a specific path
        public void RemovePath(string pathToRemove)
        {
            try
            {
                var currentJson = File.ReadAllText(_filePath);
                dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(currentJson);

                // Get the existing RepositoryPaths 
                var repositoryPaths = jsonObj["RepositoryPaths"] ?? new JObject();
                var localPaths = repositoryPaths["LocalPaths"] ?? new JArray();

                // Find and remove the path
                JToken itemToRemove = null;
                foreach (var path in localPaths)
                {
                    if (path.ToString().Equals(pathToRemove, StringComparison.OrdinalIgnoreCase))
                    {
                        itemToRemove = path;
                        break;
                    }
                }

                if (itemToRemove != null)
                {
                    ((JArray)localPaths).Remove(itemToRemove);

                    // Update the RepositoryPaths 
                    repositoryPaths["LocalPaths"] = localPaths;
                    jsonObj["RepositoryPaths"] = repositoryPaths;

                    // Preserve the AccessToken 
                    jsonObj["Git"]["AccessToken"] = _config.GetSection("Git")["AccessToken"];

                    // Serialize 
                    string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
                    File.WriteAllText(_filePath, output);

                    Console.WriteLine($"Path '{pathToRemove}' removed successfully.");
                }
                else
                {
                    Console.WriteLine($"Path '{pathToRemove}' not found.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing path: {ex.Message}");
            }
        }

        // NEW METHOD: Clear all paths
        public void ClearAllPaths()
        {
            try
            {
                var currentJson = File.ReadAllText(_filePath);
                dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(currentJson);

                // Clear all paths
                jsonObj["RepositoryPaths"]["LocalPaths"] = new JArray();

                // Preserve the AccessToken 
                jsonObj["Git"]["AccessToken"] = _config.GetSection("Git")["AccessToken"];

                // Serialize 
                string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(_filePath, output);

                Console.WriteLine("All paths cleared successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clearing all paths: {ex.Message}");
            }
        }

        // NEW METHOD: Get all paths as a formatted list for display
        public List<string> GetAllPathsForDisplay()
        {
            try
            {
                var currentJson = File.ReadAllText(_filePath);
                dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(currentJson);

                var repositoryPaths = jsonObj["RepositoryPaths"]?["LocalPaths"];
                var pathsList = new List<string>();

                if (repositoryPaths != null)
                {
                    foreach (var path in repositoryPaths)
                    {
                        pathsList.Add(path.ToString());
                    }
                }

                return pathsList;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting paths for display: {ex.Message}");
                return new List<string>();
            }
        }
    }
}