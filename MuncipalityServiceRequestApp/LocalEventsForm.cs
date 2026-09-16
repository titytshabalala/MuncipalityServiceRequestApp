using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MuncipalityServiceRequestApp
{
    public class LocalEventsForm : Form
    {
        private TextBox txtSearch = null!;
        private ComboBox cboCategory = null!;
        private ComboBox cboLocation = null!;
        private Button btnSearch = null!;
        private Button btnClearFilters = null!;
        private Panel pnlUrgent = null!;
        private Label lblUrgent = null!;
        private DataGridView dgvRecommended = null!;
        private DataGridView dgvEvents = null!;
        private Label lblRecentlyViewed = null!;
        private Button btnBack = null!;

        public LocalEventsForm()
        {
            this.Text = "Local Events and Announcements";
            this.Width = 900;
            this.Height = 760;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(780, 640);
            this.BackColor = AppTheme.LightBackground;

            SetupUi();
            SetupGridColumns(dgvEvents);
            SetupGridColumns(dgvRecommended);
            LoadFilterOptions();
            RunSearchAndRefresh(null, null, null); // initial load — no search recorded
        }

        private void SetupUi()
        {
            Panel header = AppTheme.CreateHeaderPanel("CITY OF TSHWANE", "Local Events and Announcements");
            int y = AppTheme.HeaderHeight + 15;

            // --- Search / filter row ---
            Label lblSearch = new Label { Text = "Search:", Left = 20, Top = y + 4, Width = 55 };
            txtSearch = new TextBox { Left = 78, Top = y, Width = 160 };

            Label lblCategory = new Label { Text = "Category:", Left = 250, Top = y + 4, Width = 62 };
            cboCategory = new ComboBox { Left = 315, Top = y, Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };

            Label lblLocation = new Label { Text = "Location:", Left = 475, Top = y + 4, Width = 60 };
            cboLocation = new ComboBox { Left = 538, Top = y, Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };

            btnSearch = new Button { Text = "Search", Left = 698, Top = y - 1, Width = 75, Height = 25, Anchor = AnchorStyles.Top | AnchorStyles.Right };
            AppTheme.StylePrimaryButton(btnSearch);
            btnSearch.Click += BtnSearch_Click;

            btnClearFilters = new Button { Text = "Clear", Left = 778, Top = y - 1, Width = 75, Height = 25, Anchor = AnchorStyles.Top | AnchorStyles.Right };
            AppTheme.StyleSecondaryButton(btnClearFilters);
            btnClearFilters.Click += BtnClearFilters_Click;

            y += 40;

            // --- Urgent announcements banner ---
            pnlUrgent = new Panel
            {
                Left = 20,
                Top = y,
                Width = this.ClientSize.Width - 40,
                Height = 44,
                BackColor = Color.FromArgb(255, 243, 205),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            lblUrgent = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 10, 0),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(120, 80, 0)
            };
            pnlUrgent.Controls.Add(lblUrgent);
            y += 44 + 12;

            // --- Recommended for You ---
            Label lblRecommendedHeader = new Label
            {
                Text = "Recommended for You",
                Left = 20,
                Top = y,
                Width = 300,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = AppTheme.Black
            };
            y += 22;

            dgvRecommended = new DataGridView
            {
                Left = 20,
                Top = y,
                Width = this.ClientSize.Width - 40,
                Height = 110,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White
            };
            dgvRecommended.CellClick += DgvGrid_CellClick;
            y += 110 + 15;

            // --- All events ---
            Label lblResultsHeader = new Label
            {
                Text = "All Events & Announcements",
                Left = 20,
                Top = y,
                Width = 300,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = AppTheme.Black
            };
            y += 22;

            int gridHeight = this.ClientSize.Height - y - 100;
            dgvEvents = new DataGridView
            {
                Left = 20,
                Top = y,
                Width = this.ClientSize.Width - 40,
                Height = Math.Max(gridHeight, 120),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White
            };
            dgvEvents.CellClick += DgvGrid_CellClick;

            // --- Footer ---
            lblRecentlyViewed = new Label
            {
                Left = 20,
                Width = this.ClientSize.Width - 40,
                Height = 20,
                Top = this.ClientSize.Height - 70,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                ForeColor = Color.DimGray,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Italic)
            };

            btnBack = new Button
            {
                Text = "Back to Main Menu",
                Left = 20,
                Top = this.ClientSize.Height - 50,
                Width = 200,
                Height = 34,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            AppTheme.StyleSecondaryButton(btnBack);
            btnBack.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[]
            {
                lblSearch, txtSearch, lblCategory, cboCategory, lblLocation, cboLocation,
                btnSearch, btnClearFilters, pnlUrgent,
                lblRecommendedHeader, dgvRecommended,
                lblResultsHeader, dgvEvents,
                lblRecentlyViewed, btnBack
            });
            this.Controls.Add(header);
        }

        private static void SetupGridColumns(DataGridView grid)
        {
            grid.Columns.Clear();
            grid.Columns.Add("Title", "Title");
            grid.Columns.Add("Date", "Date");
            grid.Columns.Add("Location", "Location");
            grid.Columns.Add("Category", "Category");
            grid.Columns.Add("Priority", "Priority");
            grid.Columns.Add("Description", "Description");
        }

        private void LoadFilterOptions()
        {
            cboCategory.Items.Clear();
            cboCategory.Items.Add("All Categories");
            foreach (string category in EventRepository.Categories.OrderBy(c => c))
            {
                cboCategory.Items.Add(category);
            }
            cboCategory.SelectedIndex = 0;

            cboLocation.Items.Clear();
            cboLocation.Items.Add("All Locations");
            foreach (string location in EventRepository.Locations.OrderBy(l => l))
            {
                cboLocation.Items.Add(location);
            }
            cboLocation.SelectedIndex = 0;
        }

        private void BtnSearch_Click(object? sender, EventArgs e)
        {
            string? keyword = string.IsNullOrWhiteSpace(txtSearch.Text) ? null : txtSearch.Text.Trim();
            string? category = cboCategory.SelectedIndex > 0 ? cboCategory.SelectedItem?.ToString() : null;
            string? location = cboLocation.SelectedIndex > 0 ? cboLocation.SelectedItem?.ToString() : null;

            SearchHistoryService.RecordSearch(new SearchQuery { Keyword = keyword, Category = category, Location = location });
            RunSearchAndRefresh(keyword, category, location);
        }

        private void BtnClearFilters_Click(object? sender, EventArgs e)
        {
            txtSearch.Clear();
            cboCategory.SelectedIndex = 0;
            cboLocation.SelectedIndex = 0;
            RunSearchAndRefresh(null, null, null);
        }

        private void RunSearchAndRefresh(string? keyword, string? category, string? location)
        {
            List<MunicipalEvent> results = FilterEvents(keyword, category, location);
            PopulateGrid(dgvEvents, results);

            List<MunicipalEvent> recommended = RecommendationEngine.GetRecommendations();
            PopulateGrid(dgvRecommended, recommended);

            RefreshUrgentBanner();
            RefreshRecentlyViewedLabel();
        }

        // Uses the category Dictionary for an O(1) starting point when a category
        // filter is active, then walks the date-sorted dictionary so results come
        // back in chronological order without re-sorting.
        private List<MunicipalEvent> FilterEvents(string? keyword, string? category, string? location)
        {
            IEnumerable<MunicipalEvent> source = (!string.IsNullOrEmpty(category)
                && EventRepository.ByCategory.TryGetValue(category, out List<MunicipalEvent>? categoryMatches))
                ? categoryMatches
                : EventRepository.Events;

            if (!string.IsNullOrEmpty(location))
            {
                source = source.Where(ev => string.Equals(ev.Location, location, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                source = source.Where(ev =>
                    ev.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    ev.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase));
            }

            return GetEventsInDateOrder(source);
        }

        private static List<MunicipalEvent> GetEventsInDateOrder(IEnumerable<MunicipalEvent> candidates)
        {
            HashSet<int> candidateIds = new HashSet<int>(candidates.Select(ev => ev.Id));
            List<MunicipalEvent> ordered = new List<MunicipalEvent>();

            foreach (KeyValuePair<DateTime, List<MunicipalEvent>> dayGroup in EventRepository.ByDate)
            {
                foreach (MunicipalEvent ev in dayGroup.Value)
                {
                    if (candidateIds.Contains(ev.Id))
                    {
                        ordered.Add(ev);
                    }
                }
            }
            return ordered;
        }

        private static void PopulateGrid(DataGridView grid, List<MunicipalEvent> items)
        {
            grid.Rows.Clear();
            foreach (MunicipalEvent ev in items)
            {
                int rowIndex = grid.Rows.Add(ev.Title, ev.Date.ToString("dd MMM yyyy"), ev.Location, ev.Category, ev.Priority.ToString(), ev.Description);
                grid.Rows[rowIndex].Tag = ev.Id;
            }

            if (grid.Rows.Count == 0)
            {
                grid.Rows.Add("No matching events found.", "", "", "", "", "");
            }
        }

        private void DgvGrid_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || sender is not DataGridView grid) return;
            if (grid.Rows[e.RowIndex].Tag is int eventId)
            {
                SearchHistoryService.RecordView(eventId);
                RefreshRecentlyViewedLabel();
            }
        }

        private void RefreshUrgentBanner()
        {
            List<MunicipalEvent> urgent = EventRepository.GetByPriorityOrder()
                .Where(ev => ev.Priority == EventPriority.Urgent || ev.Priority == EventPriority.High)
                .Take(3)
                .ToList();

            lblUrgent.Text = urgent.Count == 0
                ? "No urgent announcements at this time."
                : "\u26A0 " + string.Join("   |   ", urgent.Select(ev => $"{ev.Title} ({ev.Date:dd MMM})"));
        }

        private void RefreshRecentlyViewedLabel()
        {
            if (SearchHistoryService.RecentlyViewed.Count == 0)
            {
                lblRecentlyViewed.Text = "Recently viewed: none yet.";
                return;
            }

            List<string> titles = SearchHistoryService.RecentlyViewed
                .Select(id => EventRepository.Events.FirstOrDefault(ev => ev.Id == id)?.Title)
                .Where(title => title != null)
                .Select(title => title!)
                .ToList();

            lblRecentlyViewed.Text = "Recently viewed: " + string.Join(", ", titles);
        }
    }
}