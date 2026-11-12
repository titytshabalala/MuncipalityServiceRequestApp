using System;
using System.Collections.Generic;
using System.Linq;

namespace MuncipalityServiceRequestApp
{
    /// <summary>An undirected, weighted road between two service zones.</summary>
    public struct ZoneRoad
    {
        public readonly string From;
        public readonly string To;
        public readonly double Km;

        public ZoneRoad(string from, string to, double km) { From = from; To = to; Km = km; }

        public string Other(string zone)
        {
            return string.Equals(zone, From, StringComparison.OrdinalIgnoreCase) ? To : From;
        }

        public override string ToString()
        {
            return From + " \u2014 " + To + "  (" + Km.ToString("0.#") + " km)";
        }
    }

    /// <summary>
    /// Weighted undirected graph (adjacency list) of Tshwane service zones.
    /// Nodes are zones, edges are roads that maintenance crews can drive.
    /// Offers BFS / DFS traversal and two minimum-spanning-tree algorithms
    /// (Kruskal with union-find, Prim with a binary heap).
    /// </summary>
    public class ServiceZoneGraph
    {
        private static readonly StringComparer Cmp = StringComparer.OrdinalIgnoreCase;

        private readonly Dictionary<string, string> canonicalNames = new Dictionary<string, string>(Cmp);
        private readonly Dictionary<string, List<ZoneRoad>> adjacency = new Dictionary<string, List<ZoneRoad>>(Cmp);
        private readonly List<ZoneRoad> roads = new List<ZoneRoad>();
        private readonly Dictionary<string, string> aliases = new Dictionary<string, string>(Cmp);

        /// <summary>The Tshwane sample network used by the app. Distances are approximate.</summary>
        public static readonly ServiceZoneGraph Default = CreateTshwaneNetwork();

        public int ZoneCount { get { return adjacency.Count; } }
        public IReadOnlyList<ZoneRoad> Roads { get { return roads; } }

        public IReadOnlyList<string> Zones
        {
            get { return canonicalNames.Values.OrderBy(z => z, Cmp).ToList(); }
        }

        public bool HasZone(string zone)
        {
            return !string.IsNullOrWhiteSpace(zone) && canonicalNames.ContainsKey(zone.Trim());
        }

        public void AddZone(string zone)
        {
            if (string.IsNullOrWhiteSpace(zone)) throw new ArgumentException("Zone name is required.", "zone");
            string name = zone.Trim();
            if (canonicalNames.ContainsKey(name)) return;
            canonicalNames[name] = name;
            adjacency[name] = new List<ZoneRoad>();
        }

        public void AddAlias(string alias, string zone)
        {
            string canonical = Require(zone);
            if (string.IsNullOrWhiteSpace(alias)) throw new ArgumentException("Alias is required.", "alias");
            aliases[alias.Trim()] = canonical;
        }

        public void AddRoad(string a, string b, double km)
        {
            string from = Require(a);
            string to = Require(b);
            if (Cmp.Equals(from, to)) throw new ArgumentException("A road must connect two different zones.");
            if (double.IsNaN(km) || double.IsInfinity(km) || km <= 0) throw new ArgumentException("Distance must be a positive number.", "km");
            if (adjacency[from].Any(r => Cmp.Equals(r.Other(from), to))) throw new InvalidOperationException("A road between these zones already exists.");

            ZoneRoad road = new ZoneRoad(from, to, km);
            adjacency[from].Add(road);
            adjacency[to].Add(road);
            roads.Add(road);
        }

        public void ReplaceNetwork(IEnumerable<string> zones, IEnumerable<ZoneRoad> restoredRoads)
        {
            canonicalNames.Clear();
            adjacency.Clear();
            aliases.Clear();
            roads.Clear();

            foreach (string zone in zones ?? Enumerable.Empty<string>()) AddZone(zone);
            foreach (ZoneRoad road in restoredRoads ?? Enumerable.Empty<ZoneRoad>()) AddRoad(road.From, road.To, road.Km);
        }

        /// <summary>Returns the zone named (or aliased) inside free text such as an address, or null.</summary>
        public string FindZone(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;

            IEnumerable<KeyValuePair<string, string>> candidates =
                canonicalNames.Values.Select(z => new KeyValuePair<string, string>(z, z))
                .Concat(aliases)
                .OrderByDescending(kv => kv.Key.Length); // "Pretoria CBD" beats "Pretoria"

            foreach (KeyValuePair<string, string> kv in candidates)
            {
                if (text.IndexOf(kv.Key, StringComparison.OrdinalIgnoreCase) >= 0) return kv.Value;
            }
            return null;
        }

        public List<string> Neighbours(string zone)
        {
            string z = Require(zone);
            return adjacency[z].Select(r => r.Other(z)).OrderBy(n => n, Cmp).ToList();
        }

        public bool IsConnected()
        {
            if (adjacency.Count == 0) return true;
            return BreadthFirst(Zones[0]).Count == adjacency.Count;
        }

        // ---------- traversals ----------

        /// <summary>Visits zones nearest-first (fewest road hops). Uses a queue.</summary>
        public List<string> BreadthFirst(string start)
        {
            return new List<string>(HopDistances(start).OrderBy(kv => kv.Value).Select(kv => kv.Key));
        }

        /// <summary>Hops from <paramref name="start"/> to every reachable zone (BFS).</summary>
        public Dictionary<string, int> HopDistances(string start)
        {
            string s = Require(start);
            Dictionary<string, int> hops = new Dictionary<string, int>(Cmp);
            Queue<string> queue = new Queue<string>();
            hops[s] = 0;
            queue.Enqueue(s);

            while (queue.Count > 0)
            {
                string zone = queue.Dequeue();
                foreach (string next in Neighbours(zone))
                {
                    if (hops.ContainsKey(next)) continue;
                    hops[next] = hops[zone] + 1;
                    queue.Enqueue(next);
                }
            }
            return hops;
        }

        /// <summary>Follows each road as deep as possible before backtracking. Uses a stack.</summary>
        public List<string> DepthFirst(string start)
        {
            string s = Require(start);
            List<string> order = new List<string>();
            HashSet<string> seen = new HashSet<string>(Cmp);
            Stack<string> stack = new Stack<string>();
            stack.Push(s);

            while (stack.Count > 0)
            {
                string zone = stack.Pop();
                if (!seen.Add(zone)) continue;
                order.Add(zone);

                List<string> next = Neighbours(zone);
                for (int i = next.Count - 1; i >= 0; i--) // reverse so alphabetical zones are popped first
                {
                    if (!seen.Contains(next[i])) stack.Push(next[i]);
                }
            }
            return order;
        }

        // ---------- minimum spanning tree ----------

        /// <summary>
        /// Kruskal: take roads shortest-first, skipping any that would form a cycle
        /// (checked with union-find). Returns a spanning forest if the graph is disconnected.
        /// </summary>
        public List<ZoneRoad> KruskalMst()
        {
            DisjointSet sets = new DisjointSet(canonicalNames.Values);
            List<ZoneRoad> tree = new List<ZoneRoad>();

            foreach (ZoneRoad road in roads.OrderBy(r => r.Km).ThenBy(r => r.From, Cmp).ThenBy(r => r.To, Cmp))
            {
                if (sets.Union(road.From, road.To)) tree.Add(road);
            }
            return tree;
        }

        /// <summary>
        /// Prim: grow one tree from <paramref name="start"/>, always adding the cheapest road
        /// that reaches a new zone (candidate roads are kept in a binary heap).
        /// Only covers the zones reachable from start.
        /// </summary>
        public List<ZoneRoad> PrimMst(string start)
        {
            string s = Require(start);
            HashSet<string> inTree = new HashSet<string>(Cmp) { s };
            BinaryHeap<ZoneRoad> frontier = new BinaryHeap<ZoneRoad>(Comparer<ZoneRoad>.Create((a, b) => a.Km.CompareTo(b.Km)));
            List<ZoneRoad> tree = new List<ZoneRoad>();

            foreach (ZoneRoad r in adjacency[s]) frontier.Enqueue(r);

            while (frontier.Count > 0)
            {
                ZoneRoad road = frontier.Dequeue();
                bool fromIn = inTree.Contains(road.From);
                bool toIn = inTree.Contains(road.To);
                if (fromIn && toIn) continue; // would form a cycle

                string added = fromIn ? road.To : road.From;
                inTree.Add(added);
                tree.Add(road);

                foreach (ZoneRoad r in adjacency[added])
                {
                    if (!inTree.Contains(r.Other(added))) frontier.Enqueue(r);
                }
            }
            return tree;
        }

        public static double TotalKm(IEnumerable<ZoneRoad> tree)
        {
            return tree.Sum(r => r.Km);
        }

        // ---------- helpers ----------

        private string Require(string zone)
        {
            if (string.IsNullOrWhiteSpace(zone)) throw new ArgumentException("A zone name is required.", "zone");
            string canonical;
            if (!canonicalNames.TryGetValue(zone.Trim(), out canonical))
                throw new ArgumentException("Unknown service zone: " + zone, "zone");
            return canonical;
        }

        private static ServiceZoneGraph CreateTshwaneNetwork()
        {
            ServiceZoneGraph g = new ServiceZoneGraph();
            foreach (string zone in new[] { "Pretoria CBD", "Sunnyside", "Hatfield", "Brooklyn", "Menlyn", "Mamelodi", "Centurion" })
                g.AddZone(zone);

            // Approximate road distances in km (illustrative values for routing demo purposes).
            g.AddRoad("Pretoria CBD", "Sunnyside", 3);
            g.AddRoad("Sunnyside", "Hatfield", 4);
            g.AddRoad("Pretoria CBD", "Hatfield", 7);
            g.AddRoad("Hatfield", "Brooklyn", 3);
            g.AddRoad("Brooklyn", "Menlyn", 4);
            g.AddRoad("Hatfield", "Menlyn", 6);
            g.AddRoad("Menlyn", "Mamelodi", 12);
            g.AddRoad("Pretoria CBD", "Mamelodi", 17);
            g.AddRoad("Pretoria CBD", "Centurion", 25);
            g.AddRoad("Brooklyn", "Centurion", 21);
            g.AddRoad("Menlyn", "Centurion", 22);

            g.AddAlias("Pretoria", "Pretoria CBD");
            g.AddAlias("CBD", "Pretoria CBD");
            g.AddAlias("Civic Centre", "Pretoria CBD");
            g.AddAlias("Lynnwood", "Menlyn");
            return g;
        }

        /// <summary>Union-find with path compression and union by rank.</summary>
        private class DisjointSet
        {
            private readonly Dictionary<string, string> parent = new Dictionary<string, string>(Cmp);
            private readonly Dictionary<string, int> rank = new Dictionary<string, int>(Cmp);

            public DisjointSet(IEnumerable<string> items)
            {
                foreach (string item in items) { parent[item] = item; rank[item] = 0; }
            }

            private string Find(string item)
            {
                string root = item;
                while (!Cmp.Equals(parent[root], root)) root = parent[root];
                while (!Cmp.Equals(parent[item], root)) { string next = parent[item]; parent[item] = root; item = next; }
                return root;
            }

            /// <summary>Joins the two sets. Returns false if they were already joined (cycle).</summary>
            public bool Union(string a, string b)
            {
                string ra = Find(a), rb = Find(b);
                if (Cmp.Equals(ra, rb)) return false;

                if (rank[ra] < rank[rb]) parent[ra] = rb;
                else if (rank[ra] > rank[rb]) parent[rb] = ra;
                else { parent[rb] = ra; rank[ra]++; }
                return true;
            }
        }
    }
}