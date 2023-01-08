using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomCollections
{
    public sealed class LinkedListNode_<T> : Wrapper<T>
    {
        public LinkedList_<T> List { get; private set; }
        public LinkedListNode_<T> Previous { get; private set; }
        public LinkedListNode_<T> Next { get; private set; }

        public LinkedListNode_(T value) => Value = value;

        internal LinkedListNode_(LinkedList_<T> list, T value) : this(value) =>  List = list;

        internal void ChainNext(LinkedListNode_<T> node)
        {
            Next = node;
            node.Previous = this;
        }

        internal void ChainPrevious(LinkedListNode_<T> node)
        {
            Previous = node;
            node.Next = this;
        }

        internal void RemoveNext() => Next = null;
        internal void RemovePrevious() => Previous = null;
        internal void Invalidate()
        {
            List = null;
            Previous = null;
            Next = null;
        }
    }
}
