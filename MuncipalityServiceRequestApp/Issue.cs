using System;
using System.Collections.Generic;

namespace MuncipalityServiceRequestApp
{
    public enum IssueStatus
    {
        Logged = 0,
        Assigned = 1,
        InProgress = 2,
        Resolved = 3
    }

    public class StatusChange
    {
        public DateTime When { get; private set; }
        public IssueStatus Status { get; private set; }
        public string Note { get; private set; }

        public StatusChange(DateTime when, IssueStatus status, string note)
        {
            When = when;
            Status = status;
            Note = note;
        }
    }

    public class Issue
    {
        public int Id { get; set; }
        public string ReferenceNumber { get { return "SR-" + Id; } }

        public string Location { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public List<string> AttachmentPaths { get; set; } = new List<string>();
        public DateTime DateReported { get; set; }

        public IssueStatus Status { get; set; } = IssueStatus.Logged;
        public EventPriority Priority { get; set; } = EventPriority.Normal;

        /// <summary>Service zone found in the location text, or null if it is outside the known network.</summary>
        public string Zone { get; set; }
        public string Assignee { get; set; }

        public List<StatusChange> History { get; } = new List<StatusChange>();

        public void AddNote(string note)
        {
            if (string.IsNullOrWhiteSpace(note)) throw new ArgumentException("A note is required.", "note");
            History.Add(new StatusChange(DateTime.Now, Status, note.Trim()));
        }

        public static string StatusText(IssueStatus status)
        {
            return status == IssueStatus.InProgress ? "In Progress" : status.ToString();
        }
    }
}