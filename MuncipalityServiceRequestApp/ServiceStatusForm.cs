using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MuncipalityServiceRequestApp
{
    /// <summary>
    /// Service Request Status. Three tabs, each backed by a different data structure:
    ///   Track Requests - AVL tree (lookup by reference), binary heap (priority order), List (newest first)
    ///   Service Zones  - weighted graph: BFS/DFS traversal and minimum spanning tree (Kruskal / Prim)
    ///   Request Index  - a live picture of the AVL tree, so its balancing can be seen
    /// </summary>
    public class ServiceStatusForm : Form
    {
        private const string AllStatuses = "All statuses";
        private const string SortPriority = "Most urgent first (priority heap)";
        private const string SortReference = "Reference number (AVL tree)";
        private const string SortNewest = "Newest first (list)";

        private readonly ServiceZoneGraph network = ServiceZoneGraph.Default;
        private bool refreshing; // true while grids are being rebuilt, so selection events are ignored

        // Track Requests tab
        private TextBox txtReference;
        private Button btnFind;
        private ComboBox cboStatusFilter;
        private ComboBox cboSort;
        private Label lblMessage;
        private DataGridView grid;
        private TextBox txtDetails;
        private TextBox txtNote;
        private Button btnAdvance;

        // Service Zones tab
        private ComboBox cboZone;
        private DataGridView gridZones;
        private Label lblUnmapped;
        private TextBox txtNetwork;

        // Request Index tab
        private TextBox txtIndex;

        public ServiceStatusForm()
        {
            AppTheme.InitForm(this, "Service Request Status", 1000, 740, 780, 580);
            BuildUi();
            cboStatusFilter.SelectedIndex = 0;
            cboSort.SelectedIndex = 0;
            RefreshGrid(null);
            RefreshZoneTable();
            RefreshIndexText();
        }

        // =====================================================================
        // UI construction
        // =====================================================================

        private void BuildUi()
        {
            TabControl tabs = new TabControl { Dock = DockStyle.Fill, Font = AppTheme.FontBody };
            tabs.TabPages.Add(BuildTrackPage());
            tabs.TabPages.Add(BuildZonesPage());
            tabs.TabPages.Add(BuildIndexPage());
            tabs.SelectedIndexChanged += (s, e) => { RefreshZoneTable(); RefreshIndexText(); };

            Panel footer = new Panel { Dock = DockStyle.Bottom, Height = 56, Padding = new Padding(AppTheme.SpaceMd, 10, AppTheme.SpaceMd, 10) };
            Button btnBack = new Button { Text = "Back to Main Menu", Dock = DockStyle.Left, Width = 200 };
            AppTheme.StyleSecondaryButton(btnBack);
            btnBack.Click += (s, e) => this.Close();
            footer.Controls.Add(btnBack);

            // Fill first, then the bottom bar, header last so it docks to the very top.
            this.Controls.Add(tabs);
            this.Controls.Add(footer);
            this.Controls.Add(AppTheme.CreateHeaderPanel("CITY OF TSHWANE", "Service Request Status"));
        }

        private static Label Caption(string text)
        {
            return new Label { Text = text, AutoSize = true, Margin = new Padding(0, 7, 4, 0) };
        }

        private TabPage BuildTrackPage()
        {
            TabPage page = new TabPage("Track Requests") { BackColor = AppTheme.LightBackground };
            TableLayoutPanel layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, Padding = new Padding(AppTheme.SpaceMd) };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

            // --- toolbar ---
            FlowLayoutPanel toolbar = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true };
            txtReference = new TextBox { Width = 120, MaxLength = 15, Margin = new Padding(0, 3, 8, 3) };
            btnFind = new Button { Text = "Find", Width = 80, Height = 28, Margin = new Padding(0, 2, 16, 2) };
            AppTheme.StylePrimaryButton(btnFind);
            btnFind.Click += BtnFind_Click;
            this.AcceptButton = btnFind; // Enter key runs the lookup...
            txtReference.Enter += (s, e) => this.AcceptButton = btnFind;

            cboStatusFilter = new ComboBox { Width = 130, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(0, 3, 12, 3) };
            cboStatusFilter.Items.Add(AllStatuses);
            foreach (IssueStatus status in Enum.GetValues(typeof(IssueStatus))) cboStatusFilter.Items.Add(Issue.StatusText(status));
            cboStatusFilter.SelectedIndexChanged += (s, e) => RefreshGrid(null);

            cboSort = new ComboBox { Width = 230, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(0, 3, 12, 3) };
            cboSort.Items.AddRange(new object[] { SortPriority, SortReference, SortNewest });
            cboSort.SelectedIndexChanged += (s, e) => RefreshGrid(null);

            Button btnReset = new Button { Text = "Reset", Width = 80, Height = 28, Margin = new Padding(0, 2, 0, 2) };
            AppTheme.StyleSecondaryButton(btnReset);
            btnReset.Click += (s, e) =>
            {
                txtReference.Clear();
                cboStatusFilter.SelectedIndex = 0;
                cboSort.SelectedIndex = 0;
                ShowMessage(string.Empty, false);
            };

            toolbar.Controls.AddRange(new Control[]
            {
                Caption("Reference #:"), txtReference, btnFind, Caption("Status:"), cboStatusFilter,
                Caption("Sort by:"), cboSort, btnReset
            });
            layout.Controls.Add(toolbar, 0, 0);

            // --- feedback line ---
            lblMessage = new Label { AutoSize = true, Margin = new Padding(0, 2, 0, 6), Font = AppTheme.FontCaptionItalic };
            layout.Controls.Add(lblMessage, 0, 1);

            // --- requests grid ---
            grid = new DataGridView { Dock = DockStyle.Fill };
            AppTheme.StyleGrid(grid);
            grid.Columns.Add(AppTheme.GridColumn("Ref", "Reference", 12));
            grid.Columns.Add(AppTheme.GridColumn("Priority", "Priority", 10));
            grid.Columns.Add(AppTheme.GridColumn("Status", "Status", 12));
            grid.Columns.Add(AppTheme.GridColumn("Category", "Category", 18));
            grid.Columns.Add(AppTheme.GridColumn("Zone", "Service zone", 18));
            grid.Columns.Add(AppTheme.GridColumn("Reported", "Reported", 18, "dd MMM yyyy HH:mm"));
            grid.SelectionChanged += (s, e) => { if (!refreshing) ShowDetails(SelectedIssue()); };
            layout.Controls.Add(grid, 0, 2);

            // --- details + status update ---
            TableLayoutPanel lower = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = new Padding(0, 8, 0, 0) };
            lower.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65F));
            lower.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));

            txtDetails = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.White,
                Font = AppTheme.FontBody,
                Margin = new Padding(0, 0, 8, 0)
            };
            lower.Controls.Add(txtDetails, 0, 0);

            TableLayoutPanel update = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4 };
            update.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            update.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            update.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            update.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            update.Controls.Add(new Label { Text = "Update status", AutoSize = true, Font = AppTheme.FontSubheading, Margin = new Padding(0, 0, 0, 6) }, 0, 0);
            update.Controls.Add(new Label { Text = "Note for the history (optional):", AutoSize = true, Margin = new Padding(0, 0, 0, 3) }, 0, 1);
            txtNote = new TextBox { Dock = DockStyle.Top, MaxLength = 150, Margin = new Padding(0, 0, 0, 8) };
            txtNote.Enter += (s, e) => this.AcceptButton = null; // ...but only while typing a reference, not a note
            update.Controls.Add(txtNote, 0, 2);
            btnAdvance = new Button { Dock = DockStyle.Top, Height = 40, Text = "Select a request" };
            AppTheme.StyleDisabledButton(btnAdvance);
            btnAdvance.Click += BtnAdvance_Click;
            update.Controls.Add(btnAdvance, 0, 3);
            lower.Controls.Add(update, 1, 0);

            layout.Controls.Add(lower, 0, 3);
            page.Controls.Add(layout);
            return page;
        }

        private TabPage BuildZonesPage()
        {
            TabPage page = new TabPage("Service Zones") { BackColor = AppTheme.LightBackground };
            TableLayoutPanel layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5, Padding = new Padding(AppTheme.SpaceMd) };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

            FlowLayoutPanel toolbar = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true };
            cboZone = new ComboBox { Width = 150, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(0, 3, 12, 3) };
            foreach (string zone in network.Zones) cboZone.Items.Add(zone);
            cboZone.SelectedIndex = 0;
            cboZone.SelectedIndexChanged += (s, e) => RefreshZoneTable();

            toolbar.Controls.Add(Caption("Start zone:"));
            toolbar.Controls.Add(cboZone);
            toolbar.Controls.Add(ZoneButton("Visit zones (BFS)", 150, (s, e) => ShowTraversal(true)));
            toolbar.Controls.Add(ZoneButton("Visit zones (DFS)", 150, (s, e) => ShowTraversal(false)));
            toolbar.Controls.Add(ZoneButton("Depot routes (Kruskal MST)", 210, (s, e) => ShowMst(true)));
            toolbar.Controls.Add(ZoneButton("Depot routes (Prim MST)", 190, (s, e) => ShowMst(false)));
            layout.Controls.Add(toolbar, 0, 0);

            layout.Controls.Add(new Label
            {
                Text = "Open requests per zone, nearest zones to the start zone first. Distances are counted in road hops (breadth-first search).",
                AutoSize = true,
                ForeColor = Color.DimGray,
                Font = AppTheme.FontCaptionItalic,
                Margin = new Padding(0, 2, 0, 4)
            }, 0, 1);

            gridZones = new DataGridView { Dock = DockStyle.Fill };
            AppTheme.StyleGrid(gridZones);
            gridZones.Columns.Add(AppTheme.GridColumn("Zone", "Zone", 30));
            gridZones.Columns.Add(AppTheme.GridColumn("Hops", "Hops from start", 20));
            gridZones.Columns.Add(AppTheme.GridColumn("Open", "Open requests", 20));
            gridZones.Columns.Add(AppTheme.GridColumn("Top", "Highest priority", 30));
            layout.Controls.Add(gridZones, 0, 2);

            lblUnmapped = new Label { AutoSize = true, ForeColor = Color.DimGray, Margin = new Padding(0, 4, 0, 4) };
            layout.Controls.Add(lblUnmapped, 0, 3);

            txtNetwork = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.White,
                Font = new Font("Consolas", 9.5F),
                Text = "Choose a start zone, then pick a traversal or a minimum spanning tree to see the result here."
            };
            layout.Controls.Add(txtNetwork, 0, 4);

            page.Controls.Add(layout);
            return page;
        }

        private static Button ZoneButton(string text, int width, EventHandler onClick)
        {
            Button b = new Button { Text = text, Width = width, Height = 28, Margin = new Padding(0, 2, 8, 2) };
            AppTheme.StyleSecondaryButton(b);
            b.Font = AppTheme.FontLabel;
            b.Click += onClick;
            return b;
        }

        private TabPage BuildIndexPage()
        {
            TabPage page = new TabPage("Request Index (AVL)") { BackColor = AppTheme.LightBackground, Padding = new Padding(AppTheme.SpaceMd) };
            txtIndex = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Both,
                WordWrap = false,
                BackColor = Color.White,
                Font = new Font("Consolas", 10F)
            };
            page.Controls.Add(txtIndex);
            return page;
        }

        // =====================================================================
        // Track Requests behaviour
        // =====================================================================

        private Issue SelectedIssue()
        {
            if (grid.CurrentRow == null) return null;
            return grid.CurrentRow.Tag as Issue;
        }

        private void ShowMessage(string text, bool isError)
        {
            lblMessage.Text = text;
            lblMessage.ForeColor = isError ? AppTheme.ErrorRed : AppTheme.Green;
        }

        private void BtnFind_Click(object sender, EventArgs e)
        {
            string input = txtReference.Text;
            if (string.IsNullOrWhiteSpace(input))
            {
                ShowMessage("Enter a reference number such as SR-1001 (or just 1001).", true);
                txtReference.Focus();
                return;
            }

            int id;
            if (!IssueRepository.TryParseReference(input, out id))
            {
                ShowMessage("\"" + input.Trim() + "\" is not a valid reference. Use the format SR-1001 or 1001.", true);
                txtReference.SelectAll();
                return;
            }

            Issue issue;
            if (!IssueRepository.TryGetById(id, out issue))
            {
                ShowMessage("No service request found with reference SR-" + id + ". Check the number on your confirmation.", true);
                txtReference.SelectAll();
                return;
            }

            int comparisons = IssueRepository.Index.LastLookupComparisons;

            // Clear filters so the match is guaranteed to be visible in the grid.
            refreshing = true;
            cboStatusFilter.SelectedIndex = 0;
            refreshing = false;
            RefreshGrid(issue.Id);

            ShowMessage("Found " + issue.ReferenceNumber + " using the AVL index in " + comparisons + " comparison(s) " +
                        "(" + IssueRepository.Index.Count + " requests, tree height " + IssueRepository.Index.Height + ").", false);
        }

        private List<Issue> GetVisibleIssues()
        {
            Func<Issue, bool> filter = null;
            if (cboStatusFilter.SelectedIndex > 0)
            {
                IssueStatus wanted = (IssueStatus)(cboStatusFilter.SelectedIndex - 1);
                filter = i => i.Status == wanted;
            }
            Func<Issue, bool> keep = filter ?? (i => true);

            string sort = cboSort.SelectedItem as string;
            if (sort == SortReference) return IssueRepository.GetInReferenceOrder().Where(keep).ToList();
            if (sort == SortNewest) return IssueRepository.Issues.Where(keep).OrderByDescending(i => i.DateReported).ToList();
            return IssueRepository.GetByPriority(filter);
        }

        private void RefreshGrid(int? selectId)
        {
            if (refreshing) return;

            Issue previous = SelectedIssue();
            int keepId = selectId.HasValue ? selectId.Value : (previous != null ? previous.Id : -1);

            refreshing = true;
            try
            {
                List<Issue> visible = GetVisibleIssues();
                grid.Rows.Clear();

                int selectRow = -1;
                foreach (Issue issue in visible)
                {
                    int row = grid.Rows.Add(
                        issue.ReferenceNumber,
                        issue.Priority.ToString(),
                        Issue.StatusText(issue.Status),
                        issue.Category,
                        issue.Zone ?? "Not recognised",
                        issue.DateReported);
                    grid.Rows[row].Tag = issue;
                    grid.Rows[row].Cells["Priority"].Style.ForeColor = AppTheme.PriorityColor(issue.Priority);
                    grid.Rows[row].Cells["Priority"].Style.Font = AppTheme.FontLabel;
                    if (issue.Id == keepId) selectRow = row;
                }

                if (visible.Count == 0)
                {
                    grid.Rows.Add("No requests match this filter.", "", "", "", "", "");
                }
                else
                {
                    if (selectRow < 0) selectRow = 0;
                    grid.ClearSelection();
                    grid.Rows[selectRow].Selected = true;
                    grid.CurrentCell = grid.Rows[selectRow].Cells[0];
                }
            }
            finally
            {
                refreshing = false;
            }

            ShowDetails(SelectedIssue());
        }

        private void ShowDetails(Issue issue)
        {
            if (issue == null)
            {
                txtDetails.Text = "Select a request to see its details and history.";
                btnAdvance.Text = "Select a request";
                AppTheme.SetPrimaryEnabled(btnAdvance, false);
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(issue.ReferenceNumber + "  -  " + Issue.StatusText(issue.Status));
            sb.AppendLine("Priority:      " + issue.Priority);
            sb.AppendLine("Category:      " + issue.Category);
            sb.AppendLine("Location:      " + issue.Location);
            sb.AppendLine("Service zone:  " + (issue.Zone ?? "not recognised (outside the depot network)"));
            sb.AppendLine("Reported:      " + issue.DateReported.ToString("dd MMM yyyy HH:mm"));
            sb.AppendLine("Attachments:   " + (issue.AttachmentPaths.Count == 0
                ? "none"
                : issue.AttachmentPaths.Count + " file(s): " + string.Join(", ", issue.AttachmentPaths.Select(System.IO.Path.GetFileName))));
            sb.AppendLine();
            sb.AppendLine("Description:");
            sb.AppendLine(issue.Description);
            sb.AppendLine();
            sb.AppendLine("Status history:");
            foreach (StatusChange change in issue.History)
            {
                sb.AppendLine("  \u2022 " + change.When.ToString("dd MMM yyyy HH:mm") + "  " + Issue.StatusText(change.Status) + ": " + change.Note);
            }
            txtDetails.Text = sb.ToString();
            txtDetails.SelectionStart = 0;

            if (issue.Status == IssueStatus.Resolved)
            {
                btnAdvance.Text = "Resolved";
                AppTheme.SetPrimaryEnabled(btnAdvance, false);
            }
            else
            {
                btnAdvance.Text = "Mark as " + Issue.StatusText((IssueStatus)((int)issue.Status + 1));
                AppTheme.SetPrimaryEnabled(btnAdvance, true);
            }
        }

        private void BtnAdvance_Click(object sender, EventArgs e)
        {
            Issue issue = SelectedIssue();
            if (issue == null) return;

            IssueStatus next = (IssueStatus)((int)issue.Status + 1);
            if (issue.Status == IssueStatus.Resolved) return;

            DialogResult answer = MessageBox.Show(
                "Change " + issue.ReferenceNumber + " from " + Issue.StatusText(issue.Status) + " to " + Issue.StatusText(next) + "?\n\nThis cannot be undone.",
                "Confirm status change", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (answer != DialogResult.Yes) return;

            string error;
            if (!IssueRepository.TryAdvance(issue, txtNote.Text, out error))
            {
                ShowMessage(error, true);
                return;
            }

            AppDataStore.Save();

            txtNote.Clear();
            RefreshGrid(issue.Id);
            RefreshZoneTable();
            RefreshIndexText();
            ShowMessage(issue.ReferenceNumber + " is now " + Issue.StatusText(issue.Status) + ".", false);
        }

        // =====================================================================
        // Service Zones behaviour
        // =====================================================================

        private string StartZone()
        {
            return cboZone.SelectedItem as string;
        }

        private void RefreshZoneTable()
        {
            string start = StartZone();
            if (start == null || gridZones == null) return;

            Dictionary<string, int> hops = network.HopDistances(start);
            gridZones.Rows.Clear();

            foreach (string zone in network.Zones
                .OrderBy(z => hops.ContainsKey(z) ? hops[z] : int.MaxValue)
                .ThenBy(z => z))
            {
                List<Issue> open = IssueRepository.Issues
                    .Where(i => i.Status != IssueStatus.Resolved && string.Equals(i.Zone, zone, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                string hopText = !hops.ContainsKey(zone) ? "unreachable" : (hops[zone] == 0 ? "start" : hops[zone].ToString());
                string top = open.Count == 0 ? "-" : ((EventPriority)open.Min(i => (int)i.Priority)).ToString();

                int row = gridZones.Rows.Add(zone, hopText, open.Count, top);
                if (open.Count > 0)
                {
                    gridZones.Rows[row].Cells["Open"].Style.Font = AppTheme.FontLabel;
                    gridZones.Rows[row].Cells["Top"].Style.ForeColor = AppTheme.PriorityColor((EventPriority)open.Min(i => (int)i.Priority));
                    gridZones.Rows[row].Cells["Top"].Style.Font = AppTheme.FontLabel;
                }
            }

            int unmapped = IssueRepository.Issues.Count(i => i.Status != IssueStatus.Resolved && i.Zone == null);
            lblUnmapped.Text = unmapped == 0
                ? "Every open request is inside the depot network."
                : unmapped + " open request(s) are not in a recognised zone, so they are not shown above.";
        }

        private void ShowTraversal(bool breadthFirst)
        {
            string start = StartZone();
            if (start == null) return;

            List<string> order = breadthFirst ? network.BreadthFirst(start) : network.DepthFirst(start);
            StringBuilder sb = new StringBuilder();
            if (breadthFirst)
            {
                sb.AppendLine("Breadth-first search from " + start + " (queue): nearest zones first.");
                sb.AppendLine("Useful for: which crews are closest, and how many road hops away is each zone.");
            }
            else
            {
                sb.AppendLine("Depth-first search from " + start + " (stack): follow each road as far as possible before backtracking.");
                sb.AppendLine("Useful for: planning one long patrol route that covers a whole corridor.");
            }
            sb.AppendLine();
            sb.AppendLine(string.Join("  ->  ", order));
            sb.AppendLine();
            sb.AppendLine(order.Count + " of " + network.ZoneCount + " zones reached.");
            txtNetwork.Text = sb.ToString();
        }

        private void ShowMst(bool kruskal)
        {
            string start = StartZone();
            if (start == null) return;

            List<ZoneRoad> tree = kruskal ? network.KruskalMst() : network.PrimMst(start);
            List<ZoneRoad> other = kruskal ? network.PrimMst(start) : network.KruskalMst();
            double total = ServiceZoneGraph.TotalKm(tree);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine((kruskal ? "Kruskal" : "Prim (from " + start + ")") + " minimum spanning tree: the shortest set of roads that links every depot zone.");
            sb.AppendLine(kruskal
                ? "Roads are taken shortest-first; a road is skipped if it would close a loop (checked with union-find)."
                : "One tree grows outward; the cheapest road reaching a new zone is always added next (candidates kept in a binary heap).");
            sb.AppendLine();
            foreach (ZoneRoad road in tree) sb.AppendLine("  " + road);
            sb.AppendLine();
            sb.AppendLine("Roads used: " + tree.Count + " of " + network.Roads.Count + "     Total: " + total.ToString("0.#") + " km");
            sb.AppendLine("Cross-check with " + (kruskal ? "Prim" : "Kruskal") + ": " + ServiceZoneGraph.TotalKm(other).ToString("0.#") + " km" +
                          (Math.Abs(ServiceZoneGraph.TotalKm(other) - total) < 1e-9 ? "  (same total, as expected)" : "  (differs because the network is not fully connected)"));
            sb.AppendLine("Distances are approximate road distances used for illustration.");
            txtNetwork.Text = sb.ToString();
        }

        // =====================================================================
        // Request Index behaviour
        // =====================================================================

        private void RefreshIndexText()
        {
            if (txtIndex == null) return;

            AvlTree<int, Issue> tree = IssueRepository.Index;
            List<Issue> byPriority = IssueRepository.GetByPriority(i => i.Status != IssueStatus.Resolved);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("AVL TREE INDEX (key = reference number)");
            sb.AppendLine("Requests: " + tree.Count + "    Height: " + tree.Height + "    Rotations so far: " + tree.Rotations);
            sb.AppendLine("Reference numbers are issued in ascending order, which would turn a plain binary search tree");
            sb.AppendLine("into a straight line (height " + tree.Count + "). Rotations keep this one near log2(n), so a lookup stays fast.");
            sb.AppendLine();
            sb.AppendLine("PRIORITY HEAP (next request to dispatch)");
            sb.AppendLine(byPriority.Count == 0
                ? "No open requests."
                : byPriority[0].ReferenceNumber + "  " + byPriority[0].Priority + "  " + byPriority[0].Category + "  " + (byPriority[0].Zone ?? "no zone"));
            sb.AppendLine();
            sb.AppendLine("TREE DIAGRAM (root at the left, larger references above, smaller below)");
            sb.AppendLine(tree.Describe(id => "SR-" + id));
            txtIndex.Text = sb.ToString();
        }
    }
}