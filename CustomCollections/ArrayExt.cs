using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomCollections
{
    public static class ArrayExt
    {
        public static bool Contains_<T>(this T[] arr, T item) => Contains_(arr, item, 0, arr.Length);
        public static bool Contains_<T>(this T[] arr, T item, int index, int count)
        {
            count += index;
            if (item == null)
            {
                for (int i = index; i < count; i++)
                    if (arr[i] == null) return true;
                return false;
            }

            var comparer = EqualityComparer<T>.Default;
            for (int i = index; i < count; i++)
                if (comparer.Equals(arr[i], item)) return true;
            return false;
        }

        public static int IndexOf_<T>(this T[] arr, T item, int index, int count)
        {
            count += index;
            if (item == null)
            {
                for (int i = index; i < count; i++)
                    if (arr[i] == null) return i;
                return -1;
            }

            var comparer = EqualityComparer<T>.Default;
            for (int i = index; i < count; i++)
                if (comparer.Equals(arr[i], item)) return i;
            return -1;
        }
    }
}
