using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using ExceptionsNS;
using static CustomCollections.Constants;

namespace CustomCollections
{
    [DebuggerDisplay(COLLECTION_DISPLAY)]
    public class Queue_<T> : ICollection<T>, IReadOnlyCollection<T>
    {
        private T[] _array;
        private int _head;
        private int _tail;

        public int Count { get; private set; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool ICollection<T>.IsReadOnly => false;

        public Queue_() => _array = Array.Empty<T>();

        public Queue_(int capacity)
        {
            if (capacity < 0) throw new NumberNegativeException(nameof(capacity));

            _array = capacity == 0 ? Array.Empty<T>() : new T[capacity];
        }

        public Queue_(IEnumerable<T> source)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));

            if (source is ICollection<T> collection)
            {
                _array = new T[collection.Count];
                collection.CopyTo(_array, 0);
                Count = collection.Count;
                _tail = Count - 1;
                return;
            }

            foreach (var item in source)
                Enque(item);
        }

        public void Enque(T item)
        {
            if (Count == _array.Length)
            {
                int newLength = _array.Length * 2;
                if (newLength == 0) newLength = DEFAULT_SIZE;
                //Check for overflow
                T[] arr = new T[((uint)newLength > ARRAY_MAX_LENGTH) ? ARRAY_MAX_LENGTH : newLength];

                if (_tail > _head)
                {
                    Array.Copy(_array, _head, arr, 0, Count);
                    _head = 0;
                    _tail = Count - 1;
                }
                else
                {
                    Array.Copy(_array, _head, arr, 0, Count - _head);
                    Array.Copy(_array, 0, arr, _tail, _tail + 1);
                }

                _array = arr;
            }

            Count++;
            _tail = (_tail + 1) % _array.Length;
            _array[_tail] = item;
        }

        public T Peek() => _array[_tail];

        public bool Contains(T item) => _array.Contains_(item, 0, Count);

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array is null) throw new ArgumentNullException(nameof(array));
            if (array.Length < Count) throw new ArrayTooShortExcpetion(nameof(array));
            if ((uint)arrayIndex > (uint)(array.Length - Count)) throw new IndexOutOfBoundsException(nameof(arrayIndex));

            if (_tail > _head)
                Array.Copy(_array, _head, array, arrayIndex, Count);
            else
            {
                Array.Copy(_array, _head, array, arrayIndex, Count - _head);
                Array.Copy(_array, 0, array, arrayIndex + Count - _head, _tail + 1);
            }
        }

        public T Deque()
        {
            if (Count == 0) throw new CollectionEmptyException();

            Count--;
            try { return _array[_tail]; }
            finally
            {
                _array[_tail] = default;
                _tail = (_array.Length + _tail - 1) % _array.Length;
            }
        }

        public void Clear()
        {
            if (_tail > _head)
                Array.Clear(_array, _head, Count);
            else
            {
                Array.Clear(_array, _head, Count - _head);
                Array.Clear(_array, 0, _tail + 1);
            }

            Count = 0;
            _head = 0;
            _tail = 0;
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (_tail > _head)
                for (int i = _head; i < Count; i++)
                    yield return _array[i];
            else
            {
                for (int i = _head; i < _array.Length; i++)
                    yield return _array[i];

                for (int i = 0; i <= _tail; i++)
                    yield return _array[i];
            }
        }

        void ICollection<T>.Add(T item) => Enque(item);
        bool ICollection<T>.Remove(T item) => throw new NotSupportedException();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
