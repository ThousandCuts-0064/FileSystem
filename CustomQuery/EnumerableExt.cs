using System;
using System.Collections;
using System.Collections.Generic;
using CustomCollections;
using ExceptionsNS;
using static CustomCollections.Constants;

namespace CustomQuery
{
    public static class EnumerableExt
    {
        public static T Last_<T>(this IEnumerable<T> source)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));

            if (source is IList<T> collection)
                return collection.Count > 0
                    ? collection[collection.Count - 1]
                    : throw new CollectionEmptyException(nameof(source));

            using (var enumerator = source.GetEnumerator())
            {
                if (!enumerator.MoveNext()) throw new CollectionEmptyException(nameof(source));
                T last = enumerator.Current;

                while (enumerator.MoveNext())
                    last = enumerator.Current;

                return last;
            }
        }

        public static IEnumerable<T> Concat_<T>(this IEnumerable<T> first, IEnumerable<T> second)
        {
            if (first is null) throw new ArgumentNullException(nameof(first));
            if (second is null) throw new ArgumentNullException(nameof(second));

            foreach (var item in first) yield return item;
            foreach (var item in second) yield return item;
        }

        public static T[] ToArray_<T>(this IEnumerable<T> source) =>
            source is null
            ? throw new ArgumentNullException(nameof(source))
            : new Buffer<T>(source).ToArray();

        public static T[] ToArrayFixed_<T>(this IEnumerable<T> source, int arrayLength)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));

            int index = 0;
            T[] arr = new T[arrayLength];
            foreach (var item in source)
                arr[index++] = item;
            return arr;
        }

        public static List_<T> ToList_<T>(this IEnumerable<T> source) =>
            source is null
            ? throw new ArgumentNullException(nameof(source))
            : new List_<T>(source);

        public static List_<T> ToListFixed_<T>(this IEnumerable<T> source, int listCapacity)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));

            int index = 0;
            List_<T> list = new List_<T>(listCapacity);
            foreach (var item in source)
                list[index++] = item;
            return list;
        }

        public static IEnumerable<TResult> Select_<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            if (selector is null) throw new ArgumentNullException(nameof(selector));

            foreach (var item in source)
                yield return selector(item);
        }

        public static IEnumerable<TResult> SelectTree_<TSource, TResult>(
            this IEnumerable<TSource> source,
            Func<TSource, IEnumerable<TSource>> childSelector,
            Func<TSource, int, TResult> selector)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            if (childSelector is null) throw new ArgumentNullException(nameof(childSelector));
            if (selector is null) throw new ArgumentNullException(nameof(selector));

            var stack = new Stack_<IEnumerator<TSource>>();
            var enumerator = source.GetEnumerator();

            try
            {
                while (true)
                {
                    if (enumerator.MoveNext())
                    {
                        TSource element = enumerator.Current;
                        yield return selector(element, stack.Count);

                        stack.Push(enumerator);
                        enumerator = childSelector(element).GetEnumerator();
                    }
                    else if (stack.Count > 0)
                    {
                        enumerator.Dispose();
                        enumerator = stack.Pop();
                    }
                    else yield break;
                }
            }
            finally
            {
                enumerator.Dispose();

                while (stack.Count > 0) // Clean up in case of an exception.
                {
                    enumerator = stack.Pop();
                    enumerator.Dispose();
                }
            }
        }

        public static bool Contains_<T>(this IEnumerable<T> source, T element)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));

            var comparer = EqualityComparer<T>.Default;
            foreach (var item in source)
                if (comparer.Equals(item, element)) 
                    return true;
            return false;
        }

        public static bool Contains_<T>(this IEnumerable<T> source, Predicate<T> predicate)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            if (predicate is null) throw new ArgumentNullException(nameof(predicate));

            foreach (var item in source)
                if (predicate(item)) 
                    return true;
            return false;
        }

        public static bool ContainsAny_<T>(this IEnumerable<T> source, params T[] elements) => ContainsAny_(source, (IEnumerable<T>)elements);
        public static bool ContainsAny_<T>(this IEnumerable<T> source, IEnumerable<T> elements)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            if (elements is null) throw new ArgumentNullException(nameof(source));

            var comparer = EqualityComparer<T>.Default;
            foreach (var item in source)
                foreach (var element in elements)
                    if (comparer.Equals(item, element)) 
                        return true;
            return false;
        }

        public static bool ContainsAll_<T>(this IEnumerable<T> source, params T[] elements) => ContainsAll_(source, (IEnumerable<T>)elements);
        public static bool ContainsAll_<T>(this IEnumerable<T> source, IEnumerable<T> elements)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            if (elements is null) throw new ArgumentNullException(nameof(source));

            var unorderedBuffer = new UnorderedBuffer<T>(elements);

            var comparer = EqualityComparer<T>.Default;
            foreach (var item in source)
                for (int i = 0; i < unorderedBuffer.Count; i++)
                {
                    if (comparer.Equals(item, unorderedBuffer.Buffer.Array[i]))
                        unorderedBuffer.RemoveAt(i);
                    if (unorderedBuffer.Count == 0) 
                        return true;
                }

            return false;
        }

        public static T First_<T>(this IEnumerable<T> source, Predicate<T> predicate)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            if (predicate is null) throw new ArgumentNullException(nameof(predicate));

            foreach (var item in source)
                if (predicate(item)) 
                    return item;

            throw new InvalidOperationException("No matches found");
        }

        public static T FirstOfType_<T>(this IEnumerable source)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));

            foreach (var item in source)
                if (item is T casted)
                    return casted;

            throw new InvalidOperationException("No matches found");
        }

        public static T FirstOrDefault_<T>(this IEnumerable<T> source, Predicate<T> predicate)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            if (predicate is null) throw new ArgumentNullException(nameof(predicate));

            foreach (var item in source)
                if (predicate(item)) 
                    return item;

            return default;
        }

        public static int IndexOf_<T>(this IEnumerable<T> source, T element)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));

            int i = 0;

            var comparer = EqualityComparer<T>.Default;
            foreach (var item in source)
            {
                if (comparer.Equals(item, element))
                    return i;
                i++;
            }

            return -1;
        }

        public static int IndexOf_<T>(this IEnumerable<T> source, Predicate<T> predicate)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            if (predicate is null) throw new ArgumentNullException(nameof(predicate));

            int i = 0;

            foreach (var item in source)
            {
                if (predicate(item))
                    return i;
                i++;
            }

            return -1;
        }

        public static int IndexOfAny_<T>(this IEnumerable<T> source, params T[] elements) => IndexOfAny_(source, (IEnumerable<T>)elements);
        public static int IndexOfAny_<T>(this IEnumerable<T> source, IEnumerable<T> elements)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            if (elements is null) throw new ArgumentNullException(nameof(elements));

            int i = 0;

            var comparer = EqualityComparer<T>.Default;
            foreach (var item in source)
            {
                foreach (var element in elements)
                    if (comparer.Equals(item, element)) 
                        return i;
                i++;
            }

            return -1;
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
                        if (Count == ARRAY_MAX_LENGTH) throw new OverflowException("Array max length was exceeded.");

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

        private ref struct UnorderedBuffer<T>
        {
            public Buffer<T> Buffer { get; }
            public int Count { get; private set; }

            public UnorderedBuffer(IEnumerable<T> source)
            {
                Buffer = new Buffer<T>(source);
                Count = Buffer.Count;
            }

            public void RemoveAt(int index)
            {
                Count--;
                Buffer.Array[index] = Buffer.Array[Count];
            }
        }
    }
}
