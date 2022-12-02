using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExceptionsNS;

namespace CustomCollections
{
    public class LinkedList_<T> : ICollection<T>, IReadOnlyCollection<T>
    {
        public int Count { get; private set; }
        public LinkedListNode_<T> First { get; private set; }
        public LinkedListNode_<T> Last { get; private set; }
        bool ICollection<T>.IsReadOnly => false;

        public LinkedList_() { }

        public LinkedList_(IEnumerable<T> source)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));

            foreach (var item in source)
                AddLast(item);
        }

        public LinkedListNode_<T> AddLast(T item) => AddLastUnchecked(NewNode(item));
        public LinkedListNode_<T> AddLast(LinkedListNode_<T> node) => node is null
                ? throw new ArgumentNullException(nameof(node))
                : node.List is null
                    ? AddLastUnchecked(node)
                    : throw new ArgumentException($"{node} was already part of {nameof(LinkedList_<T>)}", nameof(node));

        public LinkedListNode_<T> AddFirst(T item) => AddFirstUnchecked(NewNode(item));
        public LinkedListNode_<T> AddFirst(LinkedListNode_<T> node) => node is null
                ? throw new ArgumentNullException(nameof(node))
                : node.List is null
                    ? AddFirstUnchecked(node)
                    : throw new ArgumentException($"{node} was already part of {nameof(LinkedList_<T>)}", nameof(node));

        public LinkedListNode_<T> AddAfter(LinkedListNode_<T> node, T item) => AddAfterNewUnchecked(node, NewNode(item));
        public LinkedListNode_<T> AddAfter(LinkedListNode_<T> node, LinkedListNode_<T> newNode) => newNode is null
            ? throw new ArgumentNullException(nameof(newNode))
            : newNode.List is null
                ? AddAfterNewUnchecked(node, newNode)
                : throw new ArgumentException($"{newNode} was already part of {nameof(LinkedList_<T>)}", nameof(newNode));

        public LinkedListNode_<T> AddBefore(LinkedListNode_<T> node, T item) => AddBeforeNewUncheked(node, NewNode(item));
        public LinkedListNode_<T> AddBefore(LinkedListNode_<T> node, LinkedListNode_<T> newNode) => newNode is null
            ? throw new ArgumentNullException(nameof(newNode))
            : newNode.List is null
                ? AddBeforeNewUncheked(node, newNode)
                : throw new ArgumentException($"{newNode} was already part of {nameof(LinkedList_<T>)}", nameof(newNode));

        public bool ContainsLast(T item)
        {
            var curr = Last;
            EqualityComparer<T> comparer = EqualityComparer<T>.Default;
            while (!(curr is null))
            {
                if (comparer.Equals(curr.Item, item))
                    return true;
                curr = curr.Previous;
            }
            return false;
        }

        public bool ContainsFirst(T item)
        {
            var curr = First;
            EqualityComparer<T> comparer = EqualityComparer<T>.Default;
            while (!(curr is null))
            {
                if (comparer.Equals(curr.Item, item))
                    return true;
                curr = curr.Next;
            }
            return false;
        }

        public LinkedListNode_<T> FindLast(T item)
        {
            var curr = Last;
            EqualityComparer<T> comparer = EqualityComparer<T>.Default;
            while (!(curr is null))
            {
                if (comparer.Equals(curr.Item, item))
                    return curr;
                curr = curr.Previous;
            }
            return null;
        }

        public LinkedListNode_<T> FindFirst(T item)
        {
            var curr = First;
            EqualityComparer<T> comparer = EqualityComparer<T>.Default;
            while (!(curr is null))
            {
                if (comparer.Equals(curr.Item, item))
                    return curr;
                curr = curr.Next;
            }
            return null;
        }

        public bool TryFindLast(T item, out LinkedListNode_<T> node)
        {
            node = null;
            var curr = Last;
            EqualityComparer<T> comparer = EqualityComparer<T>.Default;
            while (!(curr is null))
            {
                if (comparer.Equals(curr.Item, item))
                {
                    node = curr;
                    return true;
                }
                curr = curr.Previous;
            }
            return false;
        }

        public bool TryFindFirst(T item, out LinkedListNode_<T> node)
        {
            node = null;
            var curr = First;
            EqualityComparer<T> comparer = EqualityComparer<T>.Default;
            while (!(curr is null))
            {
                if (comparer.Equals(curr.Item, item))
                {
                    node = curr;
                    return true;
                }
                curr = curr.Next;
            }
            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array is null) throw new ArgumentNullException(nameof(array));
            if (array.Length < Count) throw new ArrayTooShortExcpetion(nameof(array));
            if ((uint)arrayIndex > (uint)(array.Length - Count)) throw new IndexOutOfBoundsException(nameof(arrayIndex));

            foreach (var item in this)
                array[arrayIndex++] = item;
        }

        public void RemoveLast()
        {
            Last = Last.Previous;
            Last.Next.Invalidate();
            Count--;
        }

        public void RemoveFirst()
        {
            First = First.Next;
            First.Previous.Invalidate();
        }

        public void Remove(LinkedListNode_<T> node)
        {
            if (node is null) throw new ArgumentNullException(nameof(node));
            if (node.List != this) throw new NodeDifferentCollectionException(nameof(node));

            if (Count == 1)
            {
                First = null;
                Last = null;
            }
            else
            {
                if (node.Next is null)
                {
                    Last = node.Previous;
                    Last.RemoveNext();
                }
                else if (node.Previous is null)
                {
                    First = node.Next;
                    First.RemovePrevious();
                }

                if (Count > 2) node.Next.ChainPrevious(node.Previous);
            }
            node.Invalidate();
            Count--;
        }

        public bool RemoveLast(T item)
        {
            if (!TryFindLast(item, out var node)) return false;

            Remove(node);
            return true;
        }

        public bool RemoveFirst(T item)
        {
            if (!TryFindFirst(item, out var node)) return false;

            Remove(node);
            return true;
        }

        public void Clear()
        {
            var curr = First;
            var next = curr.Next;
            while (!(next is null))
            {
                curr.Invalidate();
                curr = next;
                next = next.Next;
            }
            curr.Invalidate();
            First = null;
            Last = null;
            Count = 0;
        }

        public IEnumerator<T> GetEnumerator()
        {
            var curr = First;
            while (!(curr is null))
            {
                yield return curr.Item;
                curr = curr.Next;
            }
        }

        private LinkedListNode_<T> NewNode(T item) => new LinkedListNode_<T>(this, item);

        private LinkedListNode_<T> AddLastUnchecked(LinkedListNode_<T> node)
        {
            if (Count == 0) First = node;
            else Last.ChainNext(node); // if Count == 1 Last = Fist

            Last = node;
            Count++;
            return node;
        }

        private LinkedListNode_<T> AddFirstUnchecked(LinkedListNode_<T> node)
        {
            if (Count == 0) Last = node;
            else First.ChainNext(node); // if Count == 1 First = Last

            First = node;
            Count++;
            return node;
        }

        private LinkedListNode_<T> AddAfterNewUnchecked(LinkedListNode_<T> node, LinkedListNode_<T> newNode)
        {
            if (node is null) throw new ArgumentNullException(nameof(node));
            if (node.List != this) throw new NodeDifferentCollectionException(nameof(node));

            node.ChainNext(newNode);
            Count++;
            return newNode;
        }

        private LinkedListNode_<T> AddBeforeNewUncheked(LinkedListNode_<T> node, LinkedListNode_<T> newNode)
        {
            if (node is null) throw new ArgumentNullException(nameof(node));
            if (node.List != this) throw new NodeDifferentCollectionException(nameof(node));

            node.ChainPrevious(newNode);
            Count++;
            return newNode;
        }

        void ICollection<T>.Add(T item) => AddLast(item);
        bool ICollection<T>.Contains(T item) => ContainsLast(item);
        bool ICollection<T>.Remove(T item) => RemoveLast(item);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
