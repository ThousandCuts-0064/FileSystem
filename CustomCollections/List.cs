using System;
using System.Collections;
using System.Collections.Generic;
using ExceptionsNS;

namespace CustomCollections
{
    public class List<T> : IList<T>, IReadOnlyList<T>
    {
        private const int DEFAULT_SIZE = 4;

        private T[] _array;
        public int Capacity
        {
            get => _array.Length;
            set
            {
                if (value < Count) throw new ArgumentOutOfRangeException(nameof(Capacity), Exceptions.CapacityLessThanSize);
                if (value == Count) return;

                T[] newArray = new T[value];
                _array.CopyTo(newArray, 0);
                _array = newArray;
            }
        }
        public int Count { get; private set; }
        public bool IsReadOnly => false;

        public T this[int index] { get => _array[index]; set => _array[index] = value; }

        public List() => _array = Array.Empty<T>();

        public List(int capacity)
        {
            if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity), Exceptions.CannotBeNegative);

            _array = capacity == 0 ? Array.Empty<T>() : new T[capacity];
        }

        public List(IEnumerable<T> source)
        {
            if (source is null) throw new ArgumentNullException(nameof(source), Exceptions.CannotBeNull);

            if (source is ICollection<T> collection)
                collection.CopyTo(_array, 0);
            else
                foreach (var item in source)
                    Add(item);
        }

        public void Add(T item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            if (Count == 0) return;
            Array.Clear(_array, 0, Count);
        }

        public bool Contains(T item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public int IndexOf(T item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, T item)
        {
            throw new NotImplementedException();
        }

        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
