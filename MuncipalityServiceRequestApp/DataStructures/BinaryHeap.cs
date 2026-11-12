using System;
using System.Collections.Generic;

namespace MuncipalityServiceRequestApp
{
    /// <summary>
    /// Array-backed binary min-heap (priority queue): the element that compares
    /// "smallest" is always at the root, so Peek is O(1) and Enqueue/Dequeue are
    /// O(log n). Elements that compare equal come out in the order they went in.
    /// .NET Framework has no built-in PriorityQueue, so this is our own.
    /// </summary>
    public class BinaryHeap<T>
    {
        private struct Entry
        {
            public T Item;
            public long Order;
        }

        private readonly List<Entry> data = new List<Entry>();
        private readonly IComparer<T> comparer;
        private long counter;

        public BinaryHeap() : this(null) { }

        public BinaryHeap(IComparer<T> comparer)
        {
            this.comparer = comparer ?? Comparer<T>.Default;
        }

        public int Count { get { return data.Count; } }

        public T Peek()
        {
            if (data.Count == 0) throw new InvalidOperationException("The heap is empty.");
            return data[0].Item;
        }

        public bool TryPeek(out T item)
        {
            item = default(T);
            if (data.Count == 0) return false;
            item = data[0].Item;
            return true;
        }

        public void Enqueue(T item)
        {
            data.Add(new Entry { Item = item, Order = counter++ });
            SiftUp(data.Count - 1);
        }

        public T Dequeue()
        {
            if (data.Count == 0) throw new InvalidOperationException("The heap is empty.");

            T top = data[0].Item;
            int last = data.Count - 1;
            data[0] = data[last];
            data.RemoveAt(last);
            if (data.Count > 0) SiftDown(0);
            return top;
        }

        private bool Less(Entry a, Entry b)
        {
            int c = comparer.Compare(a.Item, b.Item);
            return c != 0 ? c < 0 : a.Order < b.Order;
        }

        private void SiftUp(int index)
        {
            while (index > 0)
            {
                int parent = (index - 1) / 2;
                if (!Less(data[index], data[parent])) break;
                Swap(index, parent);
                index = parent;
            }
        }

        private void SiftDown(int index)
        {
            int count = data.Count;
            while (true)
            {
                int left = 2 * index + 1;
                int right = left + 1;
                int smallest = index;

                if (left < count && Less(data[left], data[smallest])) smallest = left;
                if (right < count && Less(data[right], data[smallest])) smallest = right;
                if (smallest == index) break;

                Swap(index, smallest);
                index = smallest;
            }
        }

        private void Swap(int a, int b)
        {
            Entry temp = data[a];
            data[a] = data[b];
            data[b] = temp;
        }
    }
}