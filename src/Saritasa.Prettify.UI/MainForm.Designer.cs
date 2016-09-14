namespace Saritasa.Prettify.UI
{
    partial class MainForm
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
            this.openSolutionDialog = new System.Windows.Forms.OpenFileDialog();
            this.selectSolutionButton = new System.Windows.Forms.Button();
            this.selectedSolutionTextBox = new System.Windows.Forms.TextBox();
            this.OptionsGroupBox = new System.Windows.Forms.GroupBox();
            this.autoCompleteTextBox = new System.Windows.Forms.TextBox();
            this.openHelpUrlButton = new System.Windows.Forms.Button();
            this.viewDescriptionButton = new System.Windows.Forms.Button();
            this.runButton = new System.Windows.Forms.Button();
            this.selectAllCheckBox = new System.Windows.Forms.CheckBox();
            this.issuesChecked = new System.Windows.Forms.CheckedListBox();
            this.fixIssues = new System.Windows.Forms.CheckBox();
            this.outputGroupBox = new System.Windows.Forms.GroupBox();
            this.clearOutputButton = new System.Windows.Forms.Button();
            this.outputTextBox = new System.Windows.Forms.TextBox();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.currentStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.OptionsGroupBox.SuspendLayout();
            this.outputGroupBox.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // openSolutionDialog
            // 
            this.openSolutionDialog.DefaultExt = "sln";
            this.openSolutionDialog.Filter = "Solution files|*.sln";
            // 
            // selectSolutionButton
            // 
            this.selectSolutionButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.selectSolutionButton.Location = new System.Drawing.Point(438, 28);
            this.selectSolutionButton.Name = "selectSolutionButton";
            this.selectSolutionButton.Size = new System.Drawing.Size(123, 30);
            this.selectSolutionButton.TabIndex = 0;
            this.selectSolutionButton.Text = "Open Solution";
            this.selectSolutionButton.UseVisualStyleBackColor = true;
            this.selectSolutionButton.Click += new System.EventHandler(this.selectSolutionButton_Click);
            // 
            // selectedSolutionTextBox
            // 
            this.selectedSolutionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.selectedSolutionTextBox.Location = new System.Drawing.Point(12, 28);
            this.selectedSolutionTextBox.Multiline = true;
            this.selectedSolutionTextBox.Name = "selectedSolutionTextBox";
            this.selectedSolutionTextBox.ReadOnly = true;
            this.selectedSolutionTextBox.Size = new System.Drawing.Size(420, 30);
            this.selectedSolutionTextBox.TabIndex = 1;
            // 
            // OptionsGroupBox
            // 
            this.OptionsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.OptionsGroupBox.Controls.Add(this.autoCompleteTextBox);
            this.OptionsGroupBox.Controls.Add(this.openHelpUrlButton);
            this.OptionsGroupBox.Controls.Add(this.viewDescriptionButton);
            this.OptionsGroupBox.Controls.Add(this.runButton);
            this.OptionsGroupBox.Controls.Add(this.selectAllCheckBox);
            this.OptionsGroupBox.Controls.Add(this.issuesChecked);
            this.OptionsGroupBox.Controls.Add(this.fixIssues);
            this.OptionsGroupBox.Location = new System.Drawing.Point(13, 78);
            this.OptionsGroupBox.Name = "OptionsGroupBox";
            this.OptionsGroupBox.Size = new System.Drawing.Size(548, 313);
            this.OptionsGroupBox.TabIndex = 2;
            this.OptionsGroupBox.TabStop = false;
            this.OptionsGroupBox.Text = "Options";
            // 
            // autoCompleteTextBox
            // 
            this.autoCompleteTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.autoCompleteTextBox.Location = new System.Drawing.Point(20, 43);
            this.autoCompleteTextBox.Name = "autoCompleteTextBox";
            this.autoCompleteTextBox.Size = new System.Drawing.Size(508, 20);
            this.autoCompleteTextBox.TabIndex = 6;
            // 
            // openHelpUrlButton
            // 
            this.openHelpUrlButton.Enabled = false;
            this.openHelpUrlButton.Location = new System.Drawing.Point(146, 255);
            this.openHelpUrlButton.Name = "openHelpUrlButton";
            this.openHelpUrlButton.Size = new System.Drawing.Size(104, 30);
            this.openHelpUrlButton.TabIndex = 5;
            this.openHelpUrlButton.Text = "Open Help Url";
            this.openHelpUrlButton.UseVisualStyleBackColor = true;
            this.openHelpUrlButton.Click += new System.EventHandler(this.openHelpUrlButton_Click);
            // 
            // viewDescriptionButton
            // 
            this.viewDescriptionButton.Enabled = false;
            this.viewDescriptionButton.Location = new System.Drawing.Point(20, 255);
            this.viewDescriptionButton.Name = "viewDescriptionButton";
            this.viewDescriptionButton.Size = new System.Drawing.Size(120, 30);
            this.viewDescriptionButton.TabIndex = 4;
            this.viewDescriptionButton.Text = "View Description";
            this.viewDescriptionButton.UseVisualStyleBackColor = true;
            this.viewDescriptionButton.Click += new System.EventHandler(this.viewDescriptionButton_Click);
            // 
            // runButton
            // 
            this.runButton.AccessibleDescription = "";
            this.runButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.runButton.Enabled = false;
            this.runButton.Location = new System.Drawing.Point(453, 255);
            this.runButton.Name = "runButton";
            this.runButton.Size = new System.Drawing.Size(75, 30);
            this.runButton.TabIndex = 3;
            this.runButton.Text = "Run!";
            this.runButton.UseVisualStyleBackColor = true;
            this.runButton.Click += new System.EventHandler(this.runButton_Click);
            // 
            // selectAllCheckBox
            // 
            this.selectAllCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.selectAllCheckBox.AutoSize = true;
            this.selectAllCheckBox.Location = new System.Drawing.Point(108, 19);
            this.selectAllCheckBox.Name = "selectAllCheckBox";
            this.selectAllCheckBox.Size = new System.Drawing.Size(75, 17);
            this.selectAllCheckBox.TabIndex = 2;
            this.selectAllCheckBox.Text = "Select all?";
            this.selectAllCheckBox.UseVisualStyleBackColor = true;
            this.selectAllCheckBox.CheckedChanged += new System.EventHandler(this.selectAllCheckBox_CheckedChanged);
            // 
            // issuesChecked
            // 
            this.issuesChecked.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.issuesChecked.FormattingEnabled = true;
            this.issuesChecked.Location = new System.Drawing.Point(20, 69);
            this.issuesChecked.Name = "issuesChecked";
            this.issuesChecked.Size = new System.Drawing.Size(508, 184);
            this.issuesChecked.TabIndex = 1;
            this.issuesChecked.SelectedIndexChanged += new System.EventHandler(this.issuesChecked_SelectedIndexChanged);
            // 
            // fixIssues
            // 
            this.fixIssues.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fixIssues.AutoSize = true;
            this.fixIssues.Location = new System.Drawing.Point(20, 19);
            this.fixIssues.Name = "fixIssues";
            this.fixIssues.Size = new System.Drawing.Size(78, 17);
            this.fixIssues.TabIndex = 0;
            this.fixIssues.Text = "Fix Issues?";
            this.fixIssues.UseVisualStyleBackColor = true;
            // 
            // outputGroupBox
            // 
            this.outputGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.outputGroupBox.Controls.Add(this.clearOutputButton);
            this.outputGroupBox.Controls.Add(this.outputTextBox);
            this.outputGroupBox.Location = new System.Drawing.Point(13, 397);
            this.outputGroupBox.Name = "outputGroupBox";
            this.outputGroupBox.Size = new System.Drawing.Size(548, 338);
            this.outputGroupBox.TabIndex = 3;
            this.outputGroupBox.TabStop = false;
            this.outputGroupBox.Text = "Output";
            // 
            // clearOutputButton
            // 
            this.clearOutputButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.clearOutputButton.Location = new System.Drawing.Point(453, 286);
            this.clearOutputButton.Name = "clearOutputButton";
            this.clearOutputButton.Size = new System.Drawing.Size(75, 30);
            this.clearOutputButton.TabIndex = 5;
            this.clearOutputButton.Text = "Clear Output";
            this.clearOutputButton.UseVisualStyleBackColor = true;
            this.clearOutputButton.Click += new System.EventHandler(this.clearOutputButton_Click);
            // 
            // outputTextBox
            // 
            this.outputTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.outputTextBox.Cursor = System.Windows.Forms.Cursors.No;
            this.outputTextBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.outputTextBox.Location = new System.Drawing.Point(7, 20);
            this.outputTextBox.Multiline = true;
            this.outputTextBox.Name = "outputTextBox";
            this.outputTextBox.ReadOnly = true;
            this.outputTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.outputTextBox.Size = new System.Drawing.Size(521, 260);
            this.outputTextBox.TabIndex = 0;
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.currentStatusLabel});
            this.statusStrip.Location = new System.Drawing.Point(0, 738);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(575, 22);
            this.statusStrip.TabIndex = 4;
            this.statusStrip.Text = "statusStrip1";
            // 
            // currentStatusLabel
            // 
            this.currentStatusLabel.Name = "currentStatusLabel";
            this.currentStatusLabel.Size = new System.Drawing.Size(0, 17);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(575, 760);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.outputGroupBox);
            this.Controls.Add(this.OptionsGroupBox);
            this.Controls.Add(this.selectedSolutionTextBox);
            this.Controls.Add(this.selectSolutionButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.Text = "Saritasa Prettify";
            this.OptionsGroupBox.ResumeLayout(false);
            this.OptionsGroupBox.PerformLayout();
            this.outputGroupBox.ResumeLayout(false);
            this.outputGroupBox.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog openSolutionDialog;
        private System.Windows.Forms.Button selectSolutionButton;
        private System.Windows.Forms.TextBox selectedSolutionTextBox;
        private System.Windows.Forms.GroupBox OptionsGroupBox;
        private System.Windows.Forms.CheckedListBox issuesChecked;
        private System.Windows.Forms.CheckBox fixIssues;
        private System.Windows.Forms.GroupBox outputGroupBox;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel currentStatusLabel;
        private System.Windows.Forms.CheckBox selectAllCheckBox;
        private System.Windows.Forms.Button runButton;
        private System.Windows.Forms.TextBox outputTextBox;
        private System.Windows.Forms.Button clearOutputButton;
        private System.Windows.Forms.Button viewDescriptionButton;
        private System.Windows.Forms.Button openHelpUrlButton;
        private System.Windows.Forms.TextBox autoCompleteTextBox;
    }
}

