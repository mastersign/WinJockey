namespace Mastersign.Tools;

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

}
