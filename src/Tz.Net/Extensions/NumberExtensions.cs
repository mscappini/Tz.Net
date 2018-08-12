using System;
using System.Numerics;

namespace Tz.Net.Extensions
{
    public static class NumberExtensions
    {
        public static decimal ToMicroTez(this decimal source)
        {
            return Math.Round(source, 6) * 1000000;
        }

        public static BigFloat ToMicroTez(this BigFloat source)
        {
            return source * 1000000;
        }

        public static BigFloat ToTez(this BigFloat source)
        {
            return source / 1000000;
        }
    }
}