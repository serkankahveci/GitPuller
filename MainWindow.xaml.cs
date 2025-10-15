using System;
using System.Windows;

namespace GitPuller
{
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel _viewModel;
        private readonly ConfigurationManager _configManager;

        public MainWindow()
        {
            InitializeComponent();
            _configManager = new ConfigurationManager(this);
            _viewModel = new MainWindowViewModel(_configManager, this);
            DataContext = _viewModel;

            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel.Initialize();
        }

        private async void StartPullingButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StartPullingButton.IsEnabled = false;
                StartPullingButton.Content = "Pulling...";

                await _viewModel.StartPulling();

                StartPullingButton.Content = "Start Pulling";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during pulling: {ex.Message}", "Error",
                               MessageBoxButton.OK, MessageBoxImage.Error);
                StartPullingButton.Content = "Start Pulling";
            }
            finally
            {
                StartPullingButton.IsEnabled = true;
            }
        }

        private void EnterTokenButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.SaveAccessToken();
        }

        private void RemoveTokenButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to remove the access token?",
                "Confirm Remove",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _viewModel.RemoveAccessToken();
                MessageBox.Show("Access token removed successfully!", "Success",
                               MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void AddPathButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.AddPath();
        }

        private void RemoveSelectedPathButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedPath != null)
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to remove this path?\n\n{_viewModel.SelectedPath}",
                    "Confirm Remove",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _viewModel.RemoveSelectedPath();
                    MessageBox.Show("Path removed successfully!", "Success",
                                   MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Please select a path to remove.", "No Selection",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ClearAllPathsButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.RepositoryPaths.Count == 0)
            {
                MessageBox.Show("No paths to clear.", "No Paths",
                               MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show(
                "Are you sure you want to remove ALL paths?",
                "Confirm Clear All",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _viewModel.ClearAllPaths();
                MessageBox.Show("All paths cleared successfully!", "Success",
                               MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}