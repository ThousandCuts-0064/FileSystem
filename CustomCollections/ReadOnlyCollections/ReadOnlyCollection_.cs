using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using ExceptionsNS;

namespace CustomCollections
{
    [DebuggerDisplay("{_collection}")]
    public class ReadOnlyCollection_<T> : ICollection<T>, IReadOnlyCollection<T>
    {
        private readonly ICollection<T> _collection;
        public int Count => _collection.Count;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool ICollection<T>.IsReadOnly => true;

        public ReadOnlyCollection_(ICollection<T> collection) => _collection = collection ?? throw new ArgumentNullException(nameof(collection));

        public bool Contains(T item) => _collection.Contains(item);
        public void CopyTo(T[] array, int arrayIndex) => _collection.CopyTo(array, arrayIndex);
        public IEnumerator<T> GetEnumerator() => _collection.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        void ICollection<T>.Add(T item) => throw new NotSupportedException();
        bool ICollection<T>.Remove(T item) => throw new NotSupportedException();
        void ICollection<T>.Clear() => throw new NotSupportedException();
    }
}
