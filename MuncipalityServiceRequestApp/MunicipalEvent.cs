using System;

namespace MuncipalityServiceRequestApp
{
    public class MunicipalEvent
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime Date { get; set; }                   public string Location { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public EventPriority Priority { get; set; } = EventPriority.Normal;
        public DateTime PostedDate { get; set; } = DateTime.Now; 
    }
}