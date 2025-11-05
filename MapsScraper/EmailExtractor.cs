using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace MapsScraper
{
    class EmailExtractor
    {
        private static readonly Regex EmailRegex = new Regex(
            @"\b[a-zA-Z0-9](?:[a-zA-Z0-9_.+-]*[a-zA-Z0-9])?@[a-zA-Z0-9](?:[a-zA-Z0-9.-]*[a-zA-Z0-9])?\.[a-zA-Z]{2,}\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly string[] ExcludeDomains =
        [
            "bing.com", "microsoft.com", "msn.com", "live.com", "example.com",
            "exemplo.com", "ejemplo.com", "test.com", "localhost", "domain.com",
        ];

        private static readonly Regex[] InvalidPatterns =
        [
            new Regex(@"^no-?reply", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            new Regex(@"^donotreply", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            new Regex(@"^admin@", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            new Regex(@"^webmaster@", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            new Regex(@"^info@example", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            new Regex(@"^test[\W_]", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            new Regex(@"@sentry", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            new Regex(@"@email", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            new Regex(@"@mail", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            new Regex(@"@sentry\.io$", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            new Regex(@"@\d", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            new Regex(@"^support@", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            new Regex(@"@.*\.(png|jpg|gif|svg)$", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        ];

        private static readonly HashSet<string> Tlds = new(StringComparer.OrdinalIgnoreCase)
        {
            "com", "org", "net", "edu", "gov", "mil", "int", "co", "io", "ai",
            "me", "br", "uk", "ca", "de", "tech", "app", "online", "shop",
        };

        // Detecta repetição de 3+ caracteres idênticos no domínio
        private static bool HasRepeatedCharacters(string domain)
        {
            return Regex.IsMatch(domain, @"(.)\1\1+", RegexOptions.IgnoreCase);
        }

        private static string DecodeCloudflareEmail(string email)
        {
            if (email == null)
                throw new Exception("Cannot decode None value. Please provide a valid encoded email string.");

            if (!(email is string))
                throw new Exception($"Invalid input type. Expected string, got {email.GetType().Name}.");

            if (email.Length < 2)
                throw new Exception("Encoded email string too short. Minimum length is 2 characters.");

            if (email.Length % 2 != 0)
                throw new Exception("Invalid encoded email format. String length must be even for proper hex decoding.");

            if (!Regex.IsMatch(email, @"^[a-fA-F0-9]+$"))
                throw new Exception($"Invalid encoded email format. String contains non-hexadecimal characters: '{email}'");

            try
            {
                int key = Convert.ToInt32(email.Substring(0, 2), 16);
                string finalEmail = "";

                for (int i = 2; i < email.Length; i += 2)
                {
                    string hexPair = email.Substring(i, 2);
                    int asciiValue = Convert.ToInt32(hexPair, 16) ^ key;

                    if (asciiValue < 0 || asciiValue > 127)
                        throw new Exception($"Decoded character has invalid ASCII value: {asciiValue}. Expected value between 0-127.");

                    finalEmail += (char)asciiValue;
                }

                return finalEmail;
            }
            catch (Exception e)
            {
                throw new Exception($"Error during decoding: {e.Message}");
            }
        }

        private static List<string> ExtractCloudflareEmails(string htmlContent)
        {
            Regex[] cloudflarePatterns =
            [
                new Regex(@"/cdn-cgi/l/email-protection#([a-fA-F0-9]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled),
                new Regex(@"data-cfemail=""([a-fA-F0-9]+)""", RegexOptions.IgnoreCase | RegexOptions.Compiled),
                new Regex(@"__cf_email__\s*=\s*[""']([a-fA-F0-9]+)[""']", RegexOptions.IgnoreCase | RegexOptions.Compiled),
                new Regex(@"\[email\s*protected\].*?([a-fA-F0-9]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            ];

            Regex regexRemoveExceptA = new Regex(@"<\/?(?!a\b|\/a\b)[^>]*>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            string cleanHtml = regexRemoveExceptA.Replace(htmlContent, " ");

            List<string> decodedEmails = [];

            foreach (Regex pattern in cloudflarePatterns)
            {
                MatchCollection matches = pattern.Matches(cleanHtml);
                foreach (Match match in matches)
                {
                    string encoded = match.Groups[1].Value;
                    try
                    {
                        decodedEmails.Add(DecodeCloudflareEmail(encoded));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Erro ao decodificar email '{encoded}': {e.Message}");
                    }
                }
            }

            return decodedEmails;
        }

        private static bool IsValidEmail(string email, string? targetDomain = null)
        {
            if (string.IsNullOrEmpty(email))
                return false;

            email = email.ToLower().Trim();

            if (!EmailRegex.IsMatch(email))
                return false;

            string[] parts = email.Split('@');
            if (parts.Length != 2)
                return false;

            string localPart = parts[0];
            string domain = parts[1];

            if (localPart.Length < 1 || domain.Length < 3)
                return false;

            if (HasRepeatedCharacters(domain))
                return false;

            foreach (string excludeDomain in ExcludeDomains)
            {
                if (domain.Contains(excludeDomain))
                    return false;
            }

            foreach (Regex pattern in InvalidPatterns)
            {
                if (pattern.IsMatch(email))
                    return false;
            }

            string[] domainParts = domain.Split('.');
            if (domainParts.Length < 2)
                return false;

            string tld = domainParts[^1];
            if (!Tlds.Contains(tld))
                return false;

            if (!string.IsNullOrEmpty(targetDomain))
            {
                string targetBase = Utils.RemoveTLD(targetDomain);
                return email.Contains(targetBase);
            }

            return true;
        }

        public static HashSet<string> ExtractEmailsFromText(string html, string? domain = null)
        {
            HashSet<string> emails = [];
            MatchCollection allEmails = EmailRegex.Matches(html);

            foreach (Match match in allEmails)
            {
                string email = match.Value;
                if (IsValidEmail(email, domain))
                {
                    emails.Add(email.ToLower().Trim());
                    if (!string.IsNullOrEmpty(domain))
                        Console.WriteLine($"Email extraído (HTML) {email} do domínio {domain}");
                    else
                        Console.WriteLine($"Email extraído (HTML): {email}");
                }
            }

            return emails;
        }

        public static List<string> SearchEmails(string html, string? domain = null)
        {
            HashSet<string> emails = [];
            Console.WriteLine($"Extraindo Emails do site: {domain}");

            string cleanHtml = TextDecoder.DecodeEntities(html);

            if (string.IsNullOrWhiteSpace(cleanHtml))
                return [];

            emails.UnionWith(ExtractEmailsFromText(cleanHtml, domain));

            if (emails.Count == 0)
            {
                emails.UnionWith(ExtractCloudflareEmails(html));
            }

            if (emails.Count == 0)
            {
                Console.WriteLine($"Nenhum Email encontrado no site: {domain}");
            }
            else
            {
                Console.WriteLine($"Listando Emails encontrados no site: {domain}");
            }

            List<string> sortedEmails = [.. emails];
            sortedEmails.Sort(StringComparer.OrdinalIgnoreCase);
            return sortedEmails;
        }
    }
}
