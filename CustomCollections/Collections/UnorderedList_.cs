using System;
using System.Collections;
using System.Collections.Generic;
using ExceptionsNS;
using static CustomCollections.Constants;

namespace CustomCollections
{
    public class UnorderedList_<T> : IList<T>, IReadOnlyList<T>
    {
        private T[] _array;

        public int Capacity
        {
            get => _array.Length;
            set
            {
                if (value < Count) throw new ArgumentOutOfRangeException(nameof(Capacity), $"{nameof(Capacity)} cannot be less than {nameof(Count)}.");
                if (value == Count) return;

                T[] newArray = new T[value];
                _array.CopyTo(newArray, 0);
                _array = newArray;
            }
        }
        public int Count { get; private set; }

        bool ICollection<T>.IsReadOnly => false;

        public T this[int index]
        {
            get => (uint)index < (uint)Count // negatives will be cast to more than int.Max
                ? _array[index]
                : throw new IndexOutOfBoundsException(nameof(index));

            set
            {
                if ((uint)index >= (uint)Count)
                    throw new IndexOutOfBoundsException(nameof(index));

                _array[index] = value;
            }
        }

        public UnorderedList_() => _array = new T[DEFAULT_SIZE];

        public UnorderedList_(int capacity)
        {
            if (capacity < 0) throw new NumberNegativeException(nameof(capacity));

            _array = capacity == 0 ? Array.Empty<T>() : new T[capacity];
        }

        public UnorderedList_(IEnumerable<T> source)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));

            if (source is ICollection<T> collection)
                collection.CopyTo(_array, 0);
            else
                foreach (var item in source)
                    Add(item);
        }

        public void Add(T item)
        {
            if (Count == _array.Length) ExpandTo(_array.Length * 2);
            _array[Count++] = item;
        }

        public void AddRange(IEnumerable<T> source)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));

            if (source is ICollection<T> collection)
            {
                int countTotal = Count + collection.Count;
                if (countTotal < Capacity) ExpandTo(countTotal);
                if (collection == this) Array.Copy(_array, 0, _array, Count, Count);
                else collection.CopyTo(_array, Count);
            }
            else
                foreach (var item in source)
                    Add(item);
        }

        public bool Contains(T item)
        {
            if (item == null)
            {
                for (int i = 0; i < Count; i++)
                    if (_array[i] == null) return true;
                return false;
            }

            var comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < Count; i++)
                if (comparer.Equals(_array[i], item)) return true;
            return false;
        }

        public int IndexOf(T item)
        {
            if (item == null)
            {
                for (int i = 0; i < Count; i++)
                    if (_array[i] == null) return i;
                return -1;
            }

            var comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < Count; i++)
                if (comparer.Equals(_array[i], item)) return i;
            return -1;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array is null) throw new ArgumentNullException(nameof(array));
            if (array.Length < Count) throw new ArrayTooShortExcpetion(nameof(array));
            if ((uint)arrayIndex > (uint)(array.Length - Count)) throw new IndexOutOfBoundsException(nameof(arrayIndex));

            _array.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            int index = IndexOf(item);
            if (index < 0) return false;

            RemoveAt(index);
            return true;
        }

        public void RemoveAt(int index)
        {
            if ((uint)index > (uint)Count) throw new IndexOutOfBoundsException(nameof(index));

            Count--;
            _array[index] = _array[Count]; // Fill removed element with the last one
            _array[Count] = default; // Set the previously last element to default
        }

        public void Clear()
        {
            if (Count == 0) return;
            Array.Clear(_array, 0, Count);
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
                yield return _array[i];
        }

        private void ExpandTo(int newCapacity)
        {
            if (newCapacity == 0) newCapacity = DEFAULT_SIZE;
            //Check for overflow
            if ((uint)newCapacity > ARRAY_MAX_LENGTH) newCapacity = ARRAY_MAX_LENGTH;
            Capacity = newCapacity;
        }


        void IList<T>.Insert(int index, T item)
        {
            if ((uint)index > (uint)Count) throw new IndexOutOfBoundsException(nameof(index));

            if (Count == _array.Length) ExpandTo(_array.Length * 2);
            if (index < Count) Array.Copy(_array, index, _array, index + 1, Count - index);
            _array[index] = item;
            Count++;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
