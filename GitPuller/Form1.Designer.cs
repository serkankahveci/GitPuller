namespace GitPuller
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.StartPulling = new System.Windows.Forms.Button();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.AccessTokenTaker = new System.Windows.Forms.TextBox();
            this.EnterButton = new System.Windows.Forms.Button();
            this.AccessTokenLabel = new System.Windows.Forms.Label();
            this.RepoPathLabel = new System.Windows.Forms.Label();
            this.pathsTextBox = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // StartPulling
            // 
            resources.ApplyResources(this.StartPulling, "StartPulling");
            this.StartPulling.BackColor = System.Drawing.Color.Transparent;
            this.StartPulling.Cursor = System.Windows.Forms.Cursors.Hand;
            this.StartPulling.ForeColor = System.Drawing.Color.Black;
            this.StartPulling.Name = "StartPulling";
            this.StartPulling.UseVisualStyleBackColor = false;
            this.StartPulling.Click += new System.EventHandler(this.button1_Click);
            // 
            // treeView1
            // 
            resources.ApplyResources(this.treeView1, "treeView1");
            this.treeView1.Name = "treeView1";
            this.treeView1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterSelect);
            // 
            // AccessTokenTaker
            // 
            resources.ApplyResources(this.AccessTokenTaker, "AccessTokenTaker");
            this.AccessTokenTaker.Name = "AccessTokenTaker";
            this.AccessTokenTaker.Tag = "EnterButton";
            // 
            // EnterButton
            // 
            resources.ApplyResources(this.EnterButton, "EnterButton");
            this.EnterButton.Name = "EnterButton";
            this.EnterButton.UseMnemonic = false;
            this.EnterButton.UseVisualStyleBackColor = true;
            this.EnterButton.Click += new System.EventHandler(this.EnterButton_Click_1);
            // 
            // AccessTokenLabel
            // 
            resources.ApplyResources(this.AccessTokenLabel, "AccessTokenLabel");
            this.AccessTokenLabel.BackColor = System.Drawing.Color.Transparent;
            this.AccessTokenLabel.Name = "AccessTokenLabel";
            this.AccessTokenLabel.Click += new System.EventHandler(this.label1_Click);
            // 
            // RepoPathLabel
            // 
            resources.ApplyResources(this.RepoPathLabel, "RepoPathLabel");
            this.RepoPathLabel.BackColor = System.Drawing.Color.Transparent;
            this.RepoPathLabel.Name = "RepoPathLabel";
            // 
            // pathsTextBox
            // 
            resources.ApplyResources(this.pathsTextBox, "pathsTextBox");
            this.pathsTextBox.Name = "pathsTextBox";
            this.pathsTextBox.Tag = "EnterButton";
            // 
            // button1
            // 
            resources.ApplyResources(this.button1, "button1");
            this.button1.Name = "button1";
            this.button1.UseMnemonic = false;
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // Form1
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.Controls.Add(this.button1);
            this.Controls.Add(this.pathsTextBox);
            this.Controls.Add(this.RepoPathLabel);
            this.Controls.Add(this.AccessTokenTaker);
            this.Controls.Add(this.EnterButton);
            this.Controls.Add(this.treeView1);
            this.Controls.Add(this.StartPulling);
            this.Controls.Add(this.AccessTokenLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button StartPulling;
        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.TextBox AccessTokenTaker;
        private System.Windows.Forms.Button EnterButton;
        private System.Windows.Forms.Label AccessTokenLabel;
        private System.Windows.Forms.Label RepoPathLabel;
        private System.Windows.Forms.TextBox pathsTextBox;
        private System.Windows.Forms.Button button1;
    }
}