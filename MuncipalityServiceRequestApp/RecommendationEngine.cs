using System;
using System.Collections.Generic;
using System.Linq;

namespace MuncipalityServiceRequestApp
{
    public class Recommendation
    {
        public MunicipalEvent Event { get; set; }
        public double Score { get; set; }
        public string Reason { get; set; }
    }

    /// <summary>
    /// Scores every event the user has not yet viewed:
    ///
    ///   Score = 0.40 x category affinity + 0.25 x location affinity
    ///         + 0.15 x keyword affinity  + 0.20 x recency of the posting
    ///
    /// Affinities (each scaled to 0..1) come from the user's recent searches and
    /// viewed events; newer activity counts for more (weight 1/(rank+1)).
    /// With no activity yet (cold start) it falls back to the urgency order.
    /// </summary>
    public static class RecommendationEngine
    {
        public const double CategoryWeight = 0.40;
        public const double LocationWeight = 0.25;
        public const double KeywordWeight = 0.15;
        public const double RecencyWeight = 0.20;

        private const double ViewedWeight = 0.5; // a viewed event counts half as much as a search
        private const int DefaultRecommendationCount = 5;

        public static List<Recommendation> GetRecommendations(int count = DefaultRecommendationCount)
        {
            List<SearchQuery> searches = SearchHistoryService.GetRecentSearches(20);
            List<MunicipalEvent> viewed = SearchHistoryService.GetRecentlyViewedNewestFirst()
                .Select(EventRepository.FindById).Where(e => e != null).ToList();

            if (searches.Count == 0 && viewed.Count == 0)
            {
                return EventRepository.GetByPriorityOrder()
                    .Where(e => !SearchHistoryService.HasBeenViewed(e.Id))
                    .Take(count)
                    .Select(e => new Recommendation { Event = e, Score = 0, Reason = e.Priority + " priority notice" })
                    .ToList();
            }

            Dictionary<string, double> categoryAffinity = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, double> locationAffinity = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, double> keywordAffinity = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < searches.Count; i++)
            {
                double weight = 1.0 / (i + 1);
                AddWeight(categoryAffinity, searches[i].Category, weight);
                AddWeight(locationAffinity, searches[i].Location, weight);
                AddWeight(keywordAffinity, searches[i].Keyword, weight);
            }
            for (int i = 0; i < viewed.Count; i++)
            {
                double weight = ViewedWeight / (i + 1);
                AddWeight(categoryAffinity, viewed[i].Category, weight);
                AddWeight(locationAffinity, viewed[i].Location, weight);
            }

            double maxCategory = Max(categoryAffinity);
            double maxLocation = Max(locationAffinity);
            double maxKeyword = Max(keywordAffinity);
            DateTime now = DateTime.Now;

            List<Recommendation> scored = new List<Recommendation>();
            foreach (MunicipalEvent ev in EventRepository.Events.Where(e => !SearchHistoryService.HasBeenViewed(e.Id)))
            {
                double category = Scaled(categoryAffinity, ev.Category, maxCategory);
                double location = Scaled(locationAffinity, ev.Location, maxLocation);

                string matchedKeyword = null;
                double keyword = 0;
                foreach (KeyValuePair<string, double> kv in keywordAffinity)
                {
                    if (EventRepository.ContainsIgnoreCase(ev.Title, kv.Key) || EventRepository.ContainsIgnoreCase(ev.Description, kv.Key))
                    {
                        keyword += kv.Value;
                        if (matchedKeyword == null) matchedKeyword = kv.Key;
                    }
                }
                keyword = maxKeyword > 0 ? Math.Min(1.0, keyword / maxKeyword) : 0;

                double recency = 1.0 / (1.0 + Math.Max(0, (now - ev.PostedDate).TotalDays));

                List<string> reasons = new List<string>();
                if (category > 0) reasons.Add("you follow " + ev.Category);
                if (location > 0) reasons.Add("near " + ev.Location);
                if (matchedKeyword != null) reasons.Add("matches \"" + matchedKeyword + "\"");
                if (reasons.Count == 0) reasons.Add("recently posted");

                scored.Add(new Recommendation
                {
                    Event = ev,
                    Score = CategoryWeight * category + LocationWeight * location + KeywordWeight * keyword + RecencyWeight * recency,
                    Reason = char.ToUpper(reasons[0][0]) + string.Join(", ", reasons).Substring(1)
                });
            }

            return scored
                .OrderByDescending(r => r.Score)
                .ThenBy(r => r.Event.Date)
                .Take(count)
                .ToList();
        }

        private static void AddWeight(Dictionary<string, double> affinity, string key, double weight)
        {
            if (string.IsNullOrWhiteSpace(key)) return;
            double existing;
            affinity[key.Trim()] = affinity.TryGetValue(key.Trim(), out existing) ? existing + weight : weight;
        }

        private static double Max(Dictionary<string, double> affinity)
        {
            return affinity.Count == 0 ? 0 : affinity.Values.Max();
        }

        private static double Scaled(Dictionary<string, double> affinity, string key, double max)
        {
            double value;
            if (max <= 0 || key == null || !affinity.TryGetValue(key, out value)) return 0;
            return value / max;
        }
    }
}