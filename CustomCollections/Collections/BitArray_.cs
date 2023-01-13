using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Core;
using ExceptionsNS;
using static Core.Constants;
using static CustomCollections.Constants;

namespace CustomCollections
{
    [DebuggerDisplay(COLLECTION_DISPLAY)]
    public class BitArray_ : ICollection<bool>, IReadOnlyList<bool>
    {
        private readonly byte[] _bytes;
        public int SetBits { get; private set; }
        public int Count { get; }
        public int UnsetBits => Count - SetBits;
        public int ByteCount => _bytes.Length;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool ICollection<bool>.IsReadOnly => false;

        public bool this[int index]
        {
            get => (uint)index < (uint)Count
                ? (_bytes[index / BYTE_BITS] & 1 << BYTE_LAST_BIT - index % BYTE_BITS) != 0
                : throw new IndexOutOfBoundsException(nameof(index));

            set
            {
                if ((uint)index >= (uint)Count)
                    throw new IndexOutOfBoundsException(nameof(index));

                int val = 1 << BYTE_LAST_BIT - index % BYTE_BITS;
                int byteIndex = index / BYTE_BITS;
                byte target = _bytes[byteIndex];
                if (value)
                {
                    target |= (byte)val;
                    if (target == _bytes[byteIndex]) return;

                    SetBits++;
                }
                else
                {
                    target &= (byte)~val;
                    if (target == _bytes[byteIndex]) return;

                    SetBits--;
                }
                _bytes[byteIndex] = target;
            }
        }

        public BitArray_(int length)
        {
            _bytes = new byte[Math_.DivCeiling(length, BYTE_BITS)];
            Count = length;
        }

        public BitArray_(byte[] bytes)
        {
            _bytes = new byte[bytes.Length];
            Array.Copy(bytes, _bytes, _bytes.Length);
            Count = bytes.Length * BYTE_BITS;
            foreach (var bit in this)
                if (bit) SetBits++;
        }

        public BitArray_(byte[] bytes, int lenth)
        {
            _bytes = new byte[bytes.Length];
            Array.Copy(bytes, _bytes, _bytes.Length);
            Count = lenth;
            foreach (var bit in this)
                if (bit) SetBits++;
        }

        public byte GetByte(int index) => _bytes[index];
        public byte[] GetBytes()
        {
            byte[] bytes = new byte[_bytes.Length];
            Array.Copy(_bytes, bytes, _bytes.Length);
            return bytes;
        }

        public bool Contains(bool item) => IndexOf(item) != -1;
        public int IndexOf(bool item) => IndexOf(item, 0);
        public int IndexOf(bool item, int startIndex)
        {
            byte val = item ? byte.MinValue : byte.MaxValue;
            int i = Math.DivRem(startIndex, BYTE_BITS, out int leftover);
            int fullBytes = Math.DivRem(Count, BYTE_BITS, out int remaining);

            if (_bytes[i] != val)
            {
                if (item)
                {
                    for (int y = leftover; y < BYTE_BITS; y++)
                        if ((_bytes[i] & 1 << BYTE_LAST_BIT - y) != 0)
                            return i * BYTE_BITS + y;
                }
                else
                    for (int y = leftover; y < BYTE_BITS; y++)
                        if ((_bytes[i] & 1 << BYTE_LAST_BIT - y) == 0)
                            return i * BYTE_BITS + y;
            }
            i++;

            while (i < fullBytes)
            {
                if (_bytes[i] != val)
                {
                    if (item)
                    {
                        for (int y = 0; y < BYTE_BITS; y++)
                            if ((_bytes[i] & 1 << BYTE_LAST_BIT - y) != 0)
                                return i * BYTE_BITS + y;
                    }
                    else
                        for (int y = 0; y < BYTE_BITS; y++)
                            if ((_bytes[i] & 1 << BYTE_LAST_BIT - y) == 0)
                                return i * BYTE_BITS + y;
                }
                i++;
            }
            if (remaining == 0) return -1;

            byte last = _bytes[_bytes.Length - 1];

            if (item)
            {
                for (int y = 0; y < remaining; y++)
                    if ((last & 1 << BYTE_LAST_BIT - y) != 0)
                        return i * BYTE_BITS + y;
            }
            else
                for (int y = 0; y < remaining; y++)
                    if ((last & 1 << BYTE_LAST_BIT - y) == 0)
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

        public void CopyTo(BitArray_ bitArray)
        {
            if (ByteCount != bitArray.ByteCount) throw new InvalidOperationException();

            _bytes.CopyTo(bitArray._bytes, 0);
        }

        public void Clear() => Array.Clear(_bytes, 0, _bytes.Length);

        public IEnumerator<bool> GetEnumerator()
        {
            int c = 0;
            foreach (var item in _bytes)
                for (int i = 0; i < BYTE_BITS && c++ < Count; i++)
                    yield return (item & 1 << BYTE_LAST_BIT - i) != 0;
        }

        void ICollection<bool>.Add(bool item) => throw new NotSupportedException();
        bool ICollection<bool>.Remove(bool item) => throw new NotSupportedException();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    }
}
