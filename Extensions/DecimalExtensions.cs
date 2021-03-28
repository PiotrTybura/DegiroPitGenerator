using System;

namespace Extensions
{
    public static class DecimalExtensions
    {
        public static decimal Round2(this decimal dec)
        {
            return Math.Round(dec, 2);
        }
    }
}
