using System;
using System.Windows.Forms;

namespace GitPuller
{
    public partial class Form1 : Form
    {
        private readonly ConfigurationManager _configManager;

        public Form1()
        {
            InitializeComponent();
            _configManager = new ConfigurationManager(this);
            CheckAccessToken();
        }

        private void CheckAccessToken()
        {
            string accessToken = _configManager.GetAccessToken();

            if (string.IsNullOrEmpty(accessToken))
            {
                AccessTokenTaker.Visible = true;
                EnterButton.Visible = true;
                AccessTokenLabel.Visible = true;
                StartPulling.Visible = false;
                StartPulling.Enabled = false;
            }
            else
            {
                AccessTokenTaker.Visible = false;
                EnterButton.Visible = false;
                AccessTokenLabel.Visible = false;
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

        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {

        }

        private void EnterButton_Click_1(object sender, EventArgs e)
        {
            string enteredToken = GetAccessTokenFromTextBox();
            _configManager.SetToken(enteredToken);
            CheckAccessToken();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            string newPath = pathsTextBox.Text;
            _configManager.SaveFilePaths(newPath);
        }
    }
}