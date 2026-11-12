using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MuncipalityServiceRequestApp
{
    public class AdminPortalForm : Form
    {
        private DataGridView requestGrid;
        private ComboBox requestStatus;
        private TextBox requestSearch;
        private TextBox assignee;
        private TextBox requestNote;
        private DataGridView eventGrid;
        private TextBox eventTitle;
        private TextBox eventLocation;
        private TextBox eventCategory;
        private TextBox eventDescription;
        private DateTimePicker eventDate;
        private ComboBox eventPriority;
        private TextBox zoneName;
        private TextBox roadFrom;
        private TextBox roadTo;
        private TextBox roadKm;
        private ComboBox mstAlgorithm;
        private TextBox mstOutput;
        private Panel dashboard;
        private int editingEventId;

        public AdminPortalForm()
        {
            AppTheme.InitForm(this, "Administrator Portal", 1180, 760, 920, 620);
            BuildUi();
            RefreshAll();
        }

        private void BuildUi()
        {
            TabControl tabs = new TabControl { Dock = DockStyle.Fill };
            tabs.TabPages.Add(BuildDashboardPage());
            tabs.TabPages.Add(BuildRequestsPage());
            tabs.TabPages.Add(BuildEventsPage());
            tabs.TabPages.Add(BuildNetworkPage());
            Button close = new Button { Text = "Back to Main Menu", Dock = DockStyle.Bottom, Height = 42 };
            AppTheme.StyleSecondaryButton(close);
            close.Click += (s, e) => Close();
            Controls.Add(tabs);
            Controls.Add(close);
            Controls.Add(AppTheme.CreateHeaderPanel("CITY OF TSHWANE", "Administrator Portal"));
        }

        private TabPage BuildDashboardPage()
        {
            TabPage page = new TabPage("Dashboard") { BackColor = AppTheme.LightBackground, Padding = new Padding(16) };
            dashboard = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = AppTheme.LightBackground };
            page.Controls.Add(dashboard);
            return page;
        }

        private TabPage BuildRequestsPage()
        {
            TabPage page = new TabPage("Manage Service Requests") { BackColor = AppTheme.LightBackground, Padding = new Padding(10) };
            TableLayoutPanel layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1 };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 65));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 35));
            FlowLayoutPanel toolbar = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
            requestSearch = new TextBox { Width = 180 };
            requestStatus = new ComboBox { Width = 130, DropDownStyle = ComboBoxStyle.DropDownList };
            requestStatus.Items.Add("All statuses");
            foreach (IssueStatus status in Enum.GetValues(typeof(IssueStatus))) requestStatus.Items.Add(GetReadableStatus(status));
            requestStatus.SelectedIndex = 0;
            Button search = new Button { Text = "Search requests", Width = 110 };
            search.Click += (s, e) => RefreshRequests();
            toolbar.Controls.AddRange(new Control[] { new Label { Text = "Search requests:" }, requestSearch, new Label { Text = "Request stage:" }, requestStatus, search });
            requestGrid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoGenerateColumns = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            AddColumn(requestGrid, "Request number", "ReferenceNumber");
            AddColumn(requestGrid, "Current stage", "Status");
            AddColumn(requestGrid, "Category", "Category");
            AddColumn(requestGrid, "Priority", "Priority");
            AddColumn(requestGrid, "Assigned team or officer", "Assignee");
            AddColumn(requestGrid, "Service area", "Zone");
            requestGrid.SelectionChanged += (s, e) => ShowSelectedRequest();
            FlowLayoutPanel actions = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true };
            assignee = new TextBox { Width = 150 };
            requestNote = new TextBox { Width = 220 };
            Button assign = ActionButton("Assign", (s, e) => UpdateRequest(r => r.Assignee = assignee.Text.Trim()));
            Button advance = ActionButton("Move to next stage", (s, e) => UpdateRequest(r => IssueRepository.TryAdvance(r, requestNote.Text, out _)));
            Button note = ActionButton("Add note", (s, e) => UpdateRequest(r => r.AddNote(requestNote.Text)));
            actions.Controls.AddRange(new Control[] { new Label { Text = "Assignee:" }, assignee, assign, new Label { Text = "Note:" }, requestNote, advance, note });
            layout.Controls.Add(toolbar, 0, 0);
            layout.Controls.Add(requestGrid, 0, 1);
            layout.Controls.Add(actions, 0, 2);
            page.Controls.Add(layout);
            return page;
        }

        private TabPage BuildEventsPage()
        {
            TabPage page = new TabPage("Events and Announcements") { BackColor = AppTheme.LightBackground, Padding = new Padding(10) };
            TableLayoutPanel layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            eventGrid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoGenerateColumns = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            AddColumn(eventGrid, "Title", "Title"); AddColumn(eventGrid, "Date", "Date"); AddColumn(eventGrid, "Location", "Location"); AddColumn(eventGrid, "Category", "Category");
            eventGrid.SelectionChanged += (s, e) => LoadSelectedEvent();
            TableLayoutPanel editor = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, AutoScroll = true };
            editor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35)); editor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));
            eventTitle = AddEditor(editor, "Title:"); eventDate = new DateTimePicker { Dock = DockStyle.Fill }; AddEditorControl(editor, "Date:", eventDate);
            eventLocation = AddEditor(editor, "Location:"); eventCategory = AddEditor(editor, "Category:");
            eventPriority = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList }; eventPriority.Items.AddRange(Enum.GetNames(typeof(EventPriority))); eventPriority.SelectedIndex = 2; AddEditorControl(editor, "Priority:", eventPriority);
            eventDescription = AddEditor(editor, "Description:", 80);
            FlowLayoutPanel buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
            buttons.Controls.Add(ActionButton("Save event", SaveEvent)); buttons.Controls.Add(ActionButton("New", (s, e) => ClearEventEditor())); buttons.Controls.Add(ActionButton("Remove", RemoveEvent));
            AddEditorControl(editor, "", buttons);
            layout.Controls.Add(eventGrid, 0, 0); layout.Controls.Add(editor, 1, 0); page.Controls.Add(layout);
            return page;
        }

        private TabPage BuildNetworkPage()
        {
            TabPage page = new TabPage("Service Zone Network") { BackColor = AppTheme.LightBackground, Padding = new Padding(10) };
            FlowLayoutPanel controls = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 90, WrapContents = true };
            zoneName = new TextBox { Width = 150 }; roadFrom = new TextBox { Width = 120 }; roadTo = new TextBox { Width = 120 }; roadKm = new TextBox { Width = 70 };
            controls.Controls.AddRange(new Control[] { new Label { Text = "New service area:" }, zoneName, ActionButton("Add service area", AddZone), new Label { Text = "Road from:" }, roadFrom, new Label { Text = "to:" }, roadTo, new Label { Text = "Distance (km):" }, roadKm, ActionButton("Add road", AddRoad) });
            mstAlgorithm = new ComboBox { Width = 170, DropDownStyle = ComboBoxStyle.DropDownList }; mstAlgorithm.Items.AddRange(new object[] { "Shortest roads first", "Grow from a starting area" }); mstAlgorithm.SelectedIndex = 0;
            Button showMst = ActionButton("Show minimum road network", ShowMst); controls.Controls.AddRange(new Control[] { new Label { Text = "Calculation method:" }, mstAlgorithm, showMst });
            mstOutput = new TextBox { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Both };
            page.Controls.Add(mstOutput); page.Controls.Add(controls); return page;
        }

        private void RefreshAll() { RefreshDashboard(); RefreshRequests(); RefreshEvents(); ShowMst(null, EventArgs.Empty); }

        private void RefreshDashboard()
        {
            dashboard.Controls.Clear();

            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 3,
                RowCount = 2,
                Padding = new Padding(4)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 92));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            int total = IssueRepository.Issues.Count;
            int open = IssueRepository.OpenCount();
            int resolved = IssueRepository.Issues.Count(i => i.Status == IssueStatus.Resolved);
            layout.Controls.Add(CreateMetricCard("Total service requests", total.ToString()), 0, 0);
            layout.Controls.Add(CreateMetricCard("Open requests", open.ToString()), 1, 0);
            layout.Controls.Add(CreateMetricCard("Completed requests", resolved.ToString()), 2, 0);

            layout.Controls.Add(CreateSummaryBox("Requests by stage", IssueRepository.Issues
                .GroupBy(i => GetReadableStatus(i.Status))
                .Select(g => g.Key + ": " + g.Count())), 0, 1);
            layout.Controls.Add(CreateSummaryBox("Requests by service category", IssueRepository.Issues
                .GroupBy(i => string.IsNullOrWhiteSpace(i.Category) ? "Uncategorised" : i.Category)
                .Select(g => g.Key + ": " + g.Count())), 1, 1);
            layout.Controls.Add(CreateSummaryBox("Requests by priority", IssueRepository.Issues
                .GroupBy(i => GetReadablePriority(i.Priority))
                .Select(g => g.Key + ": " + g.Count())), 2, 1);

            dashboard.Controls.Add(layout);
        }

        private static GroupBox CreateMetricCard(string title, string value)
        {
            GroupBox card = new GroupBox { Text = title, Dock = DockStyle.Fill, Margin = new Padding(6), Padding = new Padding(10) };
            Label number = new Label { Text = value, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, Font = AppTheme.FontDisplay, ForeColor = AppTheme.Green };
            card.Controls.Add(number);
            return card;
        }

        private static GroupBox CreateSummaryBox(string title, IEnumerable<string> values)
        {
            GroupBox box = new GroupBox { Text = title, Dock = DockStyle.Fill, AutoSize = true, MinimumSize = new Size(220, 150), Margin = new Padding(6), Padding = new Padding(8) };
            ListBox list = new ListBox { Dock = DockStyle.Fill, BorderStyle = BorderStyle.None, IntegralHeight = false };
            list.Items.AddRange(values.OrderBy(v => v).Cast<object>().ToArray());
            box.Controls.Add(list);
            return box;
        }

        private static string GetReadableStatus(IssueStatus status)
        {
            switch (status)
            {
                case IssueStatus.Logged: return "Received";
                case IssueStatus.Assigned: return "Assigned";
                case IssueStatus.InProgress: return "Work in progress";
                case IssueStatus.Resolved: return "Completed";
                default: return "Other";
            }
        }

        private static string GetReadablePriority(EventPriority priority)
        {
            switch (priority)
            {
                case EventPriority.Urgent: return "Urgent";
                case EventPriority.High: return "High";
                case EventPriority.Normal: return "Standard";
                case EventPriority.Low: return "Low";
                default: return "Other";
            }
        }

        private void RefreshRequests()
        {
            if (requestGrid == null) return;
            requestGrid.Rows.Clear();
            string search = requestSearch.Text.Trim(); string selectedStatus = requestStatus.SelectedItem as string;
            foreach (Issue issue in IssueRepository.Issues.Where(i => (string.IsNullOrEmpty(search) || (i.ReferenceNumber + " " + i.Description + " " + i.Location).IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0) && (selectedStatus == "All statuses" || GetReadableStatus(i.Status) == selectedStatus)))
            {
                int row = requestGrid.Rows.Add(issue.ReferenceNumber, GetReadableStatus(issue.Status), issue.Category, GetReadablePriority(issue.Priority), issue.Assignee ?? "", issue.Zone ?? ""); requestGrid.Rows[row].Tag = issue;
            }
            RefreshDashboard();
        }

        private void ShowSelectedRequest()
        {
            Issue issue = requestGrid != null && requestGrid.SelectedRows.Count > 0 ? requestGrid.SelectedRows[0].Tag as Issue : null;
            if (issue != null) { assignee.Text = issue.Assignee ?? ""; requestNote.Clear(); }
        }

        private void UpdateRequest(Action<Issue> update)
        {
            Issue issue = requestGrid.SelectedRows.Count > 0 ? requestGrid.SelectedRows[0].Tag as Issue : null;
            if (issue == null) { MessageBox.Show("Select a request first."); return; }
            try { update(issue); AppDataStore.Save(); RefreshRequests(); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Request update failed", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }

        private void RefreshEvents()
        {
            if (eventGrid == null) return; eventGrid.Rows.Clear();
            foreach (MunicipalEvent value in EventRepository.Events.OrderBy(e => e.Date)) { int row = eventGrid.Rows.Add(value.Title, value.Date.ToShortDateString(), value.Location, value.Category); eventGrid.Rows[row].Tag = value; }
        }

        private void LoadSelectedEvent()
        {
            MunicipalEvent value = eventGrid.SelectedRows.Count > 0 ? eventGrid.SelectedRows[0].Tag as MunicipalEvent : null; if (value == null) return;
            editingEventId = value.Id; eventTitle.Text = value.Title; eventDate.Value = value.Date; eventLocation.Text = value.Location; eventCategory.Text = value.Category; eventDescription.Text = value.Description; eventPriority.SelectedItem = value.Priority.ToString();
        }

        private void SaveEvent(object sender, EventArgs e)
        {
            try
            {
                MunicipalEvent value = new MunicipalEvent { Id = editingEventId == 0 ? (EventRepository.Events.Count == 0 ? 1 : EventRepository.Events.Max(x => x.Id) + 1) : editingEventId, Title = eventTitle.Text.Trim(), Date = eventDate.Value.Date, Location = eventLocation.Text.Trim(), Category = eventCategory.Text.Trim(), Description = eventDescription.Text.Trim(), Priority = (EventPriority)Enum.Parse(typeof(EventPriority), eventPriority.SelectedItem.ToString()), PostedDate = DateTime.Now };
                if (editingEventId == 0) EventRepository.AddEvent(value); else EventRepository.UpdateEvent(value);
                AppDataStore.Save(); ClearEventEditor(); RefreshEvents();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Event save failed", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }

        private void RemoveEvent(object sender, EventArgs e)
        {
            MunicipalEvent value = eventGrid.SelectedRows.Count > 0 ? eventGrid.SelectedRows[0].Tag as MunicipalEvent : null; if (value == null) return;
            if (MessageBox.Show("Remove this event?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            EventRepository.RemoveEvent(value.Id); AppDataStore.Save(); ClearEventEditor(); RefreshEvents();
        }

        private void ClearEventEditor() { editingEventId = 0; eventTitle.Clear(); eventLocation.Clear(); eventCategory.Clear(); eventDescription.Clear(); eventDate.Value = DateTime.Today; eventPriority.SelectedIndex = 2; }

        private void AddZone(object sender, EventArgs e) { try { ServiceZoneGraph.Default.AddZone(zoneName.Text); AppDataStore.Save(); zoneName.Clear(); } catch (Exception ex) { MessageBox.Show(ex.Message); } }
        private void AddRoad(object sender, EventArgs e) { try { ServiceZoneGraph.Default.AddRoad(roadFrom.Text, roadTo.Text, double.Parse(roadKm.Text)); AppDataStore.Save(); } catch (Exception ex) { MessageBox.Show(ex.Message); } }
        private void ShowMst(object sender, EventArgs e)
        {
            try { List<ZoneRoad> roads = mstAlgorithm.SelectedIndex == 1 ? ServiceZoneGraph.Default.PrimMst(ServiceZoneGraph.Default.Zones.First()) : ServiceZoneGraph.Default.KruskalMst(); mstOutput.Text = "Zones: " + string.Join(", ", ServiceZoneGraph.Default.Zones) + "\r\nConnected: " + ServiceZoneGraph.Default.IsConnected() + "\r\n\r\nMinimum spanning tree:\r\n" + string.Join("\r\n", roads) + "\r\nTotal: " + roads.Sum(r => r.Km).ToString("0.#") + " km"; } catch (Exception ex) { mstOutput.Text = ex.Message; }
        }

        private static void AddColumn(DataGridView grid, string header, string property) { grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = header, DataPropertyName = property, FillWeight = 1 }); }
        private static Button ActionButton(string text, EventHandler click) { Button button = new Button { Text = text, AutoSize = true }; AppTheme.StylePrimaryButton(button); button.Click += click; return button; }
        private static TextBox AddEditor(TableLayoutPanel panel, string label, int height = 28) { TextBox box = new TextBox { Dock = DockStyle.Fill, Height = height, Multiline = height > 28 }; AddEditorControl(panel, label, box); return box; }
        private static void AddEditorControl(TableLayoutPanel panel, string label, Control control) { int row = panel.RowCount++; panel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); panel.Controls.Add(new Label { Text = label, AutoSize = true }, 0, row); panel.Controls.Add(control, 1, row); }
    }
}
