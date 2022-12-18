using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using ExceptionsNS;

namespace CustomCollections
{
    [DebuggerDisplay("{_list}")]
    internal class ReadOnlyList_<T> : IList<T>, IReadOnlyList<T>
    {
        private readonly IList<T> _list;
        public int Count => _list.Count;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool ICollection<T>.IsReadOnly => true;

        public T this[int index] => _list[index];
        T IList<T>.this[int index] 
        {
            get => this[index];
            set => throw new NotSupportedException();
        }

        public ReadOnlyList_(IList<T> list) =>
            _list = list ?? throw new ArgumentNullException(nameof(list));

        public int IndexOf(T item) => _list.IndexOf(item);
        public bool Contains(T item) => _list.Contains(item);
        public void CopyTo(T[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);
        public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        void ICollection<T>.Add(T item) => throw new NotSupportedException();
        void IList<T>.Insert(int index, T item) => throw new NotSupportedException();
        bool ICollection<T>.Remove(T item) => throw new NotSupportedException();
        void IList<T>.RemoveAt(int index) => throw new NotSupportedException();
        void ICollection<T>.Clear() => throw new NotSupportedException();
    }
}
