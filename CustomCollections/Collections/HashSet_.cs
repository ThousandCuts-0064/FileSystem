using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Core;
using ExceptionsNS;
using static CustomCollections.Constants;

namespace CustomCollections
{
    [DebuggerDisplay(COLLECTION_DISP)]
    public class HashSet_<T> : ISet<T>, IReadOnlyCollection<T>
    {
        private readonly IEqualityComparer<T> _comparer;
        private readonly List_<UnorderedList_<Slot>> _slots;
        private int _maxCount;

        public int Count { get; private set; }
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool ICollection<T>.IsReadOnly => false;

        public HashSet_() : this(0, EqualityComparer<T>.Default) { }
        public HashSet_(int capacity) : this(capacity, EqualityComparer<T>.Default) { }
        public HashSet_(IEqualityComparer<T> comparer) : this(0, comparer) { }
        public HashSet_(int capacity, IEqualityComparer<T> comparer)
        {
            if (comparer is null) throw new NumberNegativeException(nameof(comparer));
            if (capacity < 0) throw new NumberNegativeException(nameof(capacity));

            _comparer = comparer;
            _slots = new List_<UnorderedList_<Slot>>(Math_.NextPrime(capacity - 1)); // if capacity is prime it will be taken otherwise next prime will be taken
            for (int i = 0; i < _slots.Count; i++)
                _slots[i] = new UnorderedList_<Slot>();
        }

        public HashSet_(IEnumerable<T> source) : this(source, EqualityComparer<T>.Default) { }
        public HashSet_(IEnumerable<T> source, IEqualityComparer<T> comparer)
        {
            if (comparer is null) throw new NumberNegativeException(nameof(comparer));
            if (source is null) throw new NumberNegativeException(nameof(source));

            _comparer = EqualityComparer<T>.Default;
            _slots = new List_<UnorderedList_<Slot>>(source is ICollection<T> collection ? collection.Count : 0);
            for (int i = 0; i < _slots.Count; i++)
                _slots[i] = new UnorderedList_<Slot>();

            foreach (var item in source)
                Add(item);
        }

        public bool Add(T item)
        {
            var newSlot = new Slot(item, _comparer.GetHashCode(item));
            var targetList = _slots[newSlot.HashCode % _slots.Count];

            if (ListIndexOf(targetList, newSlot) != -1)
                return false;

            if (_maxCount == targetList.Count)
            {
                Stack_<Slot> toBeMoved = new Stack_<Slot>();
                int nextPrime = Math_.NextPrime(_slots.Capacity);
                if (nextPrime < _slots.Capacity) throw new OverflowException();
                int oldCapacity = _slots.Capacity;
                _slots.Capacity = nextPrime;
                for (int i = oldCapacity; i < nextPrime; i++)
                    _slots[i] = new UnorderedList_<Slot>();

                for (int i = 0; i < _slots.Count; i++)
                {
                    var list = _slots[i];
                    for (int y = 0; y < list.Count; y++)
                    {
                        int shortHash = list[y].HashCode % nextPrime;
                        if (shortHash == i)
                            continue;

                        toBeMoved.Push(list[y]);
                        list.RemoveAt(y);
                    }
                }

                while (toBeMoved.Count > 0)
                {
                    var slot = toBeMoved.Pop();
                    _slots[slot.HashCode % _slots.Capacity].Add(slot);
                }
            }

            targetList.Add(newSlot);
            if (targetList.Count > _maxCount)
                _maxCount = targetList.Count;
            Count++;
            return true;
        }

        public bool Contains(T item)
        {
            var slot = new Slot(item, _comparer.GetHashCode(item));
            var targetList = _slots[slot.HashCode % _slots.Count];
            int index = ListIndexOf(targetList, slot);
            return index != -1;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array is null) throw new ArgumentNullException(nameof(array));
            if (array.Length < Count) throw new ArrayTooShortExcpetion(nameof(array));
            if ((uint)arrayIndex > (uint)(array.Length - Count)) throw new IndexOutOfBoundsException(nameof(arrayIndex));

            foreach (var list in _slots)
                foreach (var slot in list)
                    array[arrayIndex++] = slot.Item;
        }

        public bool Remove(T item)
        {
            var slot = new Slot(item, _comparer.GetHashCode(item));
            var targetList = _slots[slot.HashCode % _slots.Count];
            int index = ListIndexOf(targetList, slot);
            if (index == -1)
                return false;

            targetList.RemoveAt(index);
            Count--;
            return true;
        }

        public void Clear()
        {
            for (int i = 0; i < _slots.Count; i++)
                _slots[i].Clear();
            Count = 0;
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < _slots.Count; i++)
                for (int y = 0; y < _slots[i].Count; y++)
                    yield return _slots[i][y].Item;
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }
        public void IntersectWith(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }
        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }
        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }
        public bool IsSubsetOf(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }
        public bool IsSupersetOf(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }
        public bool Overlaps(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }
        public bool SetEquals(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }
        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }
        public void UnionWith(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        void ICollection<T>.Add(T item) => Add(item);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private int ListIndexOf(UnorderedList_<Slot> list, Slot slot)
        {
            for (int i = 0; i < list.Count; i++)
                if (_comparer.Equals(list[i].Item, slot.Item))
                    return i;
            return -1;
        }

        private readonly struct Slot
        {
            public readonly T Item;
            public readonly int HashCode;

            public Slot(T item, int hashCode)
            {
                Item = item;
                HashCode = hashCode;
            }
        }
    }
}
