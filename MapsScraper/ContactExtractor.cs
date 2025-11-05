using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MapsScraper
{
    public static class ContactExtractor
    {
        private const int ConcurrentLimit = 200;

        private static readonly HttpClientHandler _httpHandler = new()
        {
            AllowAutoRedirect = true,
            AutomaticDecompression = DecompressionMethods.All
        };

        private static readonly HttpClient _httpClient = new(_httpHandler)
        {
            Timeout = TimeSpan.FromSeconds(20)
        };

        static ContactExtractor()
        {
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
            _httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            _httpClient.DefaultRequestHeaders.Add("Accept-Language", "pt-BR,pt;q=0.9,en;q=0.8");
            _httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
        }

        private static bool InvalidDomains(string domain)
        {
            string[] invalidDomainList =
            [
            "google.com","yahoo.com","facebook.com","web.facebook.com","instagram.com",
            "web.whatsapp.com","api.whatsapp.com","linkedin.com","twitter.com","x.com",
            "youtube.com","youtu.be","tiktok.com","threads.com","pinterest.com","reddit.com",
            "tumblr.com","twitch.com","discord.com","iguatemiportoalegre.com.br",
            "iguatemi.com.br","medium.com","substack.com","linktr.ee","bit.ly","tinyurl.com",
            "rebrandly.com","shor.by","linklist.bio","curt.link","cutt.ly","linkdowhats.app",
            "wa.link","t.me","m.me","wa.me","w.app","shopee.com","amazon.com","ebay.com",
            "etsy.com","mercadolivre.com","magalu.com","aliexpress.com","alibaba.com"
            ];
            return invalidDomainList.Contains(domain, StringComparer.OrdinalIgnoreCase);
        }

        private static readonly Dictionary<string, Regex> Patterns = new()
        {
            ["contact"] = new Regex(@"\b(fale[-_]*(?:conosco|connosco?)|contatos?|contact[-_]*us|contacts?)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            ["about"] = new Regex(@"\b(sobre(?:[-_]*(?:nos|nós))?|quem[-_]*somos|institucional|institutional|about(?:[-_]*(?:us|me))?)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            ["work"] = new Regex(@"\b(trabalhe[-_]*conosco|trabalhe[-_]*aqui|carreiras|join[-_]*us|careers?|work[-_]*with[-_]*us)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            ["support"] = new Regex(@"\b(central[-_]*(?:de)?[-_]*(?:atendimento|ajuda|suporte)|help[-_#]*(?:center|centre)|atendimento|suporte|ajuda|help|customer[-_]*support|support|faqs?|frequently[-_]*asked[-_]*questions?|(?:duvidas|perguntas)[-_]*frequentes)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled)
        };

        private static bool IsPathnameComplex(string pathname, int maxHifens = -1, int maxSlashes = 3, int maxWords = -1)
        {
            int numHifens = Regex.Matches(pathname, "[-_]").Count;
            int numSlashes = Regex.Matches(pathname, @"/").Count;
            bool hasNumbers = Regex.IsMatch(pathname, @"\d");
            string[] words = Regex.Split(pathname, @"[-_\s/]+").Where(w => !string.IsNullOrEmpty(w)).ToArray();
            int numWords = words.Length;

            bool exceededHifens = maxHifens != -1 && numHifens > maxHifens;
            bool exceededSlashes = numSlashes > maxSlashes;
            bool exceededWords = maxWords != -1 && numWords > maxWords;

            return exceededHifens || exceededSlashes || hasNumbers || exceededWords;
        }

        private static bool UrlMayContainContactInfo(string url)
        {
            if (string.IsNullOrEmpty(url)) return false;

            string cleanUrl = url.TrimStart('/');
            if (Regex.IsMatch(cleanUrl, @"^(mailto:|mail:|#)") || Utils.HasNonWebExtension(cleanUrl)) return false;

            string allPatterns = string.Join("|", Patterns.Values.Select(p => $"({p})"));
            Regex regexKeywords = new Regex(allPatterns, RegexOptions.IgnoreCase | RegexOptions.Compiled);
            if (regexKeywords.IsMatch(url) && regexKeywords.IsMatch(Utils.GetDomainName(url))) return true;

            string pathname = url;
            string domain = url;
            try
            {
                var uri = new Uri(url);
                pathname = uri.PathAndQuery + (uri.Fragment ?? "");
                domain = uri.Host;
            }
            catch { return false; }

            if (!Utils.IsValidPathname(pathname)) return false;

            KeyValuePair<string, Regex> found = default;
            bool foundMatch = false;

            foreach (var pattern in Patterns)
            {
                if (pattern.Value.IsMatch(pathname))
                {
                    found = pattern;
                    foundMatch = true;
                    break;
                }
            }

            if (!foundMatch)
                return false;

            string patternType = found.Key;
            Regex regex = found.Value;
            Match matchedKeyword = regex.Match(pathname);

            if (!matchedKeyword.Success)
                return false;

            string keyword = matchedKeyword.Value;
            string[] segments = pathname.Trim('/').Split('/');
            string[] lastTwo = [.. segments.TakeLast(2)];
            string lastSegment = segments.LastOrDefault() ?? "";

            if (lastTwo.Length == 2 && lastTwo.All(seg => regexKeywords.IsMatch(seg)))
            {
                Utils.LogToFile("data/valid-urls.txt", pathname);
                return true;
            }

            if (regexKeywords.Match(lastSegment).Success && regexKeywords.Match(lastSegment).Value == lastSegment)
            {
                Utils.LogToFile("data/valid-urls.txt", pathname);
                return true;
            }

            if (!regexKeywords.IsMatch(lastSegment)) return false;

            if (patternType == "support")
            {
                if (pathname.EndsWith($"-{keyword}", StringComparison.OrdinalIgnoreCase)) return false;
                if (Regex.IsMatch(pathname, @"/(suporte|support)", RegexOptions.IgnoreCase))
                {
                    Console.WriteLine(pathname);
                    return false;
                }
                if (pathname.StartsWith($"/{keyword}", StringComparison.OrdinalIgnoreCase))
                    return !IsPathnameComplex(pathname);

                return !IsPathnameComplex(pathname, 3);
            }
            else if (patternType == "contact" || patternType == "about")
            {
                if (lastSegment.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    return !IsPathnameComplex(pathname);

                return !IsPathnameComplex(pathname, 1);
            }
            else if (patternType == "work")
            {
                return !IsPathnameComplex(pathname, 2);
            }

            return !IsPathnameComplex(pathname);
        }

        private static string? ProcessUrl(string url, string baseUrl, List<string>? outInvalid)
        {
            if (string.IsNullOrEmpty(url)) return null;

            if (Regex.IsMatch(url, @"^mailto:|^mail:|^tel:|^#")) return null;

            url = new Uri(new Uri(baseUrl), url.Trim().Replace("\\", "")).ToString();
            url = Utils.RemoveQueryAndFragment(url);
            url = url.Trim();
            url = Utils.AddSlashToUrl(url);
            url = url.TrimEnd('/');
            url = Utils.RemoveWebExtension(url).ToLower();

            if ((url.Contains("://") || url.Contains("www.")) && !url.Contains(Utils.GetDomainName(baseUrl))) return null;

            if (UrlMayContainContactInfo(url))
                return url;
            else
            {
                outInvalid?.Add(url);
                return null;
            }
        }

        public static async Task<List<string>> FetchEmailsFromLink(string link)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(link);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Status {(int)response.StatusCode} para URL: {link}");
                    return [];
                }

                string text = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(text))
                {
                    Console.WriteLine($"Dados inválidos para URL: {link}");
                    return [];
                }

                List<string> linkEmails = [.. EmailExtractor.SearchEmails(text)];
                if (linkEmails.Count != 0)
                    Console.WriteLine($" {linkEmails.Count} emails encontrados em: {link}");

   
                return linkEmails;
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine($"Requisição excedeu o tempo limite para: {link}");
                return [];
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Erro de conexão para {link}: {e.Message}");
                return [];
            }
            catch (Exception e)
            {
                Console.WriteLine($"Erro inesperado para {link}: {e.Message}");
                return [];
            }
        }

        private static async Task<List<string>> FetchEmailsFromLinks(List<string> links)
        {
            var semaphore = new SemaphoreSlim(20);
            var tasks = links.Select(async link =>
            {
                await semaphore.WaitAsync();
                try
                {
                    return await FetchEmailsFromLink(link);
                }
                finally
                {
                    semaphore.Release();
                }
            });

            var results = await Task.WhenAll(tasks);
            return [.. results.SelectMany(r => r)];
        }

        public static async Task<List<string>> SearchEmailInAllPages(string htmlContent, string baseUrl)
        {
            var cleanHtml = TextDecoder.DecodeEntities(htmlContent);

            var hrefPatterns = new[]
            {
                new Regex(@"<a[^>]*href=[""']([^""']*)[""']", RegexOptions.IgnoreCase | RegexOptions.Compiled),
                new Regex(@"""(?:link|href|url)""\s*:\s*""([^""]*)""", RegexOptions.IgnoreCase | RegexOptions.Compiled)
            };

            HashSet<string> allUrls = [];

            foreach (var pattern in hrefPatterns)
            {
                foreach (Match match in pattern.Matches(cleanHtml))
                {
                    if (match.Groups[1].Success)
                        allUrls.Add(match.Groups[1].Value);
                }
            }

            if(allUrls.Count == 0)
            {
                Console.WriteLine("Nenhum link encontrado na página.");
                return [];
            }

            var invalidLinks = new List<string>();
            var validLinks = allUrls
                .Select(url => ProcessUrl(url, baseUrl, invalidLinks))
                .Where(url => !string.IsNullOrWhiteSpace(url))
                .Distinct()
                .ToList();

            foreach (var link in validLinks)
            { 
                Console.WriteLine($"Link processado: {link}");
            }


            if (validLinks.Count == 0)
                return [];

            Console.WriteLine($"{validLinks.Count} links válidos encontrados");

            return await FetchEmailsFromLinks(validLinks);
        }

        private static List<string> ExtractSocialNetworkFromText(string text)
        {
            HashSet<string> socialUrls = [];

            foreach (var kvp in SocialNetworks.Networks)
            {
                var network = kvp.Value;
                foreach (Match match in network.Regex.Matches(text))
                {
                    string? url = match.Groups.Cast<Group>().Skip(1).FirstOrDefault(g => g.Success)?.Value;
                    if (!string.IsNullOrEmpty(url) && network.ValidateUrl(url) && !network.IsExcluded(url))
                        socialUrls.Add(url.TrimEnd('/'));
                }
            }

            return [.. socialUrls];
        }

        private static List<string> SearchEmailsFromBing(string text, string? domain = null)
        {
            List<string> emails = [];
            Regex algoRegex = new Regex(@"<li class=""b_algo[^""]*""[^>]*>([\s\S]*?)</li>", RegexOptions.IgnoreCase | RegexOptions.Compiled);

            foreach (Match match in algoRegex.Matches(text))
            {
                emails.AddRange(EmailExtractor.ExtractEmailsFromText(match.Groups[1].Value, domain));
            }

            return [.. emails.Distinct()];
        }

        private static async Task<List<string>> ProcessEmailFromBing(string domain)
        {
            try
            {
                string url = $"https://www.bing.com/search?q={domain}+email";
                HttpResponseMessage response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    throw new Exception($"Failed to fetch from Bing for domain: {domain}");

                string text = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(text))
                    throw new Exception($"Empty response text from Bing for domain: {domain}");

                return SearchEmailsFromBing(text, domain);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error processing domain {domain}: {e.Message}");
                return [];
            }
        }

        private static string? ExtractCnpjFromUrl(string text)
        {
            Regex cnpjPattern = new Regex(@"(?:\d{2}\.\d{3}\.\d{3}/\d{4}-\d{2}|\d{14})", RegexOptions.Compiled);
            Match match = cnpjPattern.Match(text);
            if (match.Success)
            {
                string cnpj = Regex.Replace(match.Value, @"\D", "");
                if (Utils.IsValidCNPJ(cnpj))
                    return match.Value;
            }
            return null;
        }

        public static async Task<Dictionary<string, object>> ExtractContactsUrl(string url, string domain)
        {
            if (string.IsNullOrWhiteSpace(url))
                return CreateEmptyResult();

            var emails = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var socialLinks = new Dictionary<string, string>
            {
                ["facebook"] = "",
                ["instagram"] = "",
                ["linkedin"] = "",
                ["twitter"] = "",
                ["youtube"] = ""
            };

            var bingEmails = await ProcessEmailFromBing(domain);
            if (bingEmails.Count != 0)
            {
                emails.UnionWith(bingEmails);
                Console.WriteLine($"{bingEmails.Count} emails do Bing para {domain}");
            }

            try
            {
                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Status {(int)response.StatusCode} para: {url}");
                    return CreateEmptyResult();
                }

                var text = await response.Content.ReadAsStringAsync();
                var cnpj = ExtractCnpjFromUrl(text);
                var socialType = SocialNetworks.GetSocialNetworkTypeFromUrl(url);
    
                if (!string.IsNullOrEmpty(socialType) && socialLinks.ContainsKey(socialType))
                {
                    socialLinks[socialType] = url;

                    // Facebook: apenas fetch direto
                    if (socialType == "facebook")
                    {
                        Console.WriteLine($"Facebook detectado: {url}");
                        var directEmails = await FetchEmailsFromLink(url);
                        if (directEmails.Count != 0)
                        {
                            emails.UnionWith(directEmails);
                            Console.WriteLine($"{directEmails.Count} emails do Facebook");
                        }
                    }
                }
                else
                {
                    var directEmails = await FetchEmailsFromLink(url);
                    if (directEmails.Count != 0)
                    {
                        emails.UnionWith(directEmails);
                        Console.WriteLine($"{directEmails.Count} emails diretos");
                    }
                    else
                    {
                        var recursiveEmails = await SearchEmailInAllPages(text, url);
                        if (recursiveEmails.Count != 0)
                        {
                            emails.UnionWith(recursiveEmails);
                            Console.WriteLine($"{recursiveEmails.Count} emails recursivos");
                        }
                    }

                    var extractedSocials = ExtractSocialNetworkFromText(text);
                    foreach (var link in extractedSocials)
                    {
                        var networkType = SocialNetworks.GetSocialNetworkTypeFromUrl(link);
                        if (!string.IsNullOrEmpty(networkType) &&
                            socialLinks.TryGetValue(networkType, out var existingLink) &&
                            string.IsNullOrEmpty(existingLink))
                        {
                            socialLinks[networkType] = link;
                            Console.WriteLine($"{networkType}: {link}");
                        }
                    }
                }

                return new Dictionary<string, object>
                {
                    ["email"] = emails.Count != 0 ? string.Join(", ", emails) : "",
                    ["cnpj"] = cnpj ?? "",
                    ["facebook"] = socialLinks["facebook"],
                    ["instagram"] = socialLinks["instagram"],
                    ["linkedin"] = socialLinks["linkedin"],
                    ["twitter"] = socialLinks["twitter"],
                    ["youtube"] = socialLinks["youtube"]
                };
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine($"Timeout para: {url}");
                return CreateEmptyResult();
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Erro HTTP para {url}: {e.Message}");
                return CreateEmptyResult();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Erro inesperado para {url}: {e.Message}");
                return CreateEmptyResult();
            }
        }

        private static Dictionary<string, object> CreateEmptyResult()
        {
            return new Dictionary<string, object>
            {
                ["email"] = "",
                ["cnpj"] = "",
                ["facebook"] = "",
                ["instagram"] = "",
                ["linkedin"] = "",
                ["twitter"] = "",
                ["youtube"] = ""
            };
        }
    }

}