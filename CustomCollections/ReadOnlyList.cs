using System;
using System.Collections;
using System.Collections.Generic;
using ExceptionsNS;

namespace CustomCollections
{
    internal class ReadOnlyList<T> : IReadOnlyList<T>
    {
        private readonly IList<T> _roList;
        public int Count => _roList.Count;

        public T this[int index] => _roList[index];

        public ReadOnlyList(IList<T> list) => 
            _roList = list ?? throw new ArgumentNullException(nameof(list), Exceptions.CANNOT_BE_NULL);

        public IEnumerator<T> GetEnumerator() => _roList.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
