using System;
using System.Collections.Generic;
using System.Text;

namespace MuncipalityServiceRequestApp
{
    /// <summary>
    /// Self-balancing binary search tree (AVL). Used to index service requests by
    /// reference number so lookups, inserts and removals are O(log n) even though
    /// reference numbers are issued in ascending order (which would turn a plain
    /// BST into a linked list with O(n) lookups).
    /// </summary>
    public class AvlTree<TKey, TValue> where TKey : IComparable<TKey>
    {
        private class Node
        {
            public TKey Key;
            public TValue Value;
            public Node Left;
            public Node Right;
            public int Height = 1;
            public Node(TKey key, TValue value) { Key = key; Value = value; }
        }

        private Node root;

        public int Count { get; private set; }

        public void Clear()
        {
            root = null;
            Count = 0;
            Rotations = 0;
        }
        public int Height { get { return HeightOf(root); } }
        /// <summary>Total rotations performed while keeping the tree balanced.</summary>
        public int Rotations { get; private set; }
        /// <summary>Key comparisons used by the most recent TryGetValue call.</summary>
        public int LastLookupComparisons { get; private set; }

        /// <summary>Adds a key. Returns false (and changes nothing) if the key already exists.</summary>
        public bool Insert(TKey key, TValue value)
        {
            if (key == null) throw new ArgumentNullException("key");
            bool added = false;
            root = Insert(root, key, value, ref added);
            if (added) Count++;
            return added;
        }

        public bool Remove(TKey key)
        {
            if (key == null) return false;
            bool removed = false;
            root = Remove(root, key, ref removed);
            if (removed) Count--;
            return removed;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            LastLookupComparisons = 0;
            value = default(TValue);
            if (key == null) return false;

            Node current = root;
            while (current != null)
            {
                LastLookupComparisons++;
                int c = key.CompareTo(current.Key);
                if (c == 0) { value = current.Value; return true; }
                current = c < 0 ? current.Left : current.Right;
            }
            return false;
        }

        public bool TryGetMin(out TKey key)
        {
            key = default(TKey);
            if (root == null) return false;
            Node n = root;
            while (n.Left != null) n = n.Left;
            key = n.Key;
            return true;
        }

        public bool TryGetMax(out TKey key)
        {
            key = default(TKey);
            if (root == null) return false;
            Node n = root;
            while (n.Right != null) n = n.Right;
            key = n.Key;
            return true;
        }

        /// <summary>In-order traversal: values in ascending key order.</summary>
        public IEnumerable<KeyValuePair<TKey, TValue>> InOrder()
        {
            Stack<Node> stack = new Stack<Node>();
            Node current = root;
            while (current != null || stack.Count > 0)
            {
                while (current != null) { stack.Push(current); current = current.Left; }
                current = stack.Pop();
                yield return new KeyValuePair<TKey, TValue>(current.Key, current.Value);
                current = current.Right;
            }
        }

        /// <summary>All entries with lo &lt;= key &lt;= hi, in ascending order (prunes whole subtrees).</summary>
        public IEnumerable<KeyValuePair<TKey, TValue>> Range(TKey lo, TKey hi)
        {
            return RangeNodes(root, lo, hi);
        }

        /// <summary>Text drawing of the tree, rotated 90 degrees: root on the left, larger keys on top.</summary>
        public string Describe(Func<TKey, string> label = null)
        {
            if (root == null) return "(empty tree)";
            StringBuilder sb = new StringBuilder();
            Describe(root, 0, sb, label ?? (k => k.ToString()));
            return sb.ToString();
        }

        // ---------- internals ----------

        private IEnumerable<KeyValuePair<TKey, TValue>> RangeNodes(Node n, TKey lo, TKey hi)
        {
            if (n == null) yield break;
            if (lo.CompareTo(n.Key) < 0)
                foreach (var kv in RangeNodes(n.Left, lo, hi)) yield return kv;
            if (lo.CompareTo(n.Key) <= 0 && hi.CompareTo(n.Key) >= 0)
                yield return new KeyValuePair<TKey, TValue>(n.Key, n.Value);
            if (hi.CompareTo(n.Key) > 0)
                foreach (var kv in RangeNodes(n.Right, lo, hi)) yield return kv;
        }

        private Node Insert(Node n, TKey key, TValue value, ref bool added)
        {
            if (n == null) { added = true; return new Node(key, value); }

            int c = key.CompareTo(n.Key);
            if (c < 0) n.Left = Insert(n.Left, key, value, ref added);
            else if (c > 0) n.Right = Insert(n.Right, key, value, ref added);
            else return n; // duplicate key: ignore

            return Rebalance(n);
        }

        private Node Remove(Node n, TKey key, ref bool removed)
        {
            if (n == null) return null;

            int c = key.CompareTo(n.Key);
            if (c < 0) n.Left = Remove(n.Left, key, ref removed);
            else if (c > 0) n.Right = Remove(n.Right, key, ref removed);
            else
            {
                removed = true;
                if (n.Left == null) return n.Right;
                if (n.Right == null) return n.Left;

                // Two children: copy the in-order successor here, then delete it.
                Node successor = n.Right;
                while (successor.Left != null) successor = successor.Left;
                n.Key = successor.Key;
                n.Value = successor.Value;
                bool ignored = false;
                n.Right = Remove(n.Right, successor.Key, ref ignored);
            }
            return Rebalance(n);
        }

        private Node Rebalance(Node n)
        {
            UpdateHeight(n);
            int balance = BalanceOf(n);

            if (balance > 1)
            {
                if (BalanceOf(n.Left) < 0) n.Left = RotateLeft(n.Left);   // left-right case
                return RotateRight(n);
            }
            if (balance < -1)
            {
                if (BalanceOf(n.Right) > 0) n.Right = RotateRight(n.Right); // right-left case
                return RotateLeft(n);
            }
            return n;
        }

        private Node RotateRight(Node y)
        {
            Node x = y.Left;
            y.Left = x.Right;
            x.Right = y;
            UpdateHeight(y);
            UpdateHeight(x);
            Rotations++;
            return x;
        }

        private Node RotateLeft(Node x)
        {
            Node y = x.Right;
            x.Right = y.Left;
            y.Left = x;
            UpdateHeight(x);
            UpdateHeight(y);
            Rotations++;
            return y;
        }

        private static int HeightOf(Node n) { return n == null ? 0 : n.Height; }
        private static int BalanceOf(Node n) { return n == null ? 0 : HeightOf(n.Left) - HeightOf(n.Right); }
        private static void UpdateHeight(Node n) { n.Height = 1 + Math.Max(HeightOf(n.Left), HeightOf(n.Right)); }

        private static void Describe(Node n, int depth, StringBuilder sb, Func<TKey, string> label)
        {
            if (n == null) return;
            Describe(n.Right, depth + 1, sb, label);
            sb.Append(' ', depth * 6)
              .Append(label(n.Key))
              .Append("  [h=").Append(n.Height)
              .Append(", balance=").Append(BalanceOf(n)).Append(']')
              .AppendLine();
            Describe(n.Left, depth + 1, sb, label);
        }
    }
}