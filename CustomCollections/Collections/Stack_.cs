using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using ExceptionsNS;
using static CustomCollections.Constants;

namespace CustomCollections
{
    [DebuggerDisplay(COLLECTION_DISPLAY)]
    public class Stack_<T> : ICollection<T>, IReadOnlyCollection<T>
    {
        private T[] _array;

        public int Count { get; private set; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool ICollection<T>.IsReadOnly => false;

        public Stack_() => _array = Array.Empty<T>();

        public Stack_(int capacity)
        {
            if (capacity < 0) throw new NumberNegativeException(nameof(capacity));

            _array = capacity == 0 ? Array.Empty<T>() : new T[capacity];
        }

        public Stack_(IEnumerable<T> source)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));

            if (source is ICollection<T> collection)
            {
                collection.CopyTo(_array, 0);
                Count = collection.Count;
                return;
            }

            foreach (var item in source)
                Push(item);
        }

        public void Push(T item)
        {
            int newLength = _array.Length * 2;
            if (newLength == 0) newLength = DEFAULT_SIZE;
            //Check for overflow
            T[] arr = new T[((uint)newLength > ARRAY_MAX_LENGTH) ? ARRAY_MAX_LENGTH : newLength];
            Array.Copy(_array, arr, Count);
            _array = arr;
            _array[Count++] = item;
        }

        public T Peek() => _array[Count - 1];

        public bool Contains(T item) => _array.Contains_(item, 0, Count);

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array is null) throw new ArgumentNullException(nameof(array));
            if (array.Length < Count) throw new ArrayTooShortExcpetion(nameof(array));
            if ((uint)arrayIndex > (uint)(array.Length - Count)) throw new IndexOutOfBoundsException(nameof(arrayIndex));

            Array.Copy(_array, 0, array, arrayIndex, Count);
        }

        public T Pop()
        {
            try { return _array[--Count]; }
            finally { _array[Count] = default; }
        }

        public void Clear()
        {
            Array.Clear(_array, 0, Count);
            Count = 0;
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
                yield return _array[i];
        }

        void ICollection<T>.Add(T item) => Push(item);
        bool ICollection<T>.Remove(T item) => throw new NotSupportedException();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
