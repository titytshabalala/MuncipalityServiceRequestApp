using System;
using System.Drawing;
using System.Windows.Forms;

namespace MuncipalityServiceRequestApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SetupMainMenu();
        }

        private void SetupMainMenu()
        {
            // Form setup
            this.Text = "Municipal Services Application - Main Menu";
            this.Width = 480;
            this.Height = 460;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = AppTheme.LightBackground;
            this.Controls.Clear(); // Clear controls to avoid duplicate layering on reload

            // 1. Fixed Header Panel
            Panel header = AppTheme.CreateHeaderPanel("CITY OF TSHWANE", "Municipal Services Application");
            this.Controls.Add(header);

            // 2. Footer Label (Updated for active features)
            Label footerLabel = new Label
            {
                Text = "Select an option above to manage municipal services.",
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Bottom,
                Height = 35,
                ForeColor = Color.DimGray,
                Font = AppTheme.FontCaption
            };
            this.Controls.Add(footerLabel);

            // 3. FlowLayoutPanel Container (Prevents overlapping and handles dynamic positioning)
            FlowLayoutPanel menuContainer = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(0, AppTheme.HeaderHeight + AppTheme.SpaceLg, 0, AppTheme.SpaceLg),
                BackColor = AppTheme.LightBackground
            };

            // 4. Create and Style Dashboard Buttons
            Button btnReportIssues = CreateMenuButton("Report Issues", BtnReportIssues_Click);
            Button btnLocalEvents = CreateMenuButton("Local Events/Announcements", BtnLocalEvents_Click);

            // Enabled and Activated "Service Request Status"
            Button btnServiceStatus = CreateMenuButton("Service Request Status", BtnServiceStatus_Click);

            // Add buttons to the flow container
            menuContainer.Controls.Add(btnReportIssues);
            menuContainer.Controls.Add(btnLocalEvents);
            menuContainer.Controls.Add(btnServiceStatus);

            // Add flow layout to form and ensure header stays on top
            this.Controls.Add(menuContainer);
            header.BringToFront();
        }

        /// <summary>
        /// Helper to create uniformly styled, centered buttons with bottom margins to prevent overlaps.
        /// </summary>
        private Button CreateMenuButton(string text, EventHandler clickHandler)
        {
            Button btn = new Button
            {
                Text = text,
                Width = 280,
                Height = 48,
                // Center the button horizontally in the 480px parent window ( (480 - 280) / 2 = 100 )
                Margin = new Padding(85, 0, 85, AppTheme.SpaceMd)
            };

            AppTheme.StylePrimaryButton(btn);
            btn.Click += clickHandler;
            return btn;
        }

        private void BtnReportIssues_Click(object sender, EventArgs e)
        {
            ReportIssueForm reportForm = new ReportIssueForm();
            reportForm.FormClosed += (s, args) => this.Show();
            this.Hide();
            reportForm.Show();
        }

        private void BtnLocalEvents_Click(object sender, EventArgs e)
        {
            LocalEventsForm eventsForm = new LocalEventsForm();
            eventsForm.FormClosed += (s, args) => this.Show();
            this.Hide();
            eventsForm.Show();
        }

        private void BtnServiceStatus_Click(object sender, EventArgs e)
        {
            // ACTIVATED: Add navigation to your Service Request Status form here.
            // Example:
            // ServiceStatusForm statusForm = new ServiceStatusForm();
            // statusForm.FormClosed += (s, args) => this.Show();
            // this.Hide();
            // statusForm.Show();

            MessageBox.Show(
                "Service Request Status tracking is now active.",
                "Service Request Status",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }
    }
}