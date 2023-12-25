using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Octokit;

namespace GitPuller
{
    public class GithubOperation
    {
        private readonly GitHubClient _github;
        private string _branchName;
        private string _repoName;
        private readonly ConfigurationManager _configManager;
        private readonly string _accessToken;
        private string _path;

        public GithubOperation(string accessToken)
        {
            _configManager = new ConfigurationManager();
            _accessToken = accessToken;

            _github = new GitHubClient(new ProductHeaderValue("GitPuller"));
            _github.Credentials = new Credentials(accessToken);
        }

        public async Task GetGithubAllRepositoryAndBranches()
        {
            try
            {
                var repositories = await _github.Repository.GetAllForCurrent();

                foreach (var repository in repositories)
                {
                    Console.WriteLine(repository.Name);
                    _repoName = repository.Name;

                    var branchList = await GetAllBranchesFromCurrentRepository(repository);
                    foreach (var branch in branchList)
                    {
                        Console.WriteLine(branch.Name);
                        _branchName = branch.Name;

                        if (CheckRepo(_configManager.GetRepositoryPaths(), _repoName))
                        {
                            await ExecuteGitCommands(_path);
                        }
                        else
                        {
                            Console.WriteLine("Local repository not found. Skipping ExecuteGitCommands.");
                        }
                    }
                    Console.WriteLine("--------");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                // Handle exceptions here
            }
        }

        public async Task<IReadOnlyList<Branch>> GetAllBranchesFromCurrentRepository(Repository repository)
        {
            return await _github.Repository.Branch.GetAll(repository.Id);
        }

        public async Task ExecuteGitCommands(string path)
        {
            try
            {
                var repositoryPaths = _configManager.GetRepositoryPaths();

                var exec = new CommandExecuter();

                var changeDirectory = $"cd {path}";
                exec.Execute(changeDirectory, path);

                var gitPullCommand = $"git pull origin {_branchName}";
                var pullResult = exec.Execute(gitPullCommand, path);
                Console.WriteLine(pullResult);

                //var gitStatusCommand = "git status";
                //var statusResult = exec.Execute(gitStatusCommand, path);
                //Console.WriteLine(statusResult);

                Console.WriteLine($"Branch: {_branchName}");
                Console.WriteLine($"Repository: {_repoName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing Git commands: {ex.Message}");
                // Handle exceptions during Git commands execution
            }
        }

        public bool CheckRepo(List<string> repositoryPaths, string repoName)
        {
            if (repositoryPaths != null && repositoryPaths.Count > 0)
            {
                foreach (var path in repositoryPaths)
                {
                    var directoryName = new DirectoryInfo(path).Name;

                    if (directoryName.Equals(repoName, StringComparison.OrdinalIgnoreCase) || directoryName.Equals($"{repoName}-{_branchName}", StringComparison.OrdinalIgnoreCase))
                    {
                        _path = path;
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
