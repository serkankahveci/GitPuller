using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Extensions.Configuration;
using Octokit;

namespace GitPuller
{
    public class GithubOperation
    {
        private readonly GitHubClient _github;
        private readonly ConfigurationManager _configManager;
        private readonly string _accessToken;
        private readonly Form1 _form;
        
        // Performance optimizations: Caching
        private static readonly ConcurrentDictionary<long, IReadOnlyList<Branch>> _branchCache = new ConcurrentDictionary<long, IReadOnlyList<Branch>>();
        private static readonly ConcurrentDictionary<string, IReadOnlyList<Repository>> _repositoryCache = new ConcurrentDictionary<string, IReadOnlyList<Repository>>();
        
        // Rate limiting and performance tracking
        private readonly SemaphoreSlim _rateLimitSemaphore = new SemaphoreSlim(10, 10); // Limit concurrent API calls

        public GithubOperation(string accessToken, Form1 form)
        {
            _configManager = new ConfigurationManager(form);
            _accessToken = accessToken;
            _form = form;

            _github = new GitHubClient(new ProductHeaderValue("GitPuller"));
            _github.Credentials = new Credentials(accessToken);
            
            // Performance optimization: Configure HTTP client timeout
            _github.SetRequestTimeout(TimeSpan.FromSeconds(30));
        }

        public async Task GetGithubAllRepositoryAndBranches()
        {
            try
            {
                // Use cached repositories if available
                var repositories = await GetCachedRepositories();
                var repositoryPaths = _configManager.GetRepositoryPaths();

                // Parallel processing for better performance
                var tasks = repositories.Select(async repository =>
                {
                    await _rateLimitSemaphore.WaitAsync();
                    try
                    {
                        var branchList = await GetAllBranchesFromCurrentRepository(repository);
                        
                        // Process branches in parallel
                        var branchTasks = branchList.Select(async branch =>
                        {
                            if (CheckRepo(repositoryPaths, repository.Name, branch.Name, out string path))
                            {
                                await ExecuteGitCommands(path, branch.Name, repository.Name);
                            }
                        });
                        
                        await Task.WhenAll(branchTasks);
                    }
                    finally
                    {
                        _rateLimitSemaphore.Release();
                    }
                });

                await Task.WhenAll(tasks);

                // Update UI once at the end
                await RepositoryAndBranchesToTreeView();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                // Consider logging to file for production
            }
        }

        private async Task<IReadOnlyList<Repository>> GetCachedRepositories()
        {
            const string cacheKey = "current_user_repos";
            
            if (_repositoryCache.TryGetValue(cacheKey, out var cachedRepos))
            {
                return cachedRepos;
            }

            var repositories = await _github.Repository.GetAllForCurrent();
            _repositoryCache.TryAdd(cacheKey, repositories);
            return repositories;
        }

        public async Task<IReadOnlyList<Branch>> GetAllBranchesFromCurrentRepository(Repository repository)
        {
            // Use cache to avoid redundant API calls
            if (_branchCache.TryGetValue(repository.Id, out var cachedBranches))
            {
                return cachedBranches;
            }

            var branches = await _github.Repository.Branch.GetAll(repository.Id);
            _branchCache.TryAdd(repository.Id, branches);
            return branches;
        }

        public async Task ExecuteGitCommands(string path, string branchName, string repoName)
        {
            try
            {
                var exec = new CommandExecuter();

                // Use optimized git operations
                var result = await exec.PullRepository(path, branchName);
                
                if (result.Success)
                {
                    Console.WriteLine($"✓ Completed: {repoName}/{branchName}");
                    if (!string.IsNullOrEmpty(result.PullOutput))
                    {
                        Console.WriteLine($"  Pull output: {result.PullOutput}");
                    }
                }
                else
                {
                    Console.WriteLine($"✗ Failed {repoName}/{branchName}: {result.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error executing Git commands for {repoName}/{branchName}: {ex.Message}");
            }
        }

        public bool CheckRepo(List<string> repositoryPaths, string repoName, string branchName, out string foundPath)
        {
            foundPath = null;
            
            if (repositoryPaths?.Count > 0)
            {
                // Optimize: Use LINQ for better performance and readability
                foundPath = repositoryPaths.FirstOrDefault(path =>
                {
                    var directoryName = new DirectoryInfo(path).Name;
                    return directoryName.Equals(repoName, StringComparison.OrdinalIgnoreCase) ||
                           directoryName.Equals($"{repoName}-{branchName}", StringComparison.OrdinalIgnoreCase);
                });
            }
            
            return foundPath != null;
        }

        public async Task RepositoryAndBranchesToTreeView()
        {
            try
            {
                var repositories = await GetCachedRepositories();

                // Optimize UI updates by building tree structure first, then update UI once
                var rootNodes = new List<TreeNode>();

                var tasks = repositories.Select(async repository =>
                {
                    var repoNode = new TreeNode($"@{repository.Name}");
                    var branchList = await GetAllBranchesFromCurrentRepository(repository);
                    
                    foreach (var branch in branchList)
                    {
                        var branchNode = new TreeNode($"@{branch.Name}");
                        repoNode.Nodes.Add(branchNode);
                    }
                    
                    return repoNode;
                });

                rootNodes.AddRange(await Task.WhenAll(tasks));

                // Single UI update for better performance
                await Task.Run(() =>
                {
                    _form.BeginInvoke(new Action(() =>
                    {
                        foreach (var node in rootNodes)
                        {
                            _form.UpdateTreeView(node);
                        }
                    }));
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error building tree view: {ex.Message}");
            }
        }

        // Performance optimization: Clear cache when needed
        public static void ClearCache()
        {
            _branchCache.Clear();
            _repositoryCache.Clear();
        }

        // Resource cleanup
        public void Dispose()
        {
            _rateLimitSemaphore?.Dispose();
        }
    }
}
