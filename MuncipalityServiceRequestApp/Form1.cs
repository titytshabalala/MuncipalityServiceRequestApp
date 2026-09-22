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
            this.Text = "Municipal Services Application - Main Menu";
            this.Width = 480;
            this.Height = 420;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = AppTheme.LightBackground;

            Panel header = AppTheme.CreateHeaderPanel("CITY OF TSHWANE", "Municipal Services Application");

            Button btnReportIssues = new Button
            {
                Text = "Report Issues",
                Name = "btnReportIssues",
                Width = 260,
                Height = 45,
                Top = AppTheme.HeaderHeight + 30,
                Left = 110
            };
            AppTheme.StylePrimaryButton(btnReportIssues);
            btnReportIssues.Click += BtnReportIssues_Click;

            Button btnLocalEvents = new Button
            {
                Text = "Local Events/Announcements",
                Name = "btnLocalEvents",
                Width = 260,
                Height = 45,
                Top = AppTheme.HeaderHeight + 90,
                Left = 110,
                Enabled = true
            };
            AppTheme.StylePrimaryButton(btnLocalEvents);
            btnLocalEvents.Click += BtnLocalEvents_Click;

            Button btnServiceStatus = new Button
            {
                Text = "Service Request Status",
                Name = "btnServiceStatus",
                Width = 260,
                Height = 45,
                Top = AppTheme.HeaderHeight + 150,
                Left = 110,
                Enabled = false // Part 3 — not yet built
            };
            AppTheme.StyleDisabledButton(btnServiceStatus);

            Label footerLabel = new Label
            {
                Text = "Service Request Status will be enabled in a future release.",
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Bottom,
                Height = 30,
                ForeColor = Color.Gray
            };

            this.Controls.Add(btnReportIssues);
            this.Controls.Add(btnLocalEvents);
            this.Controls.Add(btnServiceStatus);
            this.Controls.Add(footerLabel);
            this.Controls.Add(header);
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
    }
}