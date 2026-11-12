using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MuncipalityServiceRequestApp
{
    public class LocalEventsForm : Form
    {
        private const string AllCategories = "All Categories";
        private const string AllLocations = "All Locations";
        private const string AllDates = "All Dates";

        private TextBox txtSearch;
        private ComboBox cboCategory;
        private ComboBox cboLocation;
        private ComboBox cboDate;
        private Button btnSearch;
        private Button btnClearFilters;
        private Label lblUrgent;
        private Label lblResultsHeader;
        private DataGridView dgvRecommended;
        private DataGridView dgvEvents;
        private Label lblRecentSearches;
        private Label lblRecentlyViewed;
        private readonly ToolTip toolTip = new ToolTip();

        // Position i in this list matches item i+1 in cboDate (item 0 is "All Dates").
        private readonly List<DateTime> dateChoices = new List<DateTime>();

        public LocalEventsForm()
        {
            AppTheme.InitForm(this, "Local Events and Announcements", 1000, 780, 760, 600);
            BuildUi();
            LoadFilterOptions();
            RunSearchAndRefresh(null, null, null, null);
            this.AcceptButton = btnSearch; // Enter key runs the search
        }

        private void BuildUi()
        {
            TableLayoutPanel root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 9,
                Padding = new Padding(AppTheme.SpaceLg, AppTheme.SpaceSm, AppTheme.SpaceLg, AppTheme.SpaceSm)
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));          // 0 filters
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));          // 1 urgent banner
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));          // 2 recommended heading
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 32F));      // 3 recommended grid
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));          // 4 events heading
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 68F));      // 5 events grid
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));          // 6 recent searches
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));          // 7 recently viewed
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));          // 8 back button

            // --- Search / filter row (wraps onto a second line on narrow windows) ---
            FlowLayoutPanel filters = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true, Margin = new Padding(0, 0, 0, 6) };
            txtSearch = new TextBox { Width = 170, Margin = new Padding(0, 3, 12, 3), MaxLength = 60 };
            cboCategory = NewFilterCombo(150);
            cboLocation = NewFilterCombo(150);
            cboDate = NewFilterCombo(130);
            btnSearch = new Button { Text = "Search", Width = 90, Height = 28, Margin = new Padding(0, 2, 8, 2) };
            AppTheme.StylePrimaryButton(btnSearch);
            btnSearch.Click += BtnSearch_Click;
            btnClearFilters = new Button { Text = "Clear", Width = 80, Height = 28, Margin = new Padding(0, 2, 0, 2) };
            AppTheme.StyleSecondaryButton(btnClearFilters);
            btnClearFilters.Click += BtnClearFilters_Click;
            filters.Controls.AddRange(new Control[]
            {
                Caption("Search:"), txtSearch, Caption("Category:"), cboCategory,
                Caption("Location:"), cboLocation, Caption("Date:"), cboDate, btnSearch, btnClearFilters
            });
            root.Controls.Add(filters, 0, 0);

            // --- Urgent announcements banner (fed by the priority heap) ---
            Panel urgentPanel = new Panel { Dock = DockStyle.Fill, Height = 44, BackColor = Color.FromArgb(255, 243, 205), Margin = new Padding(0, 0, 0, 8) };
            lblUrgent = new Label
            {
                Dock = DockStyle.Fill,
                AutoEllipsis = true,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 10, 0),
                Font = AppTheme.FontLabel,
                ForeColor = Color.FromArgb(120, 80, 0)
            };
            urgentPanel.Controls.Add(lblUrgent);
            root.Controls.Add(urgentPanel, 0, 1);

            // --- Recommended for You ---
            root.Controls.Add(Heading("Recommended for You"), 0, 2);
            dgvRecommended = NewGrid();
            dgvRecommended.Columns.Add(AppTheme.GridColumn("Title", "Title", 28));
            dgvRecommended.Columns.Add(AppTheme.GridColumn("Date", "Date", 11, "dd MMM yyyy"));
            dgvRecommended.Columns.Add(AppTheme.GridColumn("Location", "Location", 14));
            dgvRecommended.Columns.Add(AppTheme.GridColumn("Category", "Category", 16));
            dgvRecommended.Columns.Add(AppTheme.GridColumn("Why", "Why recommended", 31));
            root.Controls.Add(dgvRecommended, 0, 3);

            // --- All events ---
            lblResultsHeader = Heading("All Events & Announcements");
            root.Controls.Add(lblResultsHeader, 0, 4);
            dgvEvents = NewGrid();
            dgvEvents.Columns.Add(AppTheme.GridColumn("Title", "Title", 22));
            dgvEvents.Columns.Add(AppTheme.GridColumn("Date", "Date", 10, "dd MMM yyyy", true));
            dgvEvents.Columns.Add(AppTheme.GridColumn("Location", "Location", 12));
            dgvEvents.Columns.Add(AppTheme.GridColumn("Category", "Category", 14));
            dgvEvents.Columns.Add(AppTheme.GridColumn("Priority", "Priority", 8));
            dgvEvents.Columns.Add(AppTheme.GridColumn("Description", "Description", 34));
            root.Controls.Add(dgvEvents, 0, 5);

            // --- Footer ---
            lblRecentSearches = new Label { AutoSize = true, ForeColor = Color.DimGray, Font = AppTheme.FontCaptionItalic, Margin = new Padding(0, 6, 0, 0) };
            lblRecentlyViewed = new Label { AutoSize = true, ForeColor = Color.DimGray, Font = AppTheme.FontCaptionItalic, Margin = new Padding(0, 2, 0, 6) };
            root.Controls.Add(lblRecentSearches, 0, 6);
            root.Controls.Add(lblRecentlyViewed, 0, 7);

            Button btnBack = new Button { Text = "Back to Main Menu", Width = 200, Height = 36, Anchor = AnchorStyles.Left };
            AppTheme.StyleSecondaryButton(btnBack);
            btnBack.Click += (s, e) => this.Close();
            root.Controls.Add(btnBack, 0, 8);

            // Fill first, header last so the header docks to the very top.
            this.Controls.Add(root);
            this.Controls.Add(AppTheme.CreateHeaderPanel("CITY OF TSHWANE", "Local Events and Announcements"));
        }

        private static Label Caption(string text)
        {
            return new Label { Text = text, AutoSize = true, Margin = new Padding(0, 7, 4, 0) };
        }

        private static Label Heading(string text)
        {
            return new Label { Text = text, AutoSize = true, UseMnemonic = false, Font = AppTheme.FontSubheading, ForeColor = AppTheme.Black, Margin = new Padding(0, 4, 0, 4) };
        }

        private static ComboBox NewFilterCombo(int width)
        {
            return new ComboBox { Width = width, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(0, 3, 12, 3) };
        }

        private DataGridView NewGrid()
        {
            DataGridView grid = new DataGridView { Dock = DockStyle.Fill };
            AppTheme.StyleGrid(grid);
            grid.CellClick += DgvGrid_CellClick;
            return grid;
        }

        private void LoadFilterOptions()
        {
            // HashSets hold the unique categories / locations / dates, so no value is listed twice.
            cboCategory.Items.Clear();
            cboCategory.Items.Add(AllCategories);
            foreach (string category in EventRepository.Categories.OrderBy(c => c)) cboCategory.Items.Add(category);
            cboCategory.SelectedIndex = 0;

            cboLocation.Items.Clear();
            cboLocation.Items.Add(AllLocations);
            foreach (string location in EventRepository.Locations.OrderBy(l => l)) cboLocation.Items.Add(location);
            cboLocation.SelectedIndex = 0;

            cboDate.Items.Clear();
            cboDate.Items.Add(AllDates);
            dateChoices.Clear();
            foreach (DateTime date in EventRepository.EventDates.OrderBy(d => d))
            {
                dateChoices.Add(date);
                cboDate.Items.Add(date.ToString("ddd dd MMM"));
            }
            cboDate.SelectedIndex = 0;
        }

        // ---------- search ----------

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            string keyword = string.IsNullOrWhiteSpace(txtSearch.Text) ? null : txtSearch.Text.Trim();
            string category = cboCategory.SelectedIndex > 0 ? cboCategory.SelectedItem.ToString() : null;
            string location = cboLocation.SelectedIndex > 0 ? cboLocation.SelectedItem.ToString() : null;
            DateTime? date = cboDate.SelectedIndex > 0 ? (DateTime?)dateChoices[cboDate.SelectedIndex - 1] : null;

            // Empty searches and exact repeats are ignored by the service, so they cannot skew recommendations.
            SearchHistoryService.RecordSearch(new SearchQuery { Keyword = keyword, Category = category, Location = location });
            RunSearchAndRefresh(keyword, category, location, date);
        }

        private void BtnClearFilters_Click(object sender, EventArgs e)
        {
            txtSearch.Clear();
            cboCategory.SelectedIndex = 0;
            cboLocation.SelectedIndex = 0;
            cboDate.SelectedIndex = 0;
            RunSearchAndRefresh(null, null, null, null);
        }

        private void RunSearchAndRefresh(string keyword, string category, string location, DateTime? date)
        {
            List<MunicipalEvent> results = FilterEvents(keyword, category, location, date);
            PopulateEvents(results);
            lblResultsHeader.Text = "All Events & Announcements (" + results.Count + " of " + EventRepository.Events.Count + ")";

            PopulateRecommendations(RecommendationEngine.GetRecommendations());
            RefreshUrgentBanner();
            RefreshFooter();
        }

        private static List<MunicipalEvent> FilterEvents(string keyword, string category, string location, DateTime? date)
        {
            IEnumerable<MunicipalEvent> source = EventRepository.Events;

            if (!string.IsNullOrEmpty(category))
            {
                List<MunicipalEvent> inCategory;
                source = EventRepository.ByCategory.TryGetValue(category, out inCategory) ? inCategory : new List<MunicipalEvent>();
            }
            if (!string.IsNullOrEmpty(location))
            {
                source = source.Where(ev => string.Equals(ev.Location, location, StringComparison.OrdinalIgnoreCase));
            }
            if (date.HasValue)
            {
                source = source.Where(ev => ev.Date.Date == date.Value.Date);
            }
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                source = source.Where(ev =>
                    EventRepository.ContainsIgnoreCase(ev.Title, keyword) ||
                    EventRepository.ContainsIgnoreCase(ev.Description, keyword) ||
                    EventRepository.ContainsIgnoreCase(ev.Location, keyword) ||
                    EventRepository.ContainsIgnoreCase(ev.Category, keyword));
            }

            return InDateOrder(source);
        }

        /// <summary>Walks the SortedDictionary (already ordered by date) so no re-sorting is needed.</summary>
        private static List<MunicipalEvent> InDateOrder(IEnumerable<MunicipalEvent> candidates)
        {
            HashSet<int> candidateIds = new HashSet<int>(candidates.Select(ev => ev.Id));
            List<MunicipalEvent> ordered = new List<MunicipalEvent>();

            foreach (KeyValuePair<DateTime, List<MunicipalEvent>> day in EventRepository.ByDate)
            {
                foreach (MunicipalEvent ev in day.Value.OrderBy(e => e.Priority))
                {
                    if (candidateIds.Contains(ev.Id)) ordered.Add(ev);
                }
            }
            return ordered;
        }

        // ---------- grids ----------

        private void PopulateEvents(List<MunicipalEvent> items)
        {
            dgvEvents.Rows.Clear();
            foreach (MunicipalEvent ev in items)
            {
                int row = dgvEvents.Rows.Add(ev.Title, ev.Date, ev.Location, ev.Category, ev.Priority.ToString(), ev.Description);
                dgvEvents.Rows[row].Tag = ev.Id;
                dgvEvents.Rows[row].Cells["Priority"].Style.ForeColor = AppTheme.PriorityColor(ev.Priority);
                dgvEvents.Rows[row].Cells["Priority"].Style.Font = AppTheme.FontLabel;
                dgvEvents.Rows[row].Cells["Description"].ToolTipText = ev.Description;
            }

            if (items.Count == 0)
            {
                dgvEvents.Rows.Add("No matching events found. Try clearing a filter.", "", "", "", "", "");
            }
        }

        private void PopulateRecommendations(List<Recommendation> items)
        {
            dgvRecommended.Rows.Clear();
            foreach (Recommendation r in items)
            {
                int row = dgvRecommended.Rows.Add(r.Event.Title, r.Event.Date, r.Event.Location, r.Event.Category, r.Reason);
                dgvRecommended.Rows[row].Tag = r.Event.Id;
            }

            if (items.Count == 0)
            {
                dgvRecommended.Rows.Add("You have viewed every event. Check back later for new announcements.", "", "", "", "");
            }
        }

        private void DgvGrid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView grid = sender as DataGridView;
            if (grid == null || e.RowIndex < 0 || e.RowIndex >= grid.Rows.Count) return;

            if (grid.Rows[e.RowIndex].Tag is int)
            {
                SearchHistoryService.RecordView((int)grid.Rows[e.RowIndex].Tag);
                RefreshFooter();
            }
        }

        // ---------- banner + footer ----------

        private void RefreshUrgentBanner()
        {
            List<MunicipalEvent> urgent = EventRepository.GetByPriorityOrder()
                .Where(ev => (ev.Priority == EventPriority.Urgent || ev.Priority == EventPriority.High) && ev.Date.Date >= DateTime.Today)
                .Take(3)
                .ToList();

            lblUrgent.Text = urgent.Count == 0
                ? "No urgent announcements at this time."
                : "\u26A0 URGENT:  " + string.Join("   |   ", urgent.Select(ev => ev.Title + " (" + ev.Date.ToString("dd MMM") + ")"));
            toolTip.SetToolTip(lblUrgent, lblUrgent.Text);
        }

        private void RefreshFooter()
        {
            List<SearchQuery> recent = SearchHistoryService.GetRecentSearches(3);
            lblRecentSearches.Text = recent.Count == 0
                ? "Recent searches: none yet."
                : "Recent searches (newest first): " + string.Join("  \u2022  ", recent.Select(q => q.ToString()));

            List<string> titles = SearchHistoryService.GetRecentlyViewedNewestFirst()
                .Select(EventRepository.FindById)
                .Where(ev => ev != null)
                .Select(ev => ev.Title)
                .ToList();
            lblRecentlyViewed.Text = titles.Count == 0
                ? "Recently viewed: none yet. Click an event to view it."
                : "Recently viewed (newest first): " + string.Join(", ", titles);
        }
    }
}