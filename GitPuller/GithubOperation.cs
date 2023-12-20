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
                            await ExecuteGitCommands();
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
            }
        }

        public async Task<IReadOnlyList<Branch>> GetAllBranchesFromCurrentRepository(Repository repository)
        {
            return await _github.Repository.Branch.GetAll(repository.Id);
        }

        public async Task ExecuteGitCommands()
        {
            var repositoryPaths = _configManager.GetRepositoryPaths();

            var exec = new CommandExecuter();

            Console.WriteLine($"Cloning {_repoName}...");
            var gitCloneCommand = $"git clone https://github.com/{_repoName}.git";
            var cloneResult = exec.Execute(gitCloneCommand);
            Console.WriteLine(cloneResult);

            Console.WriteLine("Pulling...");
            var gitPullCommand = $"git pull origin {_branchName}";
            var pullResult = exec.Execute(gitPullCommand);
            Console.WriteLine(pullResult);

            Console.WriteLine("Testing...");
            var testCommand = "git status";
            var testResult = exec.Execute(testCommand);
            Console.WriteLine(testResult);

            Console.WriteLine($"Branch: {_branchName}");
            Console.WriteLine($"Repository: {_repoName}");
            Console.WriteLine("TEST");
        }

        public bool CheckRepo(List<string> repositoryPaths, string repoName)
        {
            if (repositoryPaths != null && repositoryPaths.Count > 0)
            {
                foreach (var path in repositoryPaths)
                {
                    var directoryName = new DirectoryInfo(path).Name;

                    if (repoName.Contains(directoryName))
                    {
                        return true; // Repository found in specified path
                    }
                }
            }
            return false; // Repository not found in any specified path
        }
    }
}
