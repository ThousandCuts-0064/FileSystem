using System;
using System.Collections.Generic;
using ExceptionsNS;
using CustomCollections;
using static CustomCollections.Constants;

namespace CustomQuery
{
    public static class EnumerableExt
    {
        public static T[] ToArray_<T>(this IEnumerable<T> source) =>
            source is null
            ? throw new ArgumentNullException(nameof(source), Exceptions.CANNOT_BE_NULL)
            : new Buffer<T>(source).ToArray();

        public static T[] ToArrayFixed_<T>(this IEnumerable<T> source, int arrayLength)
        {
            if (source is null) throw new ArgumentNullException(nameof(source), Exceptions.CANNOT_BE_NULL);

            int index = 0;
            T[] arr = new T[arrayLength];
            foreach (var item in source)
                arr[index++] = item;
            return arr;
        }

        public static List_<T> ToList_<T>(this IEnumerable<T> source) =>
            source is null
            ? throw new ArgumentNullException(nameof(source), Exceptions.CANNOT_BE_NULL)
            : new List_<T>(source);

        public static List_<T> ToListFixed_<T>(this IEnumerable<T> source, int listCapacity)
        {
            if (source is null) throw new ArgumentNullException(nameof(source), Exceptions.CANNOT_BE_NULL);

            int index = 0;
            List_<T> list = new List_<T>(listCapacity);
            foreach (var item in source)
                list[index++] = item;
            return list;
        }

        public static IEnumerable<TResult> Select_<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            if (source is null) throw new ArgumentNullException(nameof(source), Exceptions.CANNOT_BE_NULL);
            if (selector is null) throw new ArgumentNullException(nameof(selector), Exceptions.CANNOT_BE_NULL);

            foreach (var item in source)
                yield return selector(item);
        }

        public static bool Contains_<T>(this IEnumerable<T> source, T element)
        {
            if (source is null) throw new ArgumentNullException(nameof(source), Exceptions.CANNOT_BE_NULL);

            var comparer = EqualityComparer<T>.Default;
            foreach (var item in source)
                if (comparer.Equals(item, element)) return true;
            return false;
        }

        public static bool ContainsAny_<T>(this IEnumerable<T> source, params T[] elements) => ContainsAny_(source, (IEnumerable<T>)elements);
        public static bool ContainsAny_<T>(this IEnumerable<T> source, IEnumerable<T> elements)
        {
            if (source is null) throw new ArgumentNullException(nameof(source), Exceptions.CANNOT_BE_NULL);
            if (elements is null) throw new ArgumentNullException(nameof(source), Exceptions.CANNOT_BE_NULL);

            var comparer = EqualityComparer<T>.Default;
            foreach (var item in source)
                foreach (var element in elements)
                    if (comparer.Equals(item, element)) return true;
            return false;
        }

        public static bool ContainsAll_<T>(this IEnumerable<T> source, params T[] elements) => ContainsAll_(source, (IEnumerable<T>)elements);
        public static bool ContainsAll_<T>(this IEnumerable<T> source, IEnumerable<T> elements)
        {
            if (source is null) throw new ArgumentNullException(nameof(source), Exceptions.CANNOT_BE_NULL);
            if (elements is null) throw new ArgumentNullException(nameof(source), Exceptions.CANNOT_BE_NULL);

            var bucket = new Bucket<T>(elements);

            var comparer = EqualityComparer<T>.Default;
            foreach (var item in source)
                for (int i = 0; i < bucket.Count; i++)
                {
                    if (comparer.Equals(item, bucket.Buffer.Array[i])) 
                        bucket.Pop(i);
                    if (bucket.Count == 0) return true;
                }

            return false;
        }

        public static int IndexOf_<T>(this IEnumerable<T> source, T element)
        {
            if (source is null) throw new ArgumentNullException(nameof(source), Exceptions.CANNOT_BE_NULL);

            int index = -1;
            int i = 0;

            var comparer = EqualityComparer<T>.Default;
            foreach (var item in source)
            {
                if (comparer.Equals(item, element)) index = i;
                i++;
            }

            return index;
        }

        public static int IndexOf_<T>(this IEnumerable<T> source, Predicate<T> predicate)
        {
            if (source is null) throw new ArgumentNullException(nameof(source), Exceptions.CANNOT_BE_NULL);
            if (predicate is null) throw new ArgumentNullException(nameof(predicate), Exceptions.CANNOT_BE_NULL);

            int index = -1;
            int i = 0;

            foreach (var item in source)
            {
                if (predicate(item)) index = i;
                i++;
            }

            return index;
        }

        public static int IndexOfAny_<T>(this IEnumerable<T> source, params T[] elements) => IndexOfAny_(source, elements);
        public static int IndexOfAny_<T>(this IEnumerable<T> source, IEnumerable<T> elements)
        {
            if (source is null) throw new ArgumentNullException(nameof(source), Exceptions.CANNOT_BE_NULL);
            if (elements is null) throw new ArgumentNullException(nameof(elements), Exceptions.CANNOT_BE_NULL);

            int index = -1;
            int i = 0;

            var comparer = EqualityComparer<T>.Default;
            foreach (var item in source)
            {
                foreach (var element in elements)
                    if (comparer.Equals(item, element)) index = i;
                i++;
            }

            return index;
        }

        private readonly ref struct Buffer<T>
        {
            public T[] Array { get; }
            public int Count { get; }

            //source is null ? should be checked beforehand
            public Buffer(IEnumerable<T> source)
            {
                if (source is ICollection<T> collection)
                {
                    Count = collection.Count;
                    Array = new T[Count];
                    collection.CopyTo(Array, 0);
                    return;
                }

                Array = new T[4];
                Count = 0;
                foreach (var item in source)
                {
                    if (Count == Array.Length)
                    {
                        if (Count == ARRAY_MAX_LENGTH) throw new OverflowException(Exceptions.ARR_MAX_CAPACITY_EXCEEDED);

                        Count *= 2;
                        if ((uint)Count > ARRAY_MAX_LENGTH) Count = ARRAY_MAX_LENGTH;
                        T[] newItems = new T[Count];
                        Array.CopyTo(newItems, 0);
                        Array = newItems;
                    }
                    Array[Count] = item;
                    Count++;
                }
            }

            public T[] ToArray()
            {
                if (Count == 0) return System.Array.Empty<T>();
                if (Array.Length == Count) return Array;

                T[] result = new T[Count];
                Array.CopyTo(result, 0);
                return result;
            }
        }

        private ref struct Bucket<T>
        {
            public Buffer<T> Buffer { get; }
            public int Count { get; private set; }

            public Bucket(IEnumerable<T> source)
            {
                Buffer = new Buffer<T>(source);
                Count = Buffer.Count;
            }

            public void Pop(int index)
            {
                Count--;
                Buffer.Array[index] = Buffer.Array[Count];
            }
        }
    }
}
