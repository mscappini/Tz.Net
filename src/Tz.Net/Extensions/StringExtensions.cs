using System;
using System.Linq;

namespace Tz.Net.Extensions
{
    public static class StringExtensions
    {
        public static byte[] HexToByteArray(this string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        public static decimal ToTez(this string tez)
        {
            return int.Parse(tez) / 1000000M;
        }
    }
}