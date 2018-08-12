using System;

namespace Tz.Net.Extensions
{
    public static class ArrayExtensions
    {
        public static T[] CopyOfRange<T>(this T[] source, int start, int end)
        {
            int len = end - start;

            T[] dest = new T[len];

            for (int i = 0; i < len; i++)
            {
                dest[i] = source[start + i]; // 0..n = 0 + x .. n + x
            }

            return dest;
        }

        public static string ToHexString(this byte[] source)
        {
            return string.Join(string.Empty, Array.ConvertAll(source, x => x.ToString("X2")));
        }
    }
}