using System;

namespace MuncipalityServiceRequestApp
{
    public class SearchQuery
    {
        public string Keyword { get; set; }
        public string Category { get; set; }
        public string Location { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;

        public bool IsEmpty
        {
            get
            {
                return string.IsNullOrWhiteSpace(Keyword) && string.IsNullOrWhiteSpace(Category) && string.IsNullOrWhiteSpace(Location);
            }
        }

        public bool SameCriteriaAs(SearchQuery other)
        {
            return other != null
                && string.Equals(Keyword, other.Keyword, StringComparison.OrdinalIgnoreCase)
                && string.Equals(Category, other.Category, StringComparison.OrdinalIgnoreCase)
                && string.Equals(Location, other.Location, StringComparison.OrdinalIgnoreCase);
        }

        public override string ToString()
        {
            string text = string.Empty;
            if (!string.IsNullOrWhiteSpace(Keyword)) text += "\"" + Keyword + "\" ";
            if (!string.IsNullOrWhiteSpace(Category)) text += Category + " ";
            if (!string.IsNullOrWhiteSpace(Location)) text += "@ " + Location;
            return text.Trim();
        }
    }
}