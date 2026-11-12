using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace MuncipalityServiceRequestApp
{
    public static class AppDataStore
    {
        private static readonly string DataDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MuncipalityServiceRequestApp");
        private static readonly string DataFile = Path.Combine(DataDirectory, "municipal-data.json");

        public static string FilePath { get { return DataFile; } }

        public static void Load()
        {
            if (!File.Exists(DataFile)) return;

            try
            {
                AppData data;
                using (FileStream stream = File.OpenRead(DataFile))
                {
                    data = new DataContractJsonSerializer(typeof(AppData)).ReadObject(stream) as AppData;
                }
                if (data == null) return;

                IssueRepository.ReplaceAll((data.Issues ?? new List<IssueData>()).Select(ToIssue));
                EventRepository.ReplaceAll((data.Events ?? new List<EventData>()).Select(ToEvent));
                if (data.Zones != null && data.Roads != null)
                {
                    ServiceZoneGraph.Default.ReplaceNetwork(data.Zones, data.Roads.Select(r => new ZoneRoad(r.From, r.To, r.Km)));
                }
            }
            catch (Exception ex)
            {
                throw new InvalidDataException("Saved municipal data could not be loaded. The demo data is still available.", ex);
            }
        }

        public static void Save()
        {
            Directory.CreateDirectory(DataDirectory);
            string temporaryFile = DataFile + ".tmp";
            AppData data = new AppData
            {
                Issues = IssueRepository.Issues.Select(ToIssue).ToList(),
                Events = EventRepository.Events.Select(ToEvent).ToList(),
                Zones = ServiceZoneGraph.Default.Zones.ToList(),
                Roads = ServiceZoneGraph.Default.Roads.Select(r => new RoadData { From = r.From, To = r.To, Km = r.Km }).ToList()
            };

            using (FileStream stream = File.Create(temporaryFile))
            {
                new DataContractJsonSerializer(typeof(AppData)).WriteObject(stream, data);
            }
            if (File.Exists(DataFile)) File.Replace(temporaryFile, DataFile, null);
            else File.Move(temporaryFile, DataFile);
        }

        private static IssueData ToIssue(Issue issue)
        {
            return new IssueData
            {
                Id = issue.Id,
                Location = issue.Location,
                Category = issue.Category,
                Description = issue.Description,
                AttachmentPaths = issue.AttachmentPaths ?? new List<string>(),
                DateReported = issue.DateReported,
                Status = issue.Status,
                Priority = issue.Priority,
                Zone = issue.Zone,
                Assignee = issue.Assignee,
                History = issue.History.Select(h => new HistoryData { When = h.When, Status = h.Status, Note = h.Note }).ToList()
            };
        }

        private static Issue ToIssue(IssueData data)
        {
            Issue issue = new Issue
            {
                Id = data.Id,
                Location = data.Location ?? string.Empty,
                Category = data.Category ?? string.Empty,
                Description = data.Description ?? string.Empty,
                AttachmentPaths = data.AttachmentPaths ?? new List<string>(),
                DateReported = data.DateReported,
                Status = data.Status,
                Priority = data.Priority,
                Zone = data.Zone,
                Assignee = data.Assignee
            };
            foreach (HistoryData history in data.History ?? new List<HistoryData>())
            {
                issue.History.Add(new StatusChange(history.When, history.Status, history.Note ?? string.Empty));
            }
            return issue;
        }

        private static EventData ToEvent(MunicipalEvent value)
        {
            return new EventData { Id = value.Id, Title = value.Title, Date = value.Date, Location = value.Location, Category = value.Category, Description = value.Description, Priority = value.Priority, PostedDate = value.PostedDate };
        }

        private static MunicipalEvent ToEvent(EventData value)
        {
            return new MunicipalEvent { Id = value.Id, Title = value.Title ?? string.Empty, Date = value.Date, Location = value.Location ?? string.Empty, Category = value.Category ?? string.Empty, Description = value.Description ?? string.Empty, Priority = value.Priority, PostedDate = value.PostedDate };
        }

        [DataContract]
        private class AppData
        {
            [DataMember] public List<IssueData> Issues { get; set; }
            [DataMember] public List<EventData> Events { get; set; }
            [DataMember] public List<string> Zones { get; set; }
            [DataMember] public List<RoadData> Roads { get; set; }
        }

        [DataContract]
        private class IssueData
        {
            [DataMember] public int Id { get; set; }
            [DataMember] public string Location { get; set; }
            [DataMember] public string Category { get; set; }
            [DataMember] public string Description { get; set; }
            [DataMember] public List<string> AttachmentPaths { get; set; }
            [DataMember] public DateTime DateReported { get; set; }
            [DataMember] public IssueStatus Status { get; set; }
            [DataMember] public EventPriority Priority { get; set; }
            [DataMember] public string Zone { get; set; }
            [DataMember] public string Assignee { get; set; }
            [DataMember] public List<HistoryData> History { get; set; }
        }

        [DataContract]
        private class HistoryData
        {
            [DataMember] public DateTime When { get; set; }
            [DataMember] public IssueStatus Status { get; set; }
            [DataMember] public string Note { get; set; }
        }

        [DataContract]
        private class EventData
        {
            [DataMember] public int Id { get; set; }
            [DataMember] public string Title { get; set; }
            [DataMember] public DateTime Date { get; set; }
            [DataMember] public string Location { get; set; }
            [DataMember] public string Category { get; set; }
            [DataMember] public string Description { get; set; }
            [DataMember] public EventPriority Priority { get; set; }
            [DataMember] public DateTime PostedDate { get; set; }
        }

        [DataContract]
        public class RoadData
        {
            [DataMember] public string From { get; set; }
            [DataMember] public string To { get; set; }
            [DataMember] public double Km { get; set; }
        }
    }
}
