using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace MapsScraper
{
    public static class TextDecoder
    {
        private static readonly Regex UnicodeEscapeRegex = new(
            @"(?:\\u|\\U|/u|u)([0-9a-fA-F]{4})",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );

        private static readonly Regex HtmlEntityRegex = new(
            @"&#x([0-9a-fA-F]+);|&#(\d+);",
            RegexOptions.Compiled
        );

        public static string DecodeUnicodeEscapes(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

           text = UnicodeEscapeRegex.Replace(text, match =>
            {
                string hex = match.Groups[1].Value;
                try
                {
                    int code = Convert.ToInt32(hex, 16);
                    return char.ConvertFromUtf32(code);
                }
                catch
                {
                    return match.Value;
                }
            });

            text = HtmlEntityRegex.Replace(text, match =>
            {
                try
                {
                    if (match.Groups[1].Success)
                    {
                        int code = Convert.ToInt32(match.Groups[1].Value, 16);
                        return char.ConvertFromUtf32(code);
                    }
                    else if (match.Groups[2].Success) 
                    {
                        int code = int.Parse(match.Groups[2].Value);
                        return char.ConvertFromUtf32(code);
                    }
                }
                catch
                {
                    return match.Value;
                }
                return match.Value;
            });

            return text;
        }

        public static string DecodeEntities(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            try
            {
                text = Uri.UnescapeDataString(text);
            }
            catch
            {
            }

            text = DecodeUnicodeEscapes(text);
            text = HttpUtility.HtmlDecode(text);
            text = Regex.Replace(text, @"\s+", " ");

            return text.Trim();
        }
    }
}
