using System;
using System.Collections.Generic;
using System.Linq;

namespace MuncipalityServiceRequestApp
{
    
    /// Score events by how well they match the user's recent search behaviour
    /// (category + location affinity, weighted by recency of the search) plus
    /// how recently the event itself was posted. Falls back to urgency order
    /// when there's no search history yet (cold start).
    public static class RecommendationEngine
    {
        private const double CategoryWeight = 0.5;
        private const double LocationWeight = 0.3;
        private const double RecencyWeight = 0.2;
        private const int DefaultRecommendationCount = 5;

        public static List<MunicipalEvent> GetRecommendations(int count = DefaultRecommendationCount)
        {
            List<SearchQuery> recentSearches = SearchHistoryService.GetRecentSearches(20);

            if (recentSearches.Count == 0)
            {
                
                return EventRepository.GetByPriorityOrder()
                    .Where(e => !SearchHistoryService.HasBeenViewed(e.Id))
                    .Take(count)
                    .ToList();
            }

            Dictionary<string, double> categoryAffinity = BuildAffinity(recentSearches.Select(q => q.Category));
            Dictionary<string, double> locationAffinity = BuildAffinity(recentSearches.Select(q => q.Location));
            DateTime now = DateTime.Now;

            return EventRepository.Events
                .Where(e => !SearchHistoryService.HasBeenViewed(e.Id))
                .Select(e => new
                {
                    Event = e,
                    Score = CategoryWeight * GetOrZero(categoryAffinity, e.Category)
                          + LocationWeight * GetOrZero(locationAffinity, e.Location)
                          + RecencyWeight * RecencyScore(e.PostedDate, now)
                })
                .OrderByDescending(x => x.Score)
                .ThenBy(x => x.Event.Date)
                .Take(count)
                .Select(x => x.Event)
                .ToList();
        }

   
        private static Dictionary<string, double> BuildAffinity(IEnumerable<string?> values)
        {
            var affinity = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
            int rank = 0;

            foreach (string? value in values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    double weight = 1.0 / (rank + 1);
                    affinity[value] = affinity.TryGetValue(value, out double existing) ? existing + weight : weight;
                }
                rank++;
            }

            return affinity;
        }

        private static double GetOrZero(Dictionary<string, double> affinity, string key)
        {
            return affinity.TryGetValue(key, out double value) ? value : 0.0;
        }

     
        private static double RecencyScore(DateTime postedDate, DateTime now)
        {
            double daysSincePosted = Math.Max(0, (now - postedDate).TotalDays);
            return 1.0 / (1.0 + daysSincePosted);
        }
    }
}