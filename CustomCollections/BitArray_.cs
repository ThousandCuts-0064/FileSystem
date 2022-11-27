using System;
using System.Collections;
using System.Collections.Generic;
using ExceptionsNS;
using static Core.Constants;

namespace CustomCollections
{
    public class BitArray_ : IList<bool>, IReadOnlyList<bool>
    {
        private readonly byte[] _bytes;
        public int Count { get; private set; }
        public int ByteCount => _bytes.Length;
        bool ICollection<bool>.IsReadOnly => false;

        public bool this[int index]
        {
            get => (uint)index < (uint)Count
                ? (_bytes[index / BYTE_BITS] & 1 << BYTE_LAST_BIT - index) == 1
                : throw new IndexOutOfBoundsException(nameof(index));

            set
            {
                if ((uint)index >= (uint)Count)
                    throw new IndexOutOfBoundsException(nameof(index));

                int val = sbyte.MinValue >> index % BYTE_BITS;
                _bytes[index / BYTE_BITS] = (byte)(value ? val : ~val);
            }
        }

        public BitArray_(byte[] bytes)
        {
            _bytes = new byte[bytes.Length];
            Array.Copy(bytes, _bytes, _bytes.Length);
            Count = bytes.Length * BYTE_BITS;
        }

        public BitArray_(int length)
        {
            _bytes = new byte[length / BYTE_BITS + length % BYTE_BITS > 0 ? 1 : 0];
            Count = length;
        }

        public BitArray_(byte[] bytes, int lenth)
        {
            _bytes = new byte[bytes.Length];
            Array.Copy(bytes, _bytes, _bytes.Length);
            Count = lenth;
        }

        public byte GetByte(int index) => _bytes[index];
        public byte[] GetBytes()
        {
            byte[] bytes = new byte[_bytes.Length];
            Array.Copy(_bytes, bytes, _bytes.Length);
            return bytes;
        }

        public bool Contains(bool item)
        {
            byte val = item ? byte.MinValue : byte.MaxValue;

            for (int i = 0; i < _bytes.Length - 1; i++)
                if (_bytes[i] != val) return true;

            val = (byte)((sbyte.MinValue >> (Count % BYTE_BITS)) - 1);
            return Count != _bytes.Length / BYTE_BITS // false if Count matches _bytes.Length / BYTE_BITS
                && _bytes[_bytes.Length - 1] != (byte)(item ? ~val : val);
        }

        public int IndexOf(bool item)
        {
            byte val = item ? byte.MinValue : byte.MaxValue;
            int i = 0;

            while (i < _bytes.Length - 1)
            {
                if (_bytes[i] != val)
                {
                    if (item)
                    {
                        for (int y = 0; y < BYTE_BITS; y++)
                            if ((_bytes[i] & (byte)(sbyte.MinValue >> y)) != 0)
                                return i * BYTE_BITS + y;
                    }
                    else
                        for (int y = 0; y < BYTE_BITS; y++)
                            if ((_bytes[i] & (byte)(sbyte.MinValue >> y)) == 0)
                                return i * BYTE_BITS + y;
                }
                i++;
            }

            if (Count == _bytes.Length / BYTE_BITS) return -1;

            int leftover = Count / BYTE_BITS - _bytes.Length;
            byte last = _bytes[_bytes.Length - 1];

            if (item)
            {
                for (int y = 0; y < leftover; y++)
                    if ((last & (byte)(sbyte.MinValue >> y)) != 0)
                        return i * BYTE_BITS + y;
            }
            else
                for (int y = 0; y < leftover; y++)
                    if ((last & (byte)(sbyte.MinValue >> y)) == 0)
                        return i * BYTE_BITS + y;

            return -1;
        }

        public void CopyTo(bool[] array, int arrayIndex)
        {
            if (array is null) throw new ArgumentNullException(nameof(array));
            if (array.Length < Count) throw new ArrayTooShortExcpetion(nameof(array));
            if ((uint)arrayIndex > (uint)(array.Length - Count)) throw new IndexOutOfBoundsException(nameof(arrayIndex));

            while (arrayIndex < Count)
                array[arrayIndex] = this[arrayIndex++];
        }

        public void Clear()
        {
            Array.Clear(_bytes, 0, _bytes.Length);
            Count = 0;
        }

        public IEnumerator<bool> GetEnumerator()
        {
            int c = 0;
            foreach (var item in _bytes)
                for (int i = 0; i < BYTE_BITS && c++ < Count; i++)
                    yield return (item >> i & 1) == 1;
        }

        public void Add(bool item) => throw new NotSupportedException();
        public void Insert(int index, bool item) => throw new NotSupportedException();
        public bool Remove(bool item) => throw new NotSupportedException();
        public void RemoveAt(int index) => throw new NotSupportedException();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    }
}
