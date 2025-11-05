using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace MapsScraper
{
    public class Utils
    {
        private static readonly string[] WebExts =
        [
            "html?", "htm", "php", "asp", "aspx", "jsp", "jspx",
            "cgi", "pl", "shtml", "xhtml", "cfm", "rhtml", "py", "md", "xml", "json", "do", "action"
        ];

        private static readonly Regex WebExtPattern = new Regex(
            @"\.(" + string.Join("|", WebExts) + ")$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex AnyExtPattern = new Regex(
            @"\.([^./?#]+)$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex InvalidPathChars = new Regex(
            @"[\{\}\[\]:]",
            RegexOptions.Compiled);

        public static void LogToFile(string filePath, string message)
        {
            try
            {
                using (var writer = System.IO.File.AppendText(filePath))
                {
                    writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to log to file: {ex.Message}");
            }
        }

        public static string RemoveQueryAndFragment(string url)
        {
            try
            {
                var uri = new Uri(url);
                var cleanedUri = new UriBuilder(uri)
                {
                    Query = string.Empty,
                    Fragment = string.Empty
                };
                return cleanedUri.Uri.ToString();
            }
            catch
            {
                return url;
            }
        }

        public static Boolean IsValidPathname(string pathname)
        {
            if (string.IsNullOrWhiteSpace(pathname))
                return false;

            return !InvalidPathChars.IsMatch(pathname);
        }

        public static string AddSlashToUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return url;

            var regex = new Regex(@"^(\w+://|//|/|www\.)", RegexOptions.IgnoreCase);

            return regex.IsMatch(url) ? url : "/" + url;
        }

        public static string RemoveProtocol(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return url;

            var regex = new Regex(@"^(https?:\/\/)?(www\.)?", RegexOptions.IgnoreCase);
            return regex.Replace(url, "");
        }

        public static string GetDomainName(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return url;

            try
            {
                var uri = new UriBuilder(url).Uri;
                return uri.Host.Replace("www.", "");
            }
            catch
            {
                return url;
            }
        }

        public static string RemoveTLD(string domain)
        {
            if (string.IsNullOrWhiteSpace(domain))
                return domain;
            var parts = domain.Split('.');
            if (parts.Length <= 2)
                return domain;
            return string.Join('.', parts.Take(parts.Length - 1));
        }

        public static string RemoveWebExtension(string pathname)
        {
            if (string.IsNullOrEmpty(pathname))
                return pathname;

            string[] parts = pathname.Split(['#'], 2);
            parts[0] = Utils.WebExtPattern.Replace(parts[0], "", 1);

            return string.Join("#", parts);
        }

        public static bool HasNonWebExtension(string pathname)
        {
            if (string.IsNullOrEmpty(pathname))
                return false;

            string cleanPath = pathname.Split(['?', '#'], 2)[0];

            Match anyExtMatch = Utils.AnyExtPattern.Match(cleanPath);
            if (anyExtMatch.Success)
            {
                return !WebExtPattern.IsMatch(cleanPath);
            }

            return false;
        }

        public static List<T> RemoveDuplicates<T>(IEnumerable<T> data, Func<T, T, bool> compareFn)
        {
            var result = new List<T>();

            foreach (var item in data)
            {
                if (!result.Any(other => compareFn(item, other)))
                {
                    result.Add(item);
                }
            }

            return result;
        }

        public static Boolean IsValidCNPJ(string cnpj)
        {
            cnpj = Regex.Replace(cnpj, @"[^\d]", "");
            if (cnpj.Length != 14)
                return false;

            int[] multipliers1 = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
            int[] multipliers2 = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
            string tempCnpj = cnpj[..12];
            int sum = 0;

            for (int i = 0; i < 12; i++)
                sum += int.Parse(tempCnpj[i].ToString()) * multipliers1[i];

            int remainder = sum % 11;
            int digit1 = remainder < 2 ? 0 : 11 - remainder;
            tempCnpj += digit1.ToString();
            sum = 0;

            for (int i = 0; i < 13; i++)
                sum += int.Parse(tempCnpj[i].ToString()) * multipliers2[i];

            remainder = sum % 11;
            int digit2 = remainder < 2 ? 0 : 11 - remainder;
            return cnpj.EndsWith(digit1.ToString() + digit2.ToString());
        }

    }
}
