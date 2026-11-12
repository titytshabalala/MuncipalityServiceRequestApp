using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MuncipalityServiceRequestApp
{
    /// <summary>
    /// In-memory store of every reported issue.
    ///   List      - the master collection (insertion order, newest last)
    ///   AVL tree  - index by reference number for O(log n) lookup
    ///   Heap      - builds the "most urgent first" ordering on demand
    /// </summary>
    public static class IssueRepository
    {
        private static readonly List<Issue> issues = new List<Issue>();
        private static readonly AvlTree<int, Issue> index = new AvlTree<int, Issue>();
        private static int nextId = 1001;

        public static IReadOnlyList<Issue> Issues { get { return issues; } }
        public static AvlTree<int, Issue> Index { get { return index; } }

        static IssueRepository()
        {
            SeedDemoData();
        }

        public static Issue Add(string location, string category, string description, IEnumerable<string> attachments)
        {
            return AddAt(location, category, description, attachments, DateTime.Now);
        }

        private static Issue AddAt(string location, string category, string description, IEnumerable<string> attachments, DateTime reportedAt)
        {
            if (string.IsNullOrWhiteSpace(location)) throw new ArgumentException("A location is required.");
            if (string.IsNullOrWhiteSpace(category)) throw new ArgumentException("A category is required.");
            if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("A description is required.");

            Issue issue = new Issue
            {
                Id = nextId++,
                Location = location.Trim(),
                Category = category.Trim(),
                Description = description.Trim(),
                AttachmentPaths = attachments != null ? attachments.ToList() : new List<string>(),
                DateReported = reportedAt,
                Status = IssueStatus.Logged
            };
            issue.Priority = IssuePrioritiser.Classify(issue.Category, issue.Description);
            issue.Zone = ServiceZoneGraph.Default.FindZone(issue.Location);
            issue.History.Add(new StatusChange(reportedAt, IssueStatus.Logged, "Request received"));

            issues.Add(issue);
            index.Insert(issue.Id, issue);
            return issue;
        }

        /// <summary>Accepts "SR-1001", "sr1001", "#1001" or "1001".</summary>
        public static bool TryParseReference(string input, out int id)
        {
            id = 0;
            if (string.IsNullOrWhiteSpace(input)) return false;

            string text = input.Trim();
            bool hadPrefix = text.StartsWith("SR", StringComparison.OrdinalIgnoreCase);
            if (hadPrefix) text = text.Substring(2).TrimStart();
            if (text.StartsWith("#") || (hadPrefix && text.StartsWith("-"))) text = text.Substring(1).TrimStart();

            return int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out id) && id > 0;
        }

        public static bool TryGetById(int id, out Issue issue)
        {
            return index.TryGetValue(id, out issue);
        }

        /// <summary>Moves a request to its next status. Resolved requests cannot move further.</summary>
        public static bool TryAdvance(Issue issue, string note, out string error)
        {
            return AdvanceAt(issue, note, DateTime.Now, out error);
        }

        private static bool AdvanceAt(Issue issue, string note, DateTime when, out string error)
        {
            error = null;
            if (issue == null) { error = "No request is selected."; return false; }
            if (issue.Status == IssueStatus.Resolved) { error = issue.ReferenceNumber + " is already resolved."; return false; }

            issue.Status = (IssueStatus)((int)issue.Status + 1);
            string text = string.IsNullOrWhiteSpace(note) ? DefaultNote(issue.Status) : note.Trim();
            issue.History.Add(new StatusChange(when, issue.Status, text));
            return true;
        }

        public static string DefaultNote(IssueStatus status)
        {
            switch (status)
            {
                case IssueStatus.Assigned: return "Assigned to a field team";
                case IssueStatus.InProgress: return "Work has started on site";
                case IssueStatus.Resolved: return "Work completed";
                default: return "Request received";
            }
        }

        /// <summary>Most urgent first (then oldest first), produced by draining a binary heap.</summary>
        public static List<Issue> GetByPriority(Func<Issue, bool> filter = null)
        {
            BinaryHeap<Issue> heap = new BinaryHeap<Issue>(Comparer<Issue>.Create((a, b) =>
            {
                int c = a.Priority.CompareTo(b.Priority);
                if (c != 0) return c;
                c = a.DateReported.CompareTo(b.DateReported);
                return c != 0 ? c : a.Id.CompareTo(b.Id);
            }));

            foreach (Issue issue in issues)
            {
                if (filter == null || filter(issue)) heap.Enqueue(issue);
            }

            List<Issue> ordered = new List<Issue>(heap.Count);
            while (heap.Count > 0) ordered.Add(heap.Dequeue());
            return ordered;
        }

        /// <summary>Ascending reference number order via the AVL tree's in-order traversal.</summary>
        public static List<Issue> GetInReferenceOrder()
        {
            return index.InOrder().Select(kv => kv.Value).ToList();
        }

        public static int OpenCount()
        {
            return issues.Count(i => i.Status != IssueStatus.Resolved);
        }

        public static void ReplaceAll(IEnumerable<Issue> restoredIssues)
        {
            issues.Clear();
            index.Clear();
            nextId = 1001;

            foreach (Issue issue in restoredIssues ?? Enumerable.Empty<Issue>())
            {
                if (issue == null || issue.Id <= 0) continue;
                if (issue.AttachmentPaths == null) issue.AttachmentPaths = new List<string>();
                issues.Add(issue);
                index.Insert(issue.Id, issue);
                nextId = Math.Max(nextId, issue.Id + 1);
            }
        }

        // ---------- demo data so every screen has something to show on first launch ----------

        private static void SeedDemoData()
        {
            Seed("Corner Church St & Lilian Ngoyi St, Sunnyside", "Water & Utilities", "Burst water main flooding the intersection.", 3, IssueStatus.InProgress);
            Seed("Hatfield Square, Hatfield", "Electricity", "Street lights have been off along the whole block since Monday night.", 2, IssueStatus.Assigned);
            Seed("Main Road, Mamelodi", "Roads", "Large pothole damaging vehicles near the bus stop.", 5, IssueStatus.Logged);
            Seed("Centurion Mall parking area, Centurion", "Sanitation", "Refuse bins overflowing and not collected for two weeks.", 7, IssueStatus.Resolved);
            Seed("Brooklyn Circle, Brooklyn", "Public Safety", "Traffic light is broken at the circle.", 1, IssueStatus.Logged);
            Seed("Plot 12, Hammanskraal", "Other", "Stray animals roaming on the main road.", 4, IssueStatus.Logged); // outside the zone network
        }

        private static void Seed(string location, string category, string description, int daysAgo, IssueStatus target)
        {
            DateTime reported = DateTime.Now.AddDays(-daysAgo);
            Issue issue = AddAt(location, category, description, null, reported);

            int step = 1;
            string error;
            while (issue.Status < target)
            {
                AdvanceAt(issue, null, reported.AddHours(6 * step++), out error);
            }
        }
    }
}