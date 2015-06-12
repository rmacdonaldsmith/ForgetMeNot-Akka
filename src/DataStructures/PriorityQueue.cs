using System;
using System.Collections;
using System.Collections.Generic;

namespace ForgetMeNot.DataStructures
{
    /// <summary>
    /// Basically taken from: http://algs4.cs.princeton.edu/24pq/
    /// Priority queue implemented with a heap.
    /// The heap is always maintained in order; the maximum element is always on top.
	/// Therefore, the Min() operation is completed in constant time (it just peaks the item on top).
	/// RemoveMin() requires that the heap be resorted, so takes N log N time (the top item is 
	/// removed and replaced by the item on the bottom. Heap order is then restored by sinking the new item on top).
    /// Inserts are done in log N time; the item is added to the end of the queue.
    /// Heap order is then maintained by swimming the latest element up the heap.
    /// Construction of the heap is proportional to the number of elements to add.
    /// Array will double / halve in size as needed; this is fine as we amortize this
    /// for inserts / deletions.
	/// This data structure should be good enough to get us started...we can optimze later!
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PriorityQueue<T> : IEnumerable<T>
    {
        private readonly IComparer<T> _comparer;
        private readonly Func<T, T, bool> _comparerFunc; 
		private readonly bool _useFuncComparer = false;
        private int _count = 0;
        private T[] _heap;

        public PriorityQueue(int size)
        {
            if(size < 1)
                throw new ArgumentOutOfRangeException("size", "size must be greater than 0");

            _heap = new T[size];
        }

        public PriorityQueue(int size, IComparer<T> comparer)
        {
            if (comparer == null) throw new ArgumentNullException("comparer");
            if (size < 1) throw new ArgumentOutOfRangeException("size", "size must be greater than 0");

            _comparer = comparer;
            _heap = new T[size];
        }

        public PriorityQueue(IComparer<T> comparer) :
            this(1, comparer)
        {}

        public PriorityQueue(int size, Func<T, T, bool> comparer)
        {
            if (comparer == null) throw new ArgumentNullException("comparer");
            if (size < 1) throw new ArgumentOutOfRangeException("size", "size must be greater than 0");

            _comparerFunc = comparer;
            _heap = new T[size];
			_useFuncComparer = true;
        }

        public PriorityQueue(Func<T,T,bool> comparer) :
            this(1, comparer)
        {}

        /// <summary>
        /// Construct a priority queue from a collection of items
        /// </summary>
        /// <param name="items"></param>
        /// <param name="comparer"></param>
        public PriorityQueue(IEnumerable<T> items, IComparer<T> comparer)
        {
            
        }

        public bool IsEmpty
        {
            get { return _count == 0; }
        }

        public int Size
        {
            get { return _count; }
        }

        /// <summary>
        /// Returns the largest item in the queue, but does not delete that item
        /// </summary>
        /// <returns></returns>
        public T Max()
        {
            if(IsEmpty)
                throw new InvalidOperationException("The priority queue is empty.");

            return _heap[1];
        }

        public T RemoveMax()
        {
            if (IsEmpty)
                throw new InvalidOperationException("The priority queue is empty.");

            var max = _heap[1];
            Exchange(1, _count--);
            Sink(1);
            _heap[_count+1] = default(T);     // to avoid loiterig and help with garbage collection

            if ((_count > 0) && (_count == (_heap.Length - 1) / 4)) 
                Resize(_heap.Length / 2);
            
            if (!IsMaxHeap())
                throw new InvalidOperationException("RemoveMax caused a violation of the heap invariant.");

            return max;
        }

        public void Insert(T item)
        {
            // double size of array if necessary
            if (_count >= _heap.Length - 1) 
                Resize(2*_heap.Length);

            // add x, and percolate it up to maintain heap invariant
            _heap[++_count] = item;
            Swim(_count);
            if(!IsMaxHeap())
                throw new InvalidOperationException("Insert caused a violation of the heap invariant.");

            dump();
        }

        private void dump()
        {
            Console.WriteLine("Heap Contents:");
            foreach (var item in _heap)
            {
                Console.Write(item);
                Console.Write(",");
            }
            Console.WriteLine();
        }

        private void Sink(int k)
        {
            while (2 * k <= _count)
            {
                var j = 2 * k;
                if (j < _count && Less(j, j + 1)) j++;
                if (!Less(k, j)) break;
                Exchange(k, j);
                k = j;
            }
        }

        private void Swim(int k)
        {
            while (k > 1 && Less(k / 2, k))
            {
                Exchange(k, k / 2);
                k = k / 2;
            }
        }

        private void Resize(int capacity)
        {
            if (capacity <= _count)
                throw new ArgumentOutOfRangeException("capacity", "New capacity must be greater than the current count of elements.");
            var temp = new T[capacity];
            for (var i = 1; i <= _count; i++) 
                temp[i] = _heap[i];
            _heap = temp;
            Console.WriteLine("Resized to " + capacity);
        }

        private bool Less(int i, int j)
        {
			if(_useFuncComparer) 
				return _comparerFunc(_heap[i], _heap[j]);

            if (_comparer == null)
                return ((IComparable<T>) _heap[i]).CompareTo(_heap[j]) < 0;
            return _comparer.Compare(_heap[i], _heap[j]) < 0;
        }

        private void Exchange(int i, int j)
        {
            var swap = _heap[i];
            _heap[i] = _heap[j];
            _heap[j] = swap;
        }

        // is pq[1..N] a max heap?
        private bool IsMaxHeap()
        {
            return IsMaxHeap(1);
        }

        // is subtree of pq[1..N] rooted at k a max heap?
        private bool IsMaxHeap(int k)
        {
            if (k > _count) return true;
            int left = 2*k;
            int right = 2*k + 1;
            if (left <= _count && Less(k, left)) return false;
            if (right <= _count && Less(k, right)) return false;
            return IsMaxHeap(left) && IsMaxHeap(right);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new PqEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private class PqEnumerator : IEnumerator<T>
        {
            private readonly PriorityQueue<T> _copy; 

            public PqEnumerator(PriorityQueue<T> queue)
            {
                _copy = new PriorityQueue<T>(queue.Size);
                //start at 1 because we always keep the 0th element empty
                for (var i = 1; i <= queue.Size; i++)
                {
                    _copy.Insert(queue._heap[i]);
                }
            }

            public void Dispose()
            {
                //todo
            }

            public bool MoveNext()
            {
                return !_copy.IsEmpty;
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }

            public T Current {
                get { return _copy.RemoveMax(); }
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }
        }
    }
}
