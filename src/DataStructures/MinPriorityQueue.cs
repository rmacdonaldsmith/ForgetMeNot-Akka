using System;
using System.Collections;
using System.Collections.Generic;
using ForgetMeNot.Common;
using log4net;

namespace ForgetMeNot.DataStructures
{
	public class MinPriorityQueue<T> : IEnumerable<T>
	{
	    private static readonly ILog Logger = LogManager.GetLogger(typeof (MinPriorityQueue<T>));
		protected readonly Func<T, T, bool> _comparer;
		private int N = 0;
		private T[] _pq;

		public MinPriorityQueue(int size)
		{
			Ensure.Positive(size, "size");

			_pq = new T[size];
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ReminderService.DataStructures.MinPriorityQueue`1"/> class.
		/// </summary>
		/// <param name="size">The initial size of the priority queue.</param>
		/// <param name="greaterThanComparer">Greater than comparer: returns true if T1 is greater than T2, else false</param>
		public MinPriorityQueue(int size, Func<T, T, bool> greaterThanComparer) :
			this(size)
		{
			Ensure.NotNull (greaterThanComparer, "greaterThanComparer");

			_comparer = greaterThanComparer;
		}
			
		public MinPriorityQueue(Func<T, T, bool> comparer) : 
			this(1, comparer)
		{
			//empty
		}

		/// <summary>
		/// Initializes a priority queue from the array of keys.
		/// Takes time proportional to the number of keys, using sink-based heap construction.
		/// </summary>
		public MinPriorityQueue(T[] keys, Func<T, T, bool> comparer) 
		{
			_comparer = comparer;
			N = keys.Length;
			_pq = new T[keys.Length + 1];
			for (int i = 0; i < N; i++)
				_pq[i+1] = keys[i];
			for (int k = N/2; k >= 1; k--)
				Sink(k);
			//assert IsMinHeap();
		}

		public bool IsEmpty
		{
			get { return N == 0; }
		}

		public int Size
		{
			get { return N; }
		}
			
		//Returns a smallest key on the priority queue.
		public T Min() 
		{
			if(IsEmpty)
				throw new InvalidOperationException("The priority queue is empty.");

			return _pq[1];
		}

		// helper function to double the size of the heap array
		private void Resize(int capacity) 
		{
			if (capacity <= N)
				throw new ArgumentOutOfRangeException("capacity", "New capacity must be greater than the current count of elements.");

			var temp = new T[capacity];
			Array.Copy(_pq, temp, N + 1);
			_pq = temp;
		}
			
		//Adds a new key to the priority queue.
		public void Insert(T x) 
		{
			// double size of array if necessary
		    if (N == _pq.Length - 1)
		    {
		        var newCapacity = 2*_pq.Length;
		        Resize(newCapacity);
		        Logger.InfoFormat(new {newarraycapacity = newCapacity, oldarraycapacity = N, logname = "metrics"},
		                          "Priority queue array capacity increased");
		    }

		    // add x, and percolate it up to maintain heap invariant
			_pq[++N] = x;
			Swim(N);
		}
			
		//Removes and returns a smallest key on the priority queue.
		public T RemoveMin() 
		{
			if (IsEmpty)
				throw new InvalidOperationException("The priority queue is empty.");

			Exchange(1, N);
			T min = _pq[N--];
			Sink(1);
			_pq[N+1] = default(T);         // avoid loitering and help with garbage collection
			if ((N > 0) && (N == (_pq.Length - 1) / 4)) 
            {
                var newCapacity = _pq.Length / 2;
                Resize(newCapacity);
                Logger.InfoFormat(new { newarraycapacity = newCapacity, oldarraycapacity = N, logname = "metrics" },
                                  "Priority queue array capacity reduced");
            }

			return min;
		}


	/***********************************************************************
    * Helper functions to restore the heap invariant.
    **********************************************************************/

		private void Swim(int k) 
		{
			while (k > 1 && Greater(k/2, k)) {
				Exchange(k, k/2);
				k = k/2;
			}
		}

		private void Sink(int k) 
		{
			while (2*k <= N) {
				int j = 2*k;
				if (j < N && Greater(j, j+1)) j++;
				if (!Greater(k, j)) break;
				Exchange(k, j);
				k = j;
			}
		}

	/***********************************************************************
    * Helper functions for compares and swaps.
    **********************************************************************/
		private bool Greater(int i, int j) 
		{
			return _comparer (_pq[i], _pq[j]);
		}

		private void Exchange(int i, int j) 
		{
			T swap = _pq[i];
			_pq[i] = _pq[j];
			_pq[j] = swap;
		}

		// is pq[1..N] a min heap?
		private bool IsMinHeap() 
		{
			return IsMinHeap(1);
		}

		// is subtree of pq[1..N] rooted at k a min heap?
		private bool IsMinHeap(int k) 
		{
			if (k > N) return true;
			int left = 2*k, right = 2*k + 1;
			if (left  <= N && Greater(k, left))  return false;
			if (right <= N && Greater(k, right)) return false;
			return IsMinHeap(left) && IsMinHeap(right);
		}


	/***********************************************************************
    * Iterators
    **********************************************************************/

		/**
		* Returns an iterator that iterates over the keys on the priority queue
		* in ascending order.
		* The iterator doesn't implement <tt>remove()</tt> since it's optional.
		*/
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
			private readonly MinPriorityQueue<T> _copy; 

			public PqEnumerator(MinPriorityQueue<T> queue)
			{
				_copy = new MinPriorityQueue<T>(queue.Size, queue._comparer);
				//start at 1 because we always keep the 0th element empty
				for (var i = 1; i <= queue.Size; i++)
				{
					_copy.Insert(queue._pq[i]);
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
				get { return _copy.RemoveMin(); }
			}

			object IEnumerator.Current
			{
				get { return Current; }
			}
		}
	}
}

