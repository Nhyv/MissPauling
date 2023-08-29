using System;
using System.Text;

namespace MissPaulingBot.Extensions
{
    public static class StringExtensions
    {
        public static string OwOify(this string str)
        {
            return str.Replace("r", "w").Replace("l", "w").Replace("R", "W").Replace("L", "W");
        }

        public static string Truncate(this string str, int length, bool useEllipses = true)
        {
            if (str.Length <= length)
                return str;

            return useEllipses
                ? str[..(length - 1)] + '…'
                : str[..length];
        }

        public static string TrimTo(this string str, int length, bool useEllipses = false)
        {
            if (string.IsNullOrWhiteSpace(str))
                return str;
    
            if (!useEllipses)
                return str[..Math.Min(length, str.Length)];
    
            if (length > str.Length)
                return str;
    
            return str[..(length - 1)] + '…';
        }

        public static StringBuilder AppendNewLine(this StringBuilder builder, string text = null)
            => builder.Append($"{text}\n");
    }
}