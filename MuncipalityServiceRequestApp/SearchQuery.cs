using System;

namespace MuncipalityServiceRequestApp
{
    
    public class SearchQuery
    {
        public string? Keyword { get; set; }
        public string? Category { get; set; }
        public string? Location { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}