using System;
using System.Collections.Generic;

namespace CustomCollections
{
    public static class Extensions
    {
        public static IReadOnlyList<T> ToReadOnly<T>(this IList<T> list) => new ReadOnlyList<T>(list);
    }
}
