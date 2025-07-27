using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GitPuller
{
    public partial class Form1 : Form
    {
        private readonly ConfigurationManager _configManager;
        private GithubOperation _githubOps;
        private bool _operationInProgress = false;

        public Form1()
        {
            InitializeComponent();
            _configManager = new ConfigurationManager(this);
            CheckAccessToken();
            
            // Performance optimization: Set up UI for better responsiveness
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
        }

        private void CheckAccessToken()
        {
            string accessToken = _configManager.GetAccessToken();

            bool hasToken = !string.IsNullOrEmpty(accessToken);
            
            // Batch UI updates for better performance
            AccessTokenTaker.Visible = !hasToken;
            EnterButton.Visible = !hasToken;
            AccessTokenLabel.Visible = !hasToken;
            StartPulling.Visible = hasToken;
            StartPulling.Enabled = hasToken && !_operationInProgress;
        }

        public string GetAccessTokenFromTextBox()
        {
            // Ensure we're on UI thread
            if (InvokeRequired)
            {
                return (string)Invoke(new Func<string>(() => AccessTokenTaker.Text));
            }
            return AccessTokenTaker.Text;
        }

        public void UpdateTreeView(TreeNode node)
        {
            // Ensure thread-safe UI updates
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => UpdateTreeView(node)));
                return;
            }
            
            treeView1.Nodes.Add(node);
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            if (_operationInProgress)
            {
                MessageBox.Show("Operation already in progress. Please wait...", "GitPuller", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                _operationInProgress = true;
                UpdateUIForOperation(true);

                string accessToken = _configManager.GetAccessToken();
                
                if (string.IsNullOrEmpty(accessToken))
                {
                    MessageBox.Show("Please enter a valid access token first.", "GitPuller", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Clear previous results
                treeView1.Nodes.Clear();
                
                // Initialize GitHub operations
                _githubOps?.Dispose(); // Clean up previous instance
                _githubOps = new GithubOperation(accessToken, this);

                // Run the operation asynchronously to prevent UI freezing
                await Task.Run(async () => await _githubOps.GetGithubAllRepositoryAndBranches());
                
                MessageBox.Show("Repository pulling completed!", "GitPuller", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "GitPuller Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _operationInProgress = false;
                UpdateUIForOperation(false);
            }
        }

        private void UpdateUIForOperation(bool inProgress)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => UpdateUIForOperation(inProgress)));
                return;
            }

            StartPulling.Enabled = !inProgress && !string.IsNullOrEmpty(_configManager.GetAccessToken());
            StartPulling.Text = inProgress ? "Processing..." : "Start Pulling";
            
            // Change cursor to indicate operation
            Cursor = inProgress ? Cursors.WaitCursor : Cursors.Default;
            
            // Disable form controls during operation to prevent interference
            AccessTokenTaker.Enabled = !inProgress;
            EnterButton.Enabled = !inProgress;
            pathsTextBox.Enabled = !inProgress;
        }

        private void EnterButton_Click(object sender, EventArgs e)
        {
            ProcessAccessToken();
        }

        private void EnterButton_Click_1(object sender, EventArgs e)
        {
            ProcessAccessToken();
        }

        private void ProcessAccessToken()
        {
            try
            {
                string enteredToken = GetAccessTokenFromTextBox();
                
                if (string.IsNullOrWhiteSpace(enteredToken))
                {
                    MessageBox.Show("Please enter a valid access token.", "GitPuller", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                _configManager.SetToken(enteredToken);
                CheckAccessToken();
                
                MessageBox.Show("Access token saved successfully!", "GitPuller", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving access token: {ex.Message}", "GitPuller Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void button1_Click_1(object sender, EventArgs e)
        {
            try
            {
                string newPath = pathsTextBox.Text?.Trim();
                
                if (string.IsNullOrWhiteSpace(newPath))
                {
                    MessageBox.Show("Please enter a valid repository path.", "GitPuller", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                // Async save for better performance
                await Task.Run(() => _configManager.SaveFilePaths(newPath));
                
                // Clear the text box after successful save
                pathsTextBox.Clear();
                
                MessageBox.Show("Repository path saved successfully!", "GitPuller", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving repository path: {ex.Message}", "GitPuller Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Performance optimization: Load any cached data asynchronously
            Task.Run(() =>
            {
                try
                {
                    _configManager.GetRepositoryPaths(); // Warm up cache
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error warming up configuration cache: {ex.Message}");
                }
            });
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            // Could implement repository-specific operations here
        }

        private void label1_Click(object sender, EventArgs e)
        {
            // Empty event handler
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            try
            {
                // Clean up resources
                _githubOps?.Dispose();
                GithubOperation.ClearCache();
                _configManager?.ClearCache();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during cleanup: {ex.Message}");
            }
            finally
            {
                base.OnFormClosed(e);
            }
        }

        // Performance optimization: Handle key events for better UX
        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (keyData == Keys.Enter)
            {
                if (AccessTokenTaker.Focused)
                {
                    ProcessAccessToken();
                    return true;
                }
                else if (pathsTextBox.Focused)
                {
                    button1_Click_1(null, EventArgs.Empty);
                    return true;
                }
            }
            
            return base.ProcessDialogKey(keyData);
        }
    }
}