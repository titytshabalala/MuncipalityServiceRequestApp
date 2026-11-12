using System.Collections.Generic;
using System.Linq;

namespace MuncipalityServiceRequestApp
{
    /// <summary>
    /// Tracks what the user searched for and viewed during this session.
    /// Feeds the recommendation engine.
    ///   Stack  (LIFO) - search history, so the newest search is always on top
    ///   Queue  (FIFO) - recently viewed events; the oldest is dropped when full
    ///   HashSet       - O(1) "have I already viewed this event?"
    /// </summary>
    public static class SearchHistoryService
    {
        private const int RecentlyViewedCapacity = 10;

        private static readonly Stack<SearchQuery> searchHistory = new Stack<SearchQuery>();
        private static readonly Queue<int> recentlyViewedQueue = new Queue<int>();
        private static readonly HashSet<int> viewedEventIds = new HashSet<int>();

        public static IReadOnlyCollection<SearchQuery> History { get { return searchHistory; } }
        public static IReadOnlyCollection<int> RecentlyViewed { get { return recentlyViewedQueue; } }

        /// <summary>
        /// Records a search. Empty searches and an exact repeat of the previous search
        /// are ignored so they cannot skew the recommendations. Returns true if recorded.
        /// </summary>
        public static bool RecordSearch(SearchQuery query)
        {
            if (query == null || query.IsEmpty) return false;
            if (searchHistory.Count > 0 && query.SameCriteriaAs(searchHistory.Peek())) return false;

            searchHistory.Push(query);
            return true;
        }

        public static SearchQuery GetLastSearch()
        {
            return searchHistory.Count > 0 ? searchHistory.Peek() : null;
        }

        public static bool HasBeenViewed(int eventId)
        {
            return viewedEventIds.Contains(eventId);
        }

        public static void RecordView(int eventId)
        {
            if (viewedEventIds.Contains(eventId)) return;

            recentlyViewedQueue.Enqueue(eventId);
            viewedEventIds.Add(eventId);

            if (recentlyViewedQueue.Count > RecentlyViewedCapacity)
            {
                int evictedId = recentlyViewedQueue.Dequeue();
                viewedEventIds.Remove(evictedId);
            }
        }

        /// <summary>Newest search first.</summary>
        public static List<SearchQuery> GetRecentSearches(int count)
        {
            return searchHistory.Take(count).ToList();
        }

        /// <summary>Most recently viewed event ids first.</summary>
        public static List<int> GetRecentlyViewedNewestFirst()
        {
            return recentlyViewedQueue.Reverse().ToList();
        }

        public static void Reset()
        {
            searchHistory.Clear();
            recentlyViewedQueue.Clear();
            viewedEventIds.Clear();
        }
    }
}