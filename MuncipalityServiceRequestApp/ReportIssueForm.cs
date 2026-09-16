using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MuncipalityServiceRequestApp
{
    public class ReportIssueForm : Form
    {
        private TextBox txtLocation;
        private ComboBox cboCategory;
        private RichTextBox rtbDescription;
        private Button btnAttach;
        private Label lblAttachedFiles;
        private ProgressBar progressBar;
        private Label lblEngagement;
        private Button btnSubmit;
        private Button btnBack;

        private readonly List<string> attachedFilePaths = new List<string>();

        public ReportIssueForm()
        {
            this.Text = "Report an Issue";
            this.Width = 520;
            this.Height = 620;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(480, 560);

            SetupUi();
        }

        private void SetupUi()
        {
            this.BackColor = AppTheme.LightBackground;
            Panel header = AppTheme.CreateHeaderPanel("CITY OF TSHWANE", "Report an Issue");

            int y = AppTheme.HeaderHeight + 20;
            int labelHeight = 20;
            int spacing = 10;

            Label lblLocation = new Label { Text = "Location:", Left = 20, Top = y, Width = 460, Height = labelHeight, ForeColor = AppTheme.Black };
            y += labelHeight + 2;
            txtLocation = new TextBox { Left = 20, Top = y, Width = 460, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            txtLocation.TextChanged += OnInputChanged;
            y += 30 + spacing;

            Label lblCategory = new Label { Text = "Category:", Left = 20, Top = y, Width = 460, Height = labelHeight, ForeColor = AppTheme.Black };
            y += labelHeight + 2;
            cboCategory = new ComboBox { Left = 20, Top = y, Width = 460, DropDownStyle = ComboBoxStyle.DropDownList, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            cboCategory.Items.AddRange(new object[] { "Sanitation", "Roads", "Water & Utilities", "Electricity", "Public Safety", "Other" });
            cboCategory.SelectedIndexChanged += OnInputChanged;
            y += 30 + spacing;

            Label lblDescription = new Label { Text = "Description:", Left = 20, Top = y, Width = 460, Height = labelHeight, ForeColor = AppTheme.Black };
            y += labelHeight + 2;
            rtbDescription = new RichTextBox { Left = 20, Top = y, Width = 460, Height = 120, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            rtbDescription.TextChanged += OnInputChanged;
            y += 120 + spacing;

            btnAttach = new Button { Text = "Attach Image/Document", Left = 20, Top = y, Width = 200, Height = 32 };
            AppTheme.StyleSecondaryButton(btnAttach);
            btnAttach.Click += BtnAttach_Click;
            lblAttachedFiles = new Label { Text = "No files attached.", Left = 230, Top = y + 7, Width = 250, ForeColor = Color.Gray };
            y += 32 + spacing;

            progressBar = new ProgressBar { Left = 20, Top = y, Width = 460, Height = 20, Minimum = 0, Maximum = 100, Value = 0, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            y += 20 + 4;
            lblEngagement = new Label
            {
                Text = "Let's get started, tell us where the issue is.",
                Left = 20,
                Top = y,
                Width = 460,
                Height = 20,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Italic),
                ForeColor = AppTheme.Green
            };
            y += 20 + spacing + 10;

            btnSubmit = new Button { Text = "Submit", Left = 20, Top = y, Width = 220, Height = 40, Enabled = false };
            AppTheme.StyleDisabledButton(btnSubmit); // starts disable.
            btnSubmit.Click += BtnSubmit_Click;

            btnBack = new Button { Text = "Back to Main Menu", Left = 260, Top = y, Width = 220, Height = 40 };
            AppTheme.StyleSecondaryButton(btnBack);
            btnBack.Click += BtnBack_Click;

            this.Controls.AddRange(new Control[]
            {
        lblLocation, txtLocation,
        lblCategory, cboCategory,
        lblDescription, rtbDescription,
        btnAttach, lblAttachedFiles,
        progressBar, lblEngagement,
        btnSubmit, btnBack
            });
            this.Controls.Add(header);
        }

        private void OnInputChanged(object sender, EventArgs e)
        {
            int filled = 0;
            if (!string.IsNullOrWhiteSpace(txtLocation.Text)) filled++;
            if (cboCategory.SelectedIndex >= 0) filled++;
            if (!string.IsNullOrWhiteSpace(rtbDescription.Text)) filled++;

            int progress = filled * 33;
            progressBar.Value = Math.Min(progress, 100);

            switch (filled)
            {
                case 0:
                    lblEngagement.Text = "Let's get started, tell us where the issue is.";
                    break;
                case 1:
                    lblEngagement.Text = "Great start! Now select a category.";
                    break;
                case 2:
                    lblEngagement.Text = "Almost there! describe the issue.";
                    break;
                default:
                    lblEngagement.Text = "All set! You're ready to submit.";
                    break;
            }

            btnSubmit.Enabled = (filled == 3);
            if (btnSubmit.Enabled)
                AppTheme.StylePrimaryButton(btnSubmit);
            else
                AppTheme.StyleDisabledButton(btnSubmit);
        }

        private void BtnAttach_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "Select images or documents related to the issue";
                dialog.Filter = "Supported files (*.jpg;*.jpeg;*.png;*.pdf;*.docx)|*.jpg;*.jpeg;*.png;*.pdf;*.docx|All files (*.*)|*.*";
                dialog.Multiselect = true;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    attachedFilePaths.AddRange(dialog.FileNames);
                    RefreshAttachmentLabel();
                }
            }
        }

        private void RefreshAttachmentLabel()
        {
            if (attachedFilePaths.Count == 0)
            {
                lblAttachedFiles.Text = "No files attached.";
                lblAttachedFiles.ForeColor = Color.Gray;
            }
            else
            {
                string names = string.Join(", ", attachedFilePaths.Select(Path.GetFileName));
                lblAttachedFiles.Text = attachedFilePaths.Count + " file(s): " + names;
                lblAttachedFiles.ForeColor = Color.DarkGreen;
            }
        }

        private void BtnSubmit_Click(object sender, EventArgs e)
        {
           
            if (string.IsNullOrWhiteSpace(txtLocation.Text) ||
                cboCategory.SelectedIndex < 0 ||
                string.IsNullOrWhiteSpace(rtbDescription.Text))
            {
                MessageBox.Show(
                    "Please complete the location, category and description before submitting.",
                    "Missing Information",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            Issue issue = new Issue
            {
                Location = txtLocation.Text.Trim(),
                Category = cboCategory.SelectedItem.ToString(),
                Description = rtbDescription.Text.Trim(),
                AttachmentPaths = new List<string>(attachedFilePaths),
                DateReported = DateTime.Now,
                Status = "Logged"
            };

            IssueRepository.Issues.Add(issue);
            int referenceNumber = IssueRepository.Issues.Count;

            MessageBox.Show(
                "Thank you! Your issue has been logged successfully.\n\n" +
                "Reference #: " + referenceNumber + "\n" +
                "Status: " + issue.Status,
                "Report Submitted",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            ResetForm();
        }

        private void ResetForm()
        {
            txtLocation.Clear();
            cboCategory.SelectedIndex = -1;
            rtbDescription.Clear();
            attachedFilePaths.Clear();
            RefreshAttachmentLabel();
            progressBar.Value = 0;
            lblEngagement.Text = "Let's get started, tell us where the issue is.";
            btnSubmit.Enabled = false;
            txtLocation.Focus();
        }

        private void BtnBack_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}