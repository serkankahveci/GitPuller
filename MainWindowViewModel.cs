using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;

namespace GitPuller
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly ConfigurationManager _configManager;
        private readonly MainWindow _mainWindow;
        private string _accessToken;
        private string _newPath;
        private string _selectedPath;

        public MainWindowViewModel(ConfigurationManager configManager, MainWindow mainWindow)
        {
            _configManager = configManager;
            _mainWindow = mainWindow;
            RepositoryPaths = new ObservableCollection<string>();
            RepositoryNodes = new ObservableCollection<TreeNodeViewModel>();
        }

        public ObservableCollection<string> RepositoryPaths { get; }
        public ObservableCollection<TreeNodeViewModel> RepositoryNodes { get; }

        public string AccessToken
        {
            get => _accessToken;
            set
            {
                _accessToken = value;
                OnPropertyChanged();
            }
        }

        public string NewPath
        {
            get => _newPath;
            set
            {
                _newPath = value;
                OnPropertyChanged();
            }
        }

        public string SelectedPath
        {
            get => _selectedPath;
            set
            {
                _selectedPath = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSelectedPath));
            }
        }

        public bool HasSelectedPath => !string.IsNullOrEmpty(SelectedPath);
        public bool HasPaths => RepositoryPaths.Count > 0;

        public Visibility AccessTokenVisibility => string.IsNullOrEmpty(_configManager.GetAccessToken())
            ? Visibility.Visible
            : Visibility.Collapsed;

        public Visibility RemoveTokenVisibility => !string.IsNullOrEmpty(_configManager.GetAccessToken())
            ? Visibility.Visible
            : Visibility.Collapsed;

        public Visibility StartPullingVisibility => !string.IsNullOrEmpty(_configManager.GetAccessToken())
            ? Visibility.Visible
            : Visibility.Collapsed;

        public void Initialize()
        {
            RefreshPathsList();
            CheckAccessToken();
        }

        public void SaveAccessToken()
        {
            if (!string.IsNullOrWhiteSpace(AccessToken))
            {
                _configManager.SetToken(AccessToken);
                AccessToken = string.Empty;
                CheckAccessToken();
            }
            else
            {
                MessageBox.Show("Please enter a valid access token.", "Invalid Token",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void RemoveAccessToken()
        {
            _configManager.RemoveAccessToken();
            CheckAccessToken();
        }

        public void AddPath()
        {
            if (!string.IsNullOrWhiteSpace(NewPath))
            {
                _configManager.SaveFilePaths(NewPath);
                NewPath = string.Empty;
                RefreshPathsList();
                MessageBox.Show("Path added successfully!", "Success",
                               MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Please enter a valid path.", "Invalid Path",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void RemoveSelectedPath()
        {
            if (!string.IsNullOrEmpty(SelectedPath))
            {
                _configManager.RemovePath(SelectedPath);
                RefreshPathsList();
            }
        }

        public void ClearAllPaths()
        {
            _configManager.ClearAllPaths();
            RefreshPathsList();
        }

        public async Task StartPulling()
        {
            string accessToken = _configManager.GetAccessToken();
            var githubOps = new GithubOperation(accessToken, _mainWindow);

            // Clear existing nodes
            RepositoryNodes.Clear();

            await githubOps.GetGithubAllRepositoryAndBranches();
        }

        public void AddRepositoryNode(TreeNodeViewModel node)
        {
            RepositoryNodes.Add(node);
        }

        private void RefreshPathsList()
        {
            RepositoryPaths.Clear();
            var paths = _configManager.GetAllPathsForDisplay();

            foreach (var path in paths)
            {
                RepositoryPaths.Add(path);
            }

            OnPropertyChanged(nameof(HasPaths));
        }

        private void CheckAccessToken()
        {
            OnPropertyChanged(nameof(AccessTokenVisibility));
            OnPropertyChanged(nameof(RemoveTokenVisibility));
            OnPropertyChanged(nameof(StartPullingVisibility));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class TreeNodeViewModel : INotifyPropertyChanged
    {
        public string DisplayName { get; set; }
        public ObservableCollection<TreeNodeViewModel> Children { get; }

        public TreeNodeViewModel(string displayName)
        {
            DisplayName = displayName;
            Children = new ObservableCollection<TreeNodeViewModel>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}