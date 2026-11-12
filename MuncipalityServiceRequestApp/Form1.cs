using System;
using System.Drawing;
using System.Windows.Forms;

namespace MuncipalityServiceRequestApp
{
    /// <summary>Main menu: the first screen shown on startup.</summary>
    public partial class Form1 : Form
    {
        private Label footerLabel;

        public Form1()
        {
            InitializeComponent();
            AppTheme.InitForm(this, "Municipal Services Application - Main Menu", 560, 520, 420, 440);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SetupMainMenu();
        }

        private void SetupMainMenu()
        {
            this.Controls.Clear();

            // Three columns keep the button stack centred at any window width.
            TableLayoutPanel layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1 };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 340F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            FlowLayoutPanel menu = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(0, AppTheme.SpaceLg, 0, 0)
            };
            menu.Controls.Add(CreateMenuButton("Report Issues", BtnReportIssues_Click));
            menu.Controls.Add(CreateMenuButton("Local Events/Announcements", BtnLocalEvents_Click));
            menu.Controls.Add(CreateMenuButton("Service Request Status", BtnServiceStatus_Click));
            menu.Controls.Add(CreateMenuButton("Admin Portal", BtnAdminPortal_Click));
            layout.Controls.Add(menu, 1, 0);

            footerLabel = new Label
            {
                AutoSize = false,
                UseMnemonic = false, // so "&" in text is shown, not treated as a shortcut key
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Bottom,
                Height = 40,
                ForeColor = Color.DimGray,
                Font = AppTheme.FontCaption
            };

            // Add order matters for docking: fill first, header last (so it docks to the very top).
            this.Controls.Add(layout);
            this.Controls.Add(footerLabel);
            this.Controls.Add(AppTheme.CreateHeaderPanel("CITY OF TSHWANE", "Municipal Services Application"));

            // Forms are hidden (not closed) while a feature is open, so refresh the summary when we come back.
            this.VisibleChanged += (s, args) => { if (this.Visible) RefreshFooter(); };
            RefreshFooter();
        }

        private void RefreshFooter()
        {
            int open = IssueRepository.OpenCount();
            footerLabel.Text = "Select an option above.   Open service requests: " + open +
                               "   |   Events & announcements: " + EventRepository.Events.Count;
        }

        private Button CreateMenuButton(string text, EventHandler clickHandler)
        {
            Button btn = new Button
            {
                Text = text,
                Width = 320,
                Height = 52,
                Margin = new Padding(0, 0, 0, AppTheme.SpaceMd)
            };
            AppTheme.StylePrimaryButton(btn);
            btn.Click += clickHandler;
            return btn;
        }

        /// <summary>Opens a feature form and hides the menu until it closes.</summary>
        private void OpenFeature(Form feature)
        {
            feature.FormClosed += (s, args) => this.Show();
            this.Hide();
            feature.Show();
        }

        private void BtnReportIssues_Click(object sender, EventArgs e)
        {
            OpenFeature(new ReportIssueForm());
        }

        private void BtnLocalEvents_Click(object sender, EventArgs e)
        {
            OpenFeature(new LocalEventsForm());
        }

        private void BtnServiceStatus_Click(object sender, EventArgs e)
        {
            OpenFeature(new ServiceStatusForm());
        }

        private void BtnAdminPortal_Click(object sender, EventArgs e)
        {
            OpenFeature(new AdminPortalForm());
        }
    }
}