using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Payments.Mvc.Helpers
{
    public static class EnumerableHelpers
    {
        public static IEnumerable<IEnumerable<T>> Partition<T>(this IEnumerable<T> source, int size)
        {
            T[] array = null;
            var count = 0;
            foreach (var item in source)
            {
                if (array == null)
                {
                    array = new T[size];
                }
                array[count] = item;
                count++;
                if (count == size)
                {
                    yield return new ReadOnlyCollection<T>(array);
                    array = null;
                    count = 0;
                }
            }

            if (array != null)
            {
                Array.Resize(ref array, count);
                yield return new ReadOnlyCollection<T>(array);
            }
        }

        public static IEnumerable<string> SplitBy(this string str, int chunkSize)
        {
            return Enumerable.Range(0, str.Length / chunkSize)
                .Select(i => str.Substring(i * chunkSize, chunkSize));
        }
    }
}
