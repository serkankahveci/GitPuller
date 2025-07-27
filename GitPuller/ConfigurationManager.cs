using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace GitPuller
{
    public class ConfigurationManager
    {
        private readonly IConfigurationRoot _config;
        private readonly string _filePath;
        private readonly Form1 _form;
        
        // Performance optimization: Cache configuration data
        private static readonly object _lockObject = new object();
        private static JObject _cachedConfig;
        private static DateTime _lastFileWrite = DateTime.MinValue;
        
        // Performance optimization: Lazy loading of repository paths
        private List<string> _cachedRepositoryPaths;
        private DateTime _lastRepositoryPathsRead = DateTime.MinValue;

        public ConfigurationManager(Form1 form)
        {
            _form = form; 
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var projectPath = basePath.Substring(0, basePath.IndexOf("bin", StringComparison.Ordinal));

            _config = new ConfigurationBuilder()
                .SetBasePath(projectPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false) // Disable file watching for performance
                .Build();

            _filePath = Path.Combine(projectPath, "appsettings.json");
            
            // Initialize cache
            InitializeCache();
        }

        private void InitializeCache()
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    var fileContent = File.ReadAllText(_filePath);
                    _cachedConfig = JObject.Parse(fileContent);
                    _lastFileWrite = File.GetLastWriteTime(_filePath);
                }
                else
                {
                    // Create default configuration if file doesn't exist
                    _cachedConfig = new JObject
                    {
                        ["Git"] = new JObject { ["AccessToken"] = string.Empty },
                        ["RepositoryPaths"] = new JObject { ["LocalPaths"] = new JArray() }
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing configuration cache: {ex.Message}");
                _cachedConfig = new JObject();
            }
        }

        public string GetAccessToken()
        {
            try
            {
                lock (_lockObject)
                {
                    RefreshCacheIfNeeded();
                    
                    var accessToken = _cachedConfig["Git"]?["AccessToken"]?.ToString();

                    if (string.IsNullOrEmpty(accessToken))
                    {
                        accessToken = _form.GetAccessTokenFromTextBox();
                        
                        if (!string.IsNullOrEmpty(accessToken))
                        {
                            SetTokenInternal(accessToken);
                        }
                    }

                    return accessToken;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading access token: {ex.Message}");
                return null;
            }
        }

        public List<string> GetRepositoryPaths()
        {
            try
            {
                // Use cached repository paths if available and recent
                if (_cachedRepositoryPaths != null && 
                    DateTime.Now - _lastRepositoryPathsRead < TimeSpan.FromMinutes(5))
                {
                    return _cachedRepositoryPaths;
                }

                lock (_lockObject)
                {
                    RefreshCacheIfNeeded();
                    
                    var repositoryPaths = _cachedConfig["RepositoryPaths"]?["LocalPaths"]
                        ?.ToObject<List<string>>()
                        ?.Where(path => !string.IsNullOrWhiteSpace(path))
                        .ToList() ?? new List<string>();

                    _cachedRepositoryPaths = repositoryPaths;
                    _lastRepositoryPathsRead = DateTime.Now;
                    
                    return repositoryPaths;
                }
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
                lock (_lockObject)
                {
                    SetTokenInternal(newToken);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting access token: {ex.Message}");
            }
        }

        private void SetTokenInternal(string newToken)
        {
            if (_cachedConfig["Git"] == null)
            {
                _cachedConfig["Git"] = new JObject();
            }
            
            _cachedConfig["Git"]["AccessToken"] = newToken;
            SaveConfigurationToFileAsync();
        }

        private async void SaveConfigurationToFileAsync()
        {
            try
            {
                await Task.Run(() =>
                {
                    var output = _cachedConfig.ToString(Newtonsoft.Json.Formatting.Indented);
                    File.WriteAllText(_filePath, output);
                    _lastFileWrite = DateTime.Now;
                });
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
                if (!string.IsNullOrWhiteSpace(paths))
                {
                    AddNewPath(paths);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving file paths: {ex.Message}");
            }
        }

        public void AddNewPath(string newPath)
        {
            if (string.IsNullOrWhiteSpace(newPath))
                return;

            try
            {
                lock (_lockObject)
                {
                    RefreshCacheIfNeeded();
                    
                    // Ensure RepositoryPaths structure exists
                    if (_cachedConfig["RepositoryPaths"] == null)
                    {
                        _cachedConfig["RepositoryPaths"] = new JObject();
                    }
                    
                    if (_cachedConfig["RepositoryPaths"]["LocalPaths"] == null)
                    {
                        _cachedConfig["RepositoryPaths"]["LocalPaths"] = new JArray();
                    }

                    var localPaths = (JArray)_cachedConfig["RepositoryPaths"]["LocalPaths"];
                    
                    // Check if path already exists to avoid duplicates
                    if (!localPaths.Any(token => token.ToString().Equals(newPath, StringComparison.OrdinalIgnoreCase)))
                    {
                        localPaths.Add(newPath);
                        
                        // Invalidate cached repository paths
                        _cachedRepositoryPaths = null;
                        
                        SaveConfigurationToFileAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding new path: {ex.Message}");
            }
        }

        private void RefreshCacheIfNeeded()
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    var currentFileTime = File.GetLastWriteTime(_filePath);
                    if (currentFileTime > _lastFileWrite)
                    {
                        var fileContent = File.ReadAllText(_filePath);
                        _cachedConfig = JObject.Parse(fileContent);
                        _lastFileWrite = currentFileTime;
                        
                        // Invalidate cached repository paths when config changes
                        _cachedRepositoryPaths = null;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error refreshing configuration cache: {ex.Message}");
            }
        }

        // Performance optimization: Bulk update method
        public async Task UpdateConfigurationAsync(string accessToken, List<string> repositoryPaths)
        {
            try
            {
                await Task.Run(() =>
                {
                    lock (_lockObject)
                    {
                        if (!string.IsNullOrEmpty(accessToken))
                        {
                            if (_cachedConfig["Git"] == null)
                            {
                                _cachedConfig["Git"] = new JObject();
                            }
                            _cachedConfig["Git"]["AccessToken"] = accessToken;
                        }

                        if (repositoryPaths?.Any() == true)
                        {
                            if (_cachedConfig["RepositoryPaths"] == null)
                            {
                                _cachedConfig["RepositoryPaths"] = new JObject();
                            }
                            
                            _cachedConfig["RepositoryPaths"]["LocalPaths"] = JArray.FromObject(repositoryPaths);
                            _cachedRepositoryPaths = null; // Invalidate cache
                        }

                        var output = _cachedConfig.ToString(Newtonsoft.Json.Formatting.Indented);
                        File.WriteAllText(_filePath, output);
                        _lastFileWrite = DateTime.Now;
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating configuration: {ex.Message}");
            }
        }

        // Cleanup method for better resource management
        public void ClearCache()
        {
            lock (_lockObject)
            {
                _cachedRepositoryPaths = null;
                _lastRepositoryPathsRead = DateTime.MinValue;
            }
        }
    }
}