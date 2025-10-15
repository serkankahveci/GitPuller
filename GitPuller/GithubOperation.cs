using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
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
        private readonly MainWindow _mainWindow;

        public GithubOperation(string accessToken, MainWindow mainWindow)
        {
            _configManager = new ConfigurationManager(mainWindow);
            _accessToken = accessToken;
            _mainWindow = mainWindow;

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

                await RepositoryAndBranchesToTreeView();
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

        public Task ExecuteGitCommands(string path)
        {
            try
            {
                var repositoryPaths = _configManager.GetRepositoryPaths();

                var exec = new CommandExecuter();

                var changeDirectory = $"cd {path}";
                exec.Execute(changeDirectory, path);

                var gitStatusCommand = "git status --porcelain";
                var statusResult = exec.Execute(gitStatusCommand, path);

                if (!string.IsNullOrWhiteSpace(statusResult))
                {
                    var gitStashSave = "git stash save -S 'New Save'";
                    var stashSaveResult = exec.Execute(gitStashSave, path);
                    Console.WriteLine(stashSaveResult);
                }

                var gitPullCommand = $"git pull origin {_branchName}";
                var pullResult = exec.Execute(gitPullCommand, path);
                Console.WriteLine(pullResult);

                if (!string.IsNullOrWhiteSpace(statusResult))
                {
                    var gitStashPop = "git stash pop stash@{0}";
                    var stashPopResult = exec.Execute(gitStashPop, path);
                    Console.WriteLine(stashPopResult);
                }

                Console.WriteLine($"Branch: {_branchName}");
                Console.WriteLine($"Repository: {_repoName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing Git commands: {ex.Message}");
            }

            return Task.CompletedTask;
        }

        public bool CheckRepo(List<string> repositoryPaths, string repoName)
        {
            if (repositoryPaths != null && repositoryPaths.Count > 0)
            {
                foreach (var path in repositoryPaths)
                {
                    var directoryName = new DirectoryInfo(path).Name;

                    if (directoryName.Equals(repoName, StringComparison.OrdinalIgnoreCase) ||
                        directoryName.Equals($"{repoName}-{_branchName}", StringComparison.OrdinalIgnoreCase))
                    {
                        _path = path;
                        return true;
                    }
                }
            }
            return false;
        }

        public async Task RepositoryAndBranchesToTreeView()
        {
            try
            {
                var repositories = await _github.Repository.GetAllForCurrent();

                // Get the ViewModel from the MainWindow's DataContext
                if (_mainWindow.DataContext is MainWindowViewModel viewModel)
                {
                    // Clear existing nodes on UI thread
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        viewModel.RepositoryNodes.Clear();
                    });

                    foreach (var repository in repositories)
                    {
                        var repoNode = new TreeNodeViewModel($"📁 {repository.Name}");

                        var branchList = await GetAllBranchesFromCurrentRepository(repository);
                        foreach (var branch in branchList)
                        {
                            var branchNode = new TreeNodeViewModel($"🌿 {branch.Name}");
                            repoNode.Children.Add(branchNode);
                        }

                        // Update UI on main thread
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            viewModel.AddRepositoryNode(repoNode);
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}