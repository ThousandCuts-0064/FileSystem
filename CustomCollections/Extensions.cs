using System;
using System.Collections.Generic;

namespace CustomCollections
{
    public static class Extensions
    {
        public static IReadOnlyList<T> ToReadOnly<T>(this IList<T> list) => new ReadOnlyList_<T>(list);
        public static IReadOnlyCollection<T> ToReadOnly<T>(this ICollection<T> collection) => new ReadOnlyCollection_<T>(collection);
    }
}
