using System;
using System.Collections.Generic;
using System.Linq;

namespace MuncipalityServiceRequestApp
{
    /// <summary>
    /// Event store with several indexes, each chosen for how it is queried:
    ///   Dictionary ByCategory / ByLocation  - O(1) lookup of all events in a group
    ///   SortedDictionary ByDate             - events already in date order
    ///   HashSet Categories / Locations / EventDates - unique values for the filter lists
    ///   BinaryHeap (GetByPriorityOrder)     - most urgent events first
    /// </summary>
    public static class EventRepository
    {
        private static readonly List<MunicipalEvent> events = new List<MunicipalEvent>();

        public static IReadOnlyList<MunicipalEvent> Events { get { return events; } }

        public static Dictionary<int, MunicipalEvent> ById { get; private set; }
            = new Dictionary<int, MunicipalEvent>();

        public static Dictionary<string, List<MunicipalEvent>> ByCategory { get; private set; }
            = new Dictionary<string, List<MunicipalEvent>>(StringComparer.OrdinalIgnoreCase);

        public static Dictionary<string, List<MunicipalEvent>> ByLocation { get; private set; }
            = new Dictionary<string, List<MunicipalEvent>>(StringComparer.OrdinalIgnoreCase);

        public static SortedDictionary<DateTime, List<MunicipalEvent>> ByDate { get; private set; }
            = new SortedDictionary<DateTime, List<MunicipalEvent>>();

        public static HashSet<string> Categories { get; private set; }
            = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public static HashSet<string> Locations { get; private set; }
            = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public static HashSet<DateTime> EventDates { get; private set; }
            = new HashSet<DateTime>();

        static EventRepository()
        {
            SeedEvents();
            RebuildIndexes();
        }

        public static void UpdateEvent(MunicipalEvent updatedEvent)
        {
            if (updatedEvent == null) throw new ArgumentNullException("updatedEvent");
            MunicipalEvent existing = FindById(updatedEvent.Id);
            if (existing == null) throw new ArgumentException("The event does not exist.");
            events[events.IndexOf(existing)] = updatedEvent;
            RebuildIndexes();
        }

        public static bool RemoveEvent(int id)
        {
            MunicipalEvent existing = FindById(id);
            if (existing == null) return false;
            events.Remove(existing);
            RebuildIndexes();
            return true;
        }

        public static void ReplaceAll(IEnumerable<MunicipalEvent> restoredEvents)
        {
            events.Clear();
            if (restoredEvents != null) events.AddRange(restoredEvents.Where(e => e != null && e.Id > 0));
            RebuildIndexes();
        }

        private static void SeedEvents()
        {
            int id = 1;
            events.AddRange(new[]
            {
                new MunicipalEvent { Id = id++, Title = "Water Supply Interruption - Sunnyside", Date = DateTime.Today.AddDays(1), Location = "Sunnyside", Category = "Service Interruption", Priority = EventPriority.Urgent, Description = "Planned maintenance on the main water line. Supply interrupted 08:00-16:00.", PostedDate = DateTime.Now.AddHours(-2) },
                new MunicipalEvent { Id = id++, Title = "Load Shedding Schedule Update", Date = DateTime.Today.AddDays(1), Location = "Citywide", Category = "Public Notice", Priority = EventPriority.High, Description = "Updated load shedding stages effective this week.", PostedDate = DateTime.Now.AddHours(-5) },
                new MunicipalEvent { Id = id++, Title = "Ward 42 Community Meeting", Date = DateTime.Today.AddDays(5), Location = "Mamelodi", Category = "Community Meeting", Priority = EventPriority.Normal, Description = "Discussion on road resurfacing and ward budget allocation.", PostedDate = DateTime.Now.AddDays(-1) },
                new MunicipalEvent { Id = id++, Title = "Refuse Collection Change - Centurion", Date = DateTime.Today.AddDays(7), Location = "Centurion", Category = "Service Interruption", Priority = EventPriority.High, Description = "Refuse collection moves from Monday to Tuesday for two weeks.", PostedDate = DateTime.Now.AddDays(-1) },
                new MunicipalEvent { Id = id++, Title = "Youth Skills Development Workshop", Date = DateTime.Today.AddDays(10), Location = "Tshwane Civic Centre", Category = "Community Meeting", Priority = EventPriority.Low, Description = "Free workshop on digital skills and job readiness for unemployed youth.", PostedDate = DateTime.Now.AddDays(-2) },
                new MunicipalEvent { Id = id++, Title = "Electricity Outage - Substation Maintenance", Date = DateTime.Today.AddDays(2), Location = "Hatfield", Category = "Service Interruption", Priority = EventPriority.Urgent, Description = "Scheduled substation maintenance. Power interrupted 09:00-13:00.", PostedDate = DateTime.Now.AddHours(-1) },
                new MunicipalEvent { Id = id++, Title = "Public Participation: Draft IDP Budget", Date = DateTime.Today.AddDays(14), Location = "Centurion", Category = "Public Notice", Priority = EventPriority.Normal, Description = "Residents invited to comment on the draft Integrated Development Plan budget.", PostedDate = DateTime.Now.AddDays(-3) },
                new MunicipalEvent { Id = id++, Title = "Road Closure - Church Street Upgrade", Date = DateTime.Today.AddDays(3), Location = "Sunnyside", Category = "Service Interruption", Priority = EventPriority.High, Description = "Church Street closed to through traffic for resurfacing works.", PostedDate = DateTime.Now.AddHours(-8) },
            });
        }

        public static void RebuildIndexes()
        {
            ById = events.ToDictionary(e => e.Id);

            ByCategory = events
                .GroupBy(e => e.Category, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

            ByLocation = events
                .GroupBy(e => e.Location, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

            ByDate = new SortedDictionary<DateTime, List<MunicipalEvent>>();
            foreach (MunicipalEvent ev in events)
            {
                DateTime dateKey = ev.Date.Date;
                List<MunicipalEvent> group;
                if (!ByDate.TryGetValue(dateKey, out group))
                {
                    group = new List<MunicipalEvent>();
                    ByDate[dateKey] = group;
                }
                group.Add(ev);
            }

            Categories = new HashSet<string>(events.Select(e => e.Category), StringComparer.OrdinalIgnoreCase);
            Locations = new HashSet<string>(events.Select(e => e.Location), StringComparer.OrdinalIgnoreCase);
            EventDates = new HashSet<DateTime>(events.Select(e => e.Date.Date));
        }

        public static MunicipalEvent FindById(int id)
        {
            MunicipalEvent ev;
            return ById.TryGetValue(id, out ev) ? ev : null;
        }

        /// <summary>All events ordered by urgency (then date), by draining a binary heap.</summary>
        public static List<MunicipalEvent> GetByPriorityOrder()
        {
            BinaryHeap<MunicipalEvent> heap = new BinaryHeap<MunicipalEvent>(new EventPriorityComparer());
            foreach (MunicipalEvent ev in events) heap.Enqueue(ev);

            List<MunicipalEvent> ordered = new List<MunicipalEvent>(heap.Count);
            while (heap.Count > 0) ordered.Add(heap.Dequeue());
            return ordered;
        }

        public static void AddEvent(MunicipalEvent newEvent)
        {
            if (newEvent == null) throw new ArgumentNullException("newEvent");
            if (string.IsNullOrWhiteSpace(newEvent.Title)) throw new ArgumentException("An event needs a title.");
            if (events.Any(e => e.Id == newEvent.Id)) throw new ArgumentException("An event with this Id already exists.");

            events.Add(newEvent);
            RebuildIndexes();
        }

        /// <summary>Case-insensitive substring test (string.Contains with a comparison type does not exist on .NET Framework).</summary>
        public static bool ContainsIgnoreCase(string source, string value)
        {
            return source != null && value != null && source.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}