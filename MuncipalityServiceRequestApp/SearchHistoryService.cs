using System.Collections.Generic;
using System.Linq;

namespace MuncipalityServiceRequestApp
{
    /// Track what a user has searched for and viewed, in-memory, for the
    /// duration of the session. Feeds the recommendation engine.
    public static class SearchHistoryService
    {
        private const int RecentlyViewedCapacity = 10;

        // LIFO principle
        private static readonly Stack<SearchQuery> searchHistory = new Stack<SearchQuery>();

        // FIFO principle
        private static readonly Queue<int> recentlyViewedQueue = new Queue<int>();

        private static readonly HashSet<int> viewedEventIds = new HashSet<int>();

        public static IReadOnlyCollection<SearchQuery> History => searchHistory;
        public static IReadOnlyCollection<int> RecentlyViewed => recentlyViewedQueue;

        public static void RecordSearch(SearchQuery query)
        {
            searchHistory.Push(query);
        }

        public static SearchQuery? GetLastSearch()
        {
            return searchHistory.Count > 0 ? searchHistory.Peek() : null;
        }

        public static bool HasBeenViewed(int eventId) => viewedEventIds.Contains(eventId);

        public static void RecordView(int eventId)
        {
            if (viewedEventIds.Contains(eventId))
            {
                return; // already tracked, nothing to do
            }

            recentlyViewedQueue.Enqueue(eventId);
            viewedEventIds.Add(eventId);

            if (recentlyViewedQueue.Count > RecentlyViewedCapacity)
            {
                int evictedId = recentlyViewedQueue.Dequeue();
                viewedEventIds.Remove(evictedId);
            }
        }

        
        public static List<SearchQuery> GetRecentSearches(int count)
        {
            return searchHistory.Take(count).ToList();
        }

        public static void Reset()
        {
            searchHistory.Clear();
            recentlyViewedQueue.Clear();
            viewedEventIds.Clear();
        }
    }
}