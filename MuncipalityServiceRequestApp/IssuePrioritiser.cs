using System;
using System.Linq;

namespace MuncipalityServiceRequestApp
{
    /// <summary>
    /// Rule-based priority for a new request: the category sets a baseline and
    /// danger keywords in the description escalate it to Urgent.
    /// </summary>
    public static class IssuePrioritiser
    {
        private static readonly string[] UrgentKeywords =
        {
            "burst", "flood", "gas leak", "fire", "live wire", "exposed wire", "sparking",
            "sinkhole", "collapsed", "explosion", "sewage overflow", "unsafe"
        };

        public static EventPriority Classify(string category, string description)
        {
            if (!string.IsNullOrWhiteSpace(description) &&
                UrgentKeywords.Any(k => description.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0))
            {
                return EventPriority.Urgent;
            }

            switch ((category ?? string.Empty).Trim())
            {
                case "Public Safety":
                case "Electricity":
                case "Water & Utilities":
                    return EventPriority.High;
                case "Roads":
                case "Sanitation":
                    return EventPriority.Normal;
                default:
                    return EventPriority.Low;
            }
        }
    }
}