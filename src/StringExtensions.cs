using System;

namespace Mastersign.Tools
{
    public static class StringExtensions
    {
        public static string Ellipsis(this string s, int maxLength = 80, int remainder = 0, string ellipsis = "…")
        {
            if (remainder < 0) throw new ArgumentOutOfRangeException(nameof(remainder),
                "The remainder must not be negative.");
            if (maxLength < remainder + ellipsis.Length) throw new ArgumentOutOfRangeException(nameof(maxLength),
                "The max length must be at least the remainder plus the length of the ellipsis.");
            if (s.Length <= maxLength) return s;
            return remainder == 0
                ? s.Substring(0, maxLength - ellipsis.Length) + ellipsis
                : s.Substring(0, maxLength - ellipsis.Length - remainder) + ellipsis + s.Substring(s.Length - remainder);
        }

        // https://stackoverflow.com/a/9995303

        public static byte[] ParseHex(this string hex)
        {
            if (hex.Length % 2 == 1)
            {
                throw new FormatException("The hexadecimal representation of bytes cannot have an odd number of digits");
            }
            var arr = new byte[hex.Length >> 1];
            for (var i = 0; i < hex.Length >> 1; ++i)
            {
                arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));
            }
            return arr;
        }

        private static int GetHexVal(char hex)
        {
            var val = (int)hex;
            //For uppercase A-F letters:
            //return val - (val < 58 ? 48 : 55);
            //For lowercase a-f letters:
            //return val - (val < 58 ? 48 : 87);
            //Or the two combined, but a bit slower:
            return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
        }
    }
}
