using System;
using System.Collections.Generic;
using ExceptionsNS;
using CustomCollections;
using static CustomCollections.Constants;

namespace CustomQuery
{
    public static class EnumerableExt
    {
        public static void CopyTo<T>(this IReadOnlyCollection<T> roCollection, T[] array, int index)
        {
            if (roCollection is null) throw new ArgumentNullException(nameof(roCollection), Exceptions.CANNOT_BE_NULL);
            if (array is null) throw new ArgumentNullException(nameof(array), Exceptions.CANNOT_BE_NULL);
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index), Exceptions.CANNOT_BE_NEGATIVE);
            if (roCollection.Count > array.Length - index) throw new ArgumentException(Exceptions.DEST_ARR_NOT_LONG_ENOUGH);

            int offset = index;
            foreach (var item in roCollection)
            {
                array[offset] = item;
                offset++;
            }
        }

        public static T[] ToArray<T>(this IEnumerable<T> source) =>
            source is null
            ? throw new ArgumentNullException(nameof(source), Exceptions.CANNOT_BE_NULL)
            : new Buffer<T>(source).ToArray();

        public static T[] ToArrayFixed<T>(this IEnumerable<T> source, int arrayLength)
        {
            if (source is null) throw new ArgumentNullException(nameof(source), Exceptions.CANNOT_BE_NULL);

            int index = 0;
            T[] arr = new T[arrayLength];
            foreach (var item in source)
                arr[index++] = item;
            return arr;
        }

        public static List_<T> ToList<T>(this IEnumerable<T> source) =>
            source is null
            ? throw new ArgumentNullException(nameof(source), Exceptions.CANNOT_BE_NULL)
            : new List_<T>(source);

        public static List_<T> ToListFixed<T>(this IEnumerable<T> source, int listCapacity)
        {
            if (source is null) throw new ArgumentNullException(nameof(source), Exceptions.CANNOT_BE_NULL);

            int index = 0;
            List_<T> list = new List_<T>(listCapacity);
            foreach (var item in source)
                list[index++] = item;
            return list;
        }

        public static IEnumerable<TResult> Select<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            if (source is null) throw new ArgumentNullException(nameof(source), Exceptions.CANNOT_BE_NULL);
            if (selector is null) throw new ArgumentNullException(nameof(selector), Exceptions.CANNOT_BE_NULL);

            foreach (var item in source)
                yield return selector(item);
        }

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
                        if (_count == ARRAY_MAX_LENGTH) throw new OverflowException(Exceptions.ARR_MAX_CAPACITY_EXCEEDED);

                        _count *= 2;
                        if ((uint)_count > ARRAY_MAX_LENGTH) _count = ARRAY_MAX_LENGTH;
                        T[] newItems = new T[_count];
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
