using System;
using System.Collections;
using System.Collections.Generic;
using ExceptionsNS;
using static CustomCollections.Constants;

namespace CustomCollections
{
    public class List_<T> : IList<T>, IReadOnlyList<T>
    {
        private const int DEFAULT_SIZE = 4;

        private T[] _array;
        public int Capacity
        {
            get => _array.Length;
            set
            {
                if (value < Count) throw new ArgumentOutOfRangeException(nameof(Capacity), Exceptions.CAPACITY_LESS_THAN_SIZE);
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
                : throw new IndexOutOfRangeException(Exceptions.INDEX_OUTSIDE);

            set
            {
                if ((uint)index >= (uint)Count)
                    throw new IndexOutOfRangeException(Exceptions.INDEX_OUTSIDE);

                _array[index] = value;
            }
        }

        public List_() => _array = new T[DEFAULT_SIZE];

        public List_(int capacity)
        {
            if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity), Exceptions.CANNOT_BE_NEGATIVE);

            _array = capacity == 0 ? Array.Empty<T>() : new T[capacity];
        }

        public List_(IEnumerable<T> source)
        {
            if (source is null) throw new ArgumentNullException(nameof(source), Exceptions.CANNOT_BE_NULL);

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

        public void Insert(int index, T item)
        {
            if ((uint)index > (uint)Count) throw new IndexOutOfRangeException(Exceptions.INDEX_OUTSIDE);

            if (Count == _array.Length) ExpandTo(_array.Length * 2);
            if (index < Count) Array.Copy(_array, index, _array, index + 1, Count - index);
            _array[index] = item;
            Count++;
        }

        public void AddRange(IEnumerable<T> source)
        {
            if (source is null) throw new ArgumentNullException(nameof(source), Exceptions.CANNOT_BE_NULL);

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

        public void InsertRange(int index, ICollection<T> collection)
        {
            if (collection is null) throw new ArgumentNullException(nameof(collection), Exceptions.CANNOT_BE_NULL);
            if ((uint)index > (uint)Count) throw new IndexOutOfRangeException(Exceptions.INDEX_OUTSIDE);

            int countTotal = Count + collection.Count;
            if (countTotal < Capacity) ExpandTo(countTotal);
            //Remember elements that will be overwritten
            if (index < Count) Array.Copy(_array, index, _array, index + collection.Count, Count - index);
            if (collection == this)
            {
                // Copy first part of _array to insert location
                Array.Copy(_array, 0, _array, index, index);
                // Copy last part of _array back to inserted location
                Array.Copy(_array, index + collection.Count, _array, index * 2, Count - index);
            }
            else collection.CopyTo(_array, index);
            Count = countTotal;
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
            if (array is null) throw new ArgumentNullException(nameof(array), Exceptions.CANNOT_BE_NULL);
            if (array.Length < Count) throw new ArgumentOutOfRangeException(nameof(array), Exceptions.DEST_ARR_NOT_LONG_ENOUGH);
            if ((uint)arrayIndex > (uint)(array.Length - Count)) throw new ArgumentOutOfRangeException(nameof(arrayIndex), Exceptions.INDEX_OUTSIDE);

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
            if ((uint)index > (uint)Count) throw new IndexOutOfRangeException(Exceptions.INDEX_OUTSIDE);

            Count--;
            if (index < Count) Array.Copy(_array, index + 1, _array, index, Count - index);
            _array[Count] = default;
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

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}