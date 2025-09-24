using System;
using System.Linq;
using System.Windows.Forms;

namespace GitPuller
{
    public partial class Form1 : Form
    {
        private readonly ConfigurationManager _configManager;

        // Additional UI controls for remove functionality
        private Button removeTokenButton;
        private ListBox pathsListBox;
        private Button removeSelectedPathButton;
        private Button clearAllPathsButton;

        public Form1()
        {
            InitializeComponent();
            _configManager = new ConfigurationManager(this);
            AddRemoveFunctionalityControls();
            CheckAccessToken();
            RefreshPathsList();
        }

        private void AddRemoveFunctionalityControls()
        {
            // Remove Token Button - position it next to the Enter button
            removeTokenButton = new Button
            {
                Text = "Remove Token",
                Location = new System.Drawing.Point(118, 136), // Position it next to existing controls
                Size = new System.Drawing.Size(90, 20),
                Font = new System.Drawing.Font("Arial Rounded MT Bold", 7.75f),
                UseVisualStyleBackColor = true
            };
            removeTokenButton.Click += RemoveTokenButton_Click;
            this.Controls.Add(removeTokenButton);

            // Paths ListBox (to show current paths)
            pathsListBox = new ListBox
            {
                Location = new System.Drawing.Point(12, 185),
                Size = new System.Drawing.Size(390, 80),
                Font = new System.Drawing.Font("Arial Rounded MT Bold", 8.25f)
            };
            this.Controls.Add(pathsListBox);

            // Remove Selected Path Button
            removeSelectedPathButton = new Button
            {
                Text = "Remove Selected",
                Location = new System.Drawing.Point(408, 185),
                Size = new System.Drawing.Size(76, 25),
                Font = new System.Drawing.Font("Arial Rounded MT Bold", 7.75f),
                UseVisualStyleBackColor = true
            };
            removeSelectedPathButton.Click += RemoveSelectedPathButton_Click;
            this.Controls.Add(removeSelectedPathButton);

            // Clear All Paths Button
            clearAllPathsButton = new Button
            {
                Text = "Clear All",
                Location = new System.Drawing.Point(408, 215),
                Size = new System.Drawing.Size(76, 25),
                Font = new System.Drawing.Font("Arial Rounded MT Bold", 7.75f),
                UseVisualStyleBackColor = true
            };
            clearAllPathsButton.Click += ClearAllPathsButton_Click;
            this.Controls.Add(clearAllPathsButton);

            // Adjust the TreeView position to accommodate new controls
            treeView1.Location = new System.Drawing.Point(0, 275);
            treeView1.Size = new System.Drawing.Size(496, 152);

            // Adjust form size
            this.ClientSize = new System.Drawing.Size(496, 427);
        }

        private void RemoveTokenButton_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to remove the access token?",
                "Confirm Remove",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                _configManager.RemoveAccessToken();
                MessageBox.Show("Access token removed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                CheckAccessToken(); // Refresh the UI to show the token input fields again
            }
        }

        private void RemoveSelectedPathButton_Click(object sender, EventArgs e)
        {
            if (pathsListBox.SelectedItem != null)
            {
                string selectedPath = pathsListBox.SelectedItem.ToString();

                var result = MessageBox.Show(
                    $"Are you sure you want to remove this path?\n\n{selectedPath}",
                    "Confirm Remove",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    _configManager.RemovePath(selectedPath);
                    MessageBox.Show("Path removed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    RefreshPathsList();
                }
            }
            else
            {
                MessageBox.Show("Please select a path to remove.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ClearAllPathsButton_Click(object sender, EventArgs e)
        {
            if (pathsListBox.Items.Count == 0)
            {
                MessageBox.Show("No paths to clear.", "No Paths", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var result = MessageBox.Show(
                "Are you sure you want to remove ALL paths?",
                "Confirm Clear All",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                _configManager.ClearAllPaths();
                MessageBox.Show("All paths cleared successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                RefreshPathsList();
            }
        }

        private void RefreshPathsList()
        {
            pathsListBox.Items.Clear();
            var paths = _configManager.GetAllPathsForDisplay();

            foreach (var path in paths)
            {
                pathsListBox.Items.Add(path);
            }

            // Enable/disable remove buttons based on whether there are paths
            removeSelectedPathButton.Enabled = pathsListBox.Items.Count > 0;
            clearAllPathsButton.Enabled = pathsListBox.Items.Count > 0;
        }

        private void CheckAccessToken()
        {
            string accessToken = _configManager.GetAccessToken();

            if (string.IsNullOrEmpty(accessToken))
            {
                AccessTokenTaker.Visible = true;
                EnterButton.Visible = true;
                AccessTokenLabel.Visible = true;
                removeTokenButton.Visible = false; // Hide when no token
                StartPulling.Visible = false;
                StartPulling.Enabled = false;
            }
            else
            {
                AccessTokenTaker.Visible = false;
                EnterButton.Visible = false;
                AccessTokenLabel.Visible = false;
                removeTokenButton.Visible = true; // Show when token exists
                StartPulling.Visible = true;
                StartPulling.Enabled = true;
            }
        }

        public string GetAccessTokenFromTextBox()
        {
            return AccessTokenTaker.Text;
        }

        public void UpdateTreeView(TreeNode node)
        {
            treeView1.Nodes.Add(node);
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            string accessToken = _configManager.GetAccessToken();

            var githubOps = new GithubOperation(accessToken, this);

            await githubOps.GetGithubAllRepositoryAndBranches();
        }

        private void EnterButton_Click(object sender, EventArgs e)
        {
            string enteredToken = GetAccessTokenFromTextBox();
            _configManager.SetToken(enteredToken);
            CheckAccessToken();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            RefreshPathsList();
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {

        }

        private void EnterButton_Click_1(object sender, EventArgs e)
        {
            string enteredToken = GetAccessTokenFromTextBox();
            if (!string.IsNullOrWhiteSpace(enteredToken))
            {
                _configManager.SetToken(enteredToken);
                CheckAccessToken();
                AccessTokenTaker.Clear(); // Clear the textbox after saving
            }
            else
            {
                MessageBox.Show("Please enter a valid access token.", "Invalid Token", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            string newPath = pathsTextBox.Text;
            if (!string.IsNullOrWhiteSpace(newPath))
            {
                _configManager.SaveFilePaths(newPath);
                pathsTextBox.Clear(); // Clear the textbox after adding
                RefreshPathsList(); // Refresh the list to show the new path
                MessageBox.Show("Path added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Please enter a valid path.", "Invalid Path", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}