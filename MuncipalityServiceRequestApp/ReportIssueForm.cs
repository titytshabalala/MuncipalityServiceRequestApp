using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace MuncipalityServiceRequestApp
{
    public class ReportIssueForm : Form
    {
        private const int MinDescriptionLength = 10;
        private const int MaxDescriptionLength = 1000;
        private const int MaxAttachments = 5;
        private const long MaxAttachmentBytes = 10L * 1024 * 1024; // 10 MB
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".pdf", ".docx" };
        private static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png" };

        private ComboBox cboLocation;
        private ComboBox cboCategory;
        private TextBox txtDescription;
        private Label lblCharCount;
        private Button btnAttach;
        private Button btnRemove;
        private Label lblAttachInfo;
        private ListBox lstAttachments;
        private PictureBox picPreview;
        private ProgressBar progressBar;
        private Label lblEngagement;
        private Button btnSubmit;
        private Button btnBack;

        // The list of files the user has attached to the report currently being written.
        private readonly List<string> attachedFilePaths = new List<string>();

        public ReportIssueForm()
        {
            AppTheme.InitForm(this, "Report an Issue", 560, 720, 460, 520);
            BuildUi();
            OnInputChanged(this, EventArgs.Empty);
            this.FormClosing += ReportIssueForm_FormClosing;
        }

        private static Label FieldLabel(string text)
        {
            return new Label { Text = text, AutoSize = true, Font = AppTheme.FontLabel, ForeColor = AppTheme.Black, Margin = new Padding(0, 0, 0, 3) };
        }

        private void BuildUi()
        {
            Panel body = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(AppTheme.SpaceLg, AppTheme.SpaceMd, AppTheme.SpaceLg, AppTheme.SpaceMd)
            };
            TableLayoutPanel form = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 1
            };
            form.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            AnchorStyles stretch = AnchorStyles.Left | AnchorStyles.Right;
            Padding gap = new Padding(0, 0, 0, AppTheme.SpaceMd);

            // --- Location (type freely, or pick a known service zone) ---
            cboLocation = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDown,
                Anchor = stretch,
                Margin = gap,
                MaxLength = 120,
                AutoCompleteMode = AutoCompleteMode.SuggestAppend,
                AutoCompleteSource = AutoCompleteSource.ListItems
            };
            cboLocation.Items.AddRange(ServiceZoneGraph.Default.Zones.Cast<object>().ToArray());
            cboLocation.TextChanged += OnInputChanged;
            form.Controls.Add(FieldLabel("Location *  (street address or area, e.g. \"12 Church St, Sunnyside\")"));
            form.Controls.Add(cboLocation);

            // --- Category ---
            cboCategory = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Anchor = stretch, Margin = gap };
            cboCategory.Items.AddRange(new object[] { "Sanitation", "Roads", "Water & Utilities", "Electricity", "Public Safety", "Other" });
            cboCategory.SelectedIndexChanged += OnInputChanged;
            form.Controls.Add(FieldLabel("Category *"));
            form.Controls.Add(cboCategory);

            // --- Description ---
            txtDescription = new TextBox
            {
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                AcceptsReturn = true,
                MaxLength = MaxDescriptionLength,
                Height = 110,
                Anchor = stretch,
                Margin = new Padding(0, 0, 0, 2)
            };
            txtDescription.TextChanged += OnInputChanged;
            lblCharCount = new Label { AutoSize = true, Anchor = AnchorStyles.Right, ForeColor = Color.DimGray, Font = AppTheme.FontCaption, Margin = gap };
            form.Controls.Add(FieldLabel("Description * (at least " + MinDescriptionLength + " characters)"));
            form.Controls.Add(txtDescription);
            form.Controls.Add(lblCharCount);

            // --- Attachments ---
            form.Controls.Add(FieldLabel("Attachments (optional: JPG, PNG, PDF or DOCX, up to " + MaxAttachments + " files of 10 MB each)"));

            FlowLayoutPanel attachButtons = new FlowLayoutPanel { AutoSize = true, Anchor = stretch, Margin = new Padding(0, 0, 0, 6), WrapContents = true };
            btnAttach = new Button { Text = "Attach Image/Document", Width = 200, Height = 32 };
            AppTheme.StyleSecondaryButton(btnAttach);
            btnAttach.Click += BtnAttach_Click;
            btnRemove = new Button { Text = "Remove Selected", Width = 150, Height = 32, Enabled = false };
            AppTheme.StyleSecondaryButton(btnRemove);
            btnRemove.Click += BtnRemove_Click;
            lblAttachInfo = new Label { AutoSize = true, ForeColor = Color.Gray, Margin = new Padding(8, 8, 0, 0) };
            attachButtons.Controls.Add(btnAttach);
            attachButtons.Controls.Add(btnRemove);
            attachButtons.Controls.Add(lblAttachInfo);
            form.Controls.Add(attachButtons);

            TableLayoutPanel attachArea = new TableLayoutPanel { Anchor = stretch, AutoSize = true, ColumnCount = 2, RowCount = 1, Margin = gap };
            attachArea.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            attachArea.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
            lstAttachments = new ListBox { Anchor = stretch, Height = 96, IntegralHeight = false, Margin = new Padding(0, 0, 8, 0) };
            lstAttachments.SelectedIndexChanged += (s, e) => { btnRemove.Enabled = lstAttachments.SelectedIndex >= 0; UpdatePreview(); };
            picPreview = new PictureBox { Size = new Size(142, 96), SizeMode = PictureBoxSizeMode.Zoom, BorderStyle = BorderStyle.FixedSingle, BackColor = Color.White, Margin = Padding.Empty };
            attachArea.Controls.Add(lstAttachments, 0, 0);
            attachArea.Controls.Add(picPreview, 1, 0);
            form.Controls.Add(attachArea);

            // --- Live progress / engagement feedback ---
            progressBar = new ProgressBar { Anchor = stretch, Height = 20, Minimum = 0, Maximum = 100, Margin = new Padding(0, 0, 0, 4) };
            lblEngagement = new Label { AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Italic), ForeColor = AppTheme.Green, Margin = gap };
            form.Controls.Add(progressBar);
            form.Controls.Add(lblEngagement);

            // --- Buttons ---
            TableLayoutPanel buttons = new TableLayoutPanel { Anchor = stretch, AutoSize = true, ColumnCount = 2, RowCount = 1 };
            buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            btnSubmit = new Button { Text = "Submit", Height = 42, Dock = DockStyle.Fill, Margin = new Padding(0, 0, 6, 0) };
            AppTheme.StyleDisabledButton(btnSubmit);
            btnSubmit.Click += BtnSubmit_Click;
            btnBack = new Button { Text = "Back to Main Menu", Height = 42, Dock = DockStyle.Fill, Margin = new Padding(6, 0, 0, 0) };
            AppTheme.StyleSecondaryButton(btnBack);
            btnBack.Click += (s, e) => this.Close();
            buttons.Controls.Add(btnSubmit, 0, 0);
            buttons.Controls.Add(btnBack, 1, 0);
            form.Controls.Add(buttons);

            body.Controls.Add(form);
            // Fill first, header last so the header docks to the very top.
            this.Controls.Add(body);
            this.Controls.Add(AppTheme.CreateHeaderPanel("CITY OF TSHWANE", "Report an Issue"));
        }

        // ---------- validation + live feedback ----------

        private bool LocationFilled { get { return !string.IsNullOrWhiteSpace(cboLocation.Text); } }
        private bool CategoryFilled { get { return cboCategory.SelectedIndex >= 0; } }
        private bool DescriptionFilled { get { return txtDescription.Text.Trim().Length >= MinDescriptionLength; } }

        private void OnInputChanged(object sender, EventArgs e)
        {
            int filled = (LocationFilled ? 1 : 0) + (CategoryFilled ? 1 : 0) + (DescriptionFilled ? 1 : 0);

            progressBar.Value = filled == 3 ? 100 : filled * 33;
            lblCharCount.Text = txtDescription.Text.Length + " / " + MaxDescriptionLength;

            if (filled == 0)
            {
                lblEngagement.Text = "Let's get started: tell us where the issue is.";
            }
            else if (filled == 3)
            {
                lblEngagement.Text = "All set! You're ready to submit.";
            }
            else
            {
                List<string> missing = new List<string>();
                if (!LocationFilled) missing.Add("location");
                if (!CategoryFilled) missing.Add("category");
                if (!DescriptionFilled) missing.Add("a description (" + MinDescriptionLength + "+ characters)");
                lblEngagement.Text = (filled == 1 ? "Great start! " : "Almost there! ") + "Still needed: " + string.Join(", ", missing) + ".";
            }

            AppTheme.SetPrimaryEnabled(btnSubmit, filled == 3);
        }

        private bool HasUnsavedInput
        {
            get { return LocationFilled || CategoryFilled || txtDescription.Text.Trim().Length > 0 || attachedFilePaths.Count > 0; }
        }

        private void ReportIssueForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing && HasUnsavedInput)
            {
                DialogResult answer = MessageBox.Show(
                    "You have a report that has not been submitted. Leave without submitting?",
                    "Discard report?",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (answer != DialogResult.Yes) e.Cancel = true;
            }
        }

        // ---------- attachments ----------

        private void BtnAttach_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "Select images or documents related to the issue";
                dialog.Filter = "Supported files (*.jpg;*.jpeg;*.png;*.pdf;*.docx)|*.jpg;*.jpeg;*.png;*.pdf;*.docx";
                dialog.Multiselect = true;
                dialog.CheckFileExists = true;

                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    AddAttachments(dialog.FileNames);
                }
            }
        }

        private void AddAttachments(IEnumerable<string> paths)
        {
            List<string> problems = new List<string>();

            foreach (string path in paths)
            {
                string name = Path.GetFileName(path);
                try
                {
                    if (attachedFilePaths.Count >= MaxAttachments)
                    {
                        problems.Add(name + ": the limit of " + MaxAttachments + " files has been reached.");
                        continue;
                    }
                    if (!AllowedExtensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase))
                    {
                        problems.Add(name + ": this file type is not supported.");
                        continue;
                    }
                    if (attachedFilePaths.Contains(path, StringComparer.OrdinalIgnoreCase))
                    {
                        problems.Add(name + ": already attached.");
                        continue;
                    }

                    FileInfo info = new FileInfo(path);
                    if (!info.Exists) { problems.Add(name + ": the file could not be found."); continue; }
                    if (info.Length == 0) { problems.Add(name + ": the file is empty."); continue; }
                    if (info.Length > MaxAttachmentBytes) { problems.Add(name + ": larger than 10 MB."); continue; }

                    attachedFilePaths.Add(path);
                }
                catch (Exception ex)
                {
                    if (ex is IOException || ex is UnauthorizedAccessException || ex is ArgumentException || ex is NotSupportedException)
                        problems.Add(name + ": cannot be read (" + ex.Message + ").");
                    else
                        throw;
                }
            }

            RefreshAttachmentList();

            if (problems.Count > 0)
            {
                MessageBox.Show("Some files were not attached:\n\n" + string.Join("\n", problems), "Attachment problem", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnRemove_Click(object sender, EventArgs e)
        {
            int index = lstAttachments.SelectedIndex;
            if (index < 0 || index >= attachedFilePaths.Count) return;
            attachedFilePaths.RemoveAt(index);
            RefreshAttachmentList();
        }

        private void RefreshAttachmentList()
        {
            lstAttachments.Items.Clear();
            foreach (string path in attachedFilePaths) lstAttachments.Items.Add(Path.GetFileName(path));

            lblAttachInfo.Text = attachedFilePaths.Count == 0
                ? "No files attached."
                : attachedFilePaths.Count + " of " + MaxAttachments + " files attached.";
            lblAttachInfo.ForeColor = attachedFilePaths.Count == 0 ? Color.Gray : Color.DarkGreen;

            if (lstAttachments.Items.Count > 0) lstAttachments.SelectedIndex = lstAttachments.Items.Count - 1;
            btnRemove.Enabled = lstAttachments.SelectedIndex >= 0;
            UpdatePreview();
        }

        private void UpdatePreview()
        {
            Image old = picPreview.Image;
            picPreview.Image = null;
            if (old != null) old.Dispose();

            int index = lstAttachments.SelectedIndex;
            if (index < 0 || index >= attachedFilePaths.Count) return;

            string path = attachedFilePaths[index];
            if (!ImageExtensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase)) return; // PDF/DOCX: no preview

            try
            {
                // Load a copy so the file on disk is not locked while the form is open.
                using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (Image original = Image.FromStream(stream))
                {
                    picPreview.Image = new Bitmap(original);
                }
            }
            catch (Exception ex)
            {
                // Corrupt image, file moved or locked: keep the attachment but show no preview.
                if (!(ex is IOException || ex is UnauthorizedAccessException || ex is OutOfMemoryException || ex is ArgumentException))
                    throw;
            }
        }

        // ---------- submit ----------

        private void BtnSubmit_Click(object sender, EventArgs e)
        {
            if (!LocationFilled || !CategoryFilled || !DescriptionFilled)
            {
                MessageBox.Show(
                    "Please complete the location, category and a description of at least " + MinDescriptionLength + " characters.",
                    "Missing Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Attachments are stored as paths, so make sure none were moved or deleted since they were added.
            List<string> missing = attachedFilePaths.Where(p => !File.Exists(p)).ToList();
            if (missing.Count > 0)
            {
                foreach (string path in missing) attachedFilePaths.Remove(path);
                RefreshAttachmentList();
                MessageBox.Show(
                    "These attachments can no longer be found and were removed:\n\n" + string.Join("\n", missing.Select(Path.GetFileName)) +
                    "\n\nAttach them again if you still need them, then submit.",
                    "Attachment missing", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            AppTheme.SetPrimaryEnabled(btnSubmit, false); // guards against a double click creating two reports
            try
            {
                Issue issue = IssueRepository.Add(cboLocation.Text, cboCategory.SelectedItem.ToString(), txtDescription.Text, attachedFilePaths);
                AppDataStore.Save();

                string zoneNote = issue.Zone != null
                    ? "Service zone: " + issue.Zone
                    : "Service zone: not recognised (a coordinator will route this request)";

                MessageBox.Show(
                    "Thank you! Your issue has been logged successfully.\n\n" +
                    "Reference #: " + issue.ReferenceNumber + "\n" +
                    "Priority: " + issue.Priority + "\n" +
                    "Status: " + Issue.StatusText(issue.Status) + "\n" +
                    zoneNote + "\n" +
                    "Attachments: " + issue.AttachmentPaths.Count + "\n\n" +
                    "Use \"Service Request Status\" on the main menu to track it.",
                    "Report Submitted", MessageBoxButtons.OK, MessageBoxIcon.Information);

                ResetForm();
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, "Could not submit", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                OnInputChanged(this, EventArgs.Empty);
            }
        }

        private void ResetForm()
        {
            cboLocation.Text = string.Empty;
            cboCategory.SelectedIndex = -1;
            txtDescription.Clear();
            attachedFilePaths.Clear();
            RefreshAttachmentList();
            OnInputChanged(this, EventArgs.Empty);
            cboLocation.Focus();
        }
    }
}