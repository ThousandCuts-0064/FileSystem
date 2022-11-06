using System;
using System.Collections.Generic;
using ExceptionsNS;
using Custom = CustomCollections;

namespace CustomQuery
{
    public static class EnumerableExt
    {
        public static IEnumerable<T> CopyTo<T>(this IEnumerable<T> source, T[] array, int index, Action counter)
        {
            if (source is null) throw new ArgumentNullException(nameof(source), Exceptions.CannotBeNull);
            if (array is null) throw new ArgumentNullException(nameof(array), Exceptions.CannotBeNull);
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index), Exceptions.CannotBeNegative);

            int offset = 0;
            if (counter is null)
                foreach (var item in source)
                {
                    int curr = index + offset;
                    array[curr] = item;
                    offset++;
                    if (curr == array.Length) yield return item;
                }
            else
                foreach (var item in source)
                {
                    int curr = index + offset;
                    array[curr] = item;
                    offset++;
                    counter();
                    if (curr == array.Length) yield return item;
                }
        }

        public static void CopyTo<T>(this IReadOnlyCollection<T> roCollection, T[] array, int index)
        {
            if (roCollection is null) throw new ArgumentNullException(nameof(roCollection), Exceptions.CannotBeNull);
            if (array is null) throw new ArgumentNullException(nameof(array), Exceptions.CannotBeNull);
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index), Exceptions.CannotBeNegative);
            if (roCollection.Count > array.Length - index) throw new ArgumentException(Exceptions.DestArrNotLongEnough);

            int offset = 0;
            foreach (var item in roCollection)
            {
                array[index + offset] = item;
                offset++;
            }
        }

        public static T[] ToArray<T>(this IEnumerable<T> source) =>
            source is null
            ? throw new ArgumentNullException(nameof(source), Exceptions.CannotBeNull)
            : new Buffer<T>(source).ToArray();

        public static Custom.List<T> ToList<T>(this IEnumerable<T> source) =>
            new Custom.List<T>(source); //List constructor checks for null

        private readonly ref struct Buffer<T>
        {
            private readonly T[] _items;
            private readonly int _count;

            //source is null ? should be checked beforehand
            public Buffer(IEnumerable<T> source)
            {
                if (source is ICollection<T> collection)
                {
                    _count = collection.Count;
                    _items = new T[_count];
                    collection.CopyTo(_items, 0);
                    return;
                }

                _items = new T[4];
                _count = 0;
                foreach (var item in source)
                {
                    if (_count == _items.Length)
                    {
                        if (_count == int.MaxValue) throw new OverflowException(Exceptions.ArrCapacityExceeded);

                        _count *= 2;
                        T[] newItems = new T[_count < 0 ? int.MaxValue : _count];
                        _items.CopyTo(newItems, 0);
                        _items = newItems;
                    }
                    _items[_count] = item;
                    _count++;
                }
            }

            public T[] ToArray()
            {
                if (_count == 0) return Array.Empty<T>();
                if (_items.Length == _count) return _items;

                T[] result = new T[_count];
                _items.CopyTo(result, 0);
                return result;
            }
        }
    }
}
