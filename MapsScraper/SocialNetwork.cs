using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MapsScraper
{

    public class SocialNetwork
    {
        public string Name { get; set; }
        public string Exclusion { get; set; }
        public Regex Regex { get; set; }
        public Regex ValidatePattern { get; set; }

        public SocialNetwork(string name, string exclusion, Regex regex, Regex validatePattern)
        {
            Name = name;
            Exclusion = exclusion;
            Regex = regex;
            ValidatePattern = validatePattern;
        }

        public bool ValidateUrl(string url)
        {
            url = url.TrimEnd('/');
            return ValidatePattern.IsMatch(url);
        }

        public bool IsExcluded(string url)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(url, Exclusion, RegexOptions.IgnoreCase);
        }
    }

    public static class SocialNetworks
    {
        public static readonly Dictionary<string, SocialNetwork> Networks = new()
        {
            ["facebook"] = new SocialNetwork(
                name: "facebook",
                exclusion: @"(.*)(\.jpg|\.webp|\.png|\.php|\.gif|tr\?|2008\/fbml|whatsapp)",
                regex: new Regex(
                    @"(https?:\/\/(?:[a-z]{2,3}\.)?(?:[a-z]{2}(?:-[a-z]{2})?\.)?(?:m\.facebook|facebook|fb)\.com\/(?!.*\.php|tr\?|2008\/fbml)[-\w@:%._+~#=/\u00E7\u00F5]+)",
                    RegexOptions.IgnoreCase
                ),
                validatePattern: new Regex(
                    @"^(?:https?:)?(?:\/\/)?(?:[a-z]{2,3}\.)?(?:[a-z]{2}(?:-[a-z]{2})?\.)?(?:m\.facebook|facebook|fb)\.com\/(?!.*profile\.php|tr\?|2008\/fbml)[-\w%_.-\u00E7\u00F5/]+$",
                    RegexOptions.IgnoreCase
                )
            ),
            ["instagram"] = new SocialNetwork(
                name: "instagram",
                exclusion: @"(.*)(\.jpg|\.webp|\.png|\.php|\.cdn|\/v\/|whatsapp)",
                regex: new Regex(
                    @"(((https?:\/\/)|(\/\/))?(?:www\.)?instagram\.com\/[-\w@:%._+~#=/\u00E7\u00F5]*)",
                    RegexOptions.IgnoreCase
                ),
                validatePattern: new Regex(
                    @"^(?:https?:)?(?:\/\/)?(?:www\.)?instagram\.com\/[\w_.-/\u00E7\u00F5]+$",
                    RegexOptions.IgnoreCase
                )
            ),
            ["linkedin"] = new SocialNetwork(
                name: "linkedin",
                exclusion: @"(.*)(\.jpg|\.webp|\.png|\/company(?!\/))",
                regex: new Regex(
                    @"((https?:\/\/)?(?:[a-z]{2,3}\.)?linkedin\.com\/[-\w@:%._+~#=/]*)",
                    RegexOptions.IgnoreCase
                ),
                validatePattern: new Regex(
                    @"^(?:https?:)?(?:\/\/)?(?:[a-z]{2,3}\.)?linkedin\.com\/.*$",
                    RegexOptions.IgnoreCase
                )
            ),
            ["twitter"] = new SocialNetwork(
                name: "twitter",
                exclusion: @"(.*)(\.jpg|\.webp|\.png|\.php|\.cdn)",
                regex: new Regex(
                    @"(https?:\/\/)?(?:www\.)?twitter|x\.com\/[\w-]+",
                    RegexOptions.IgnoreCase
                ),
                validatePattern: new Regex(
                    @"(?:https?:)?(?:\/\/)?(?:www\.)?twitter|x\.com\/[\w-]+",
                    RegexOptions.IgnoreCase
                )
            ),
            ["youtube"] = new SocialNetwork(
                name: "youtube",
                exclusion: @"(.*)(watch|embed)",
                regex: new Regex(
                    @"(https?:\/{2})?(m\.|www\.)?(youtube\.com|youtu\.be\.com)\/[\w-]+",
                    RegexOptions.IgnoreCase
                ),
                validatePattern: new Regex(
                    @"(?:https?:)?(?:\/\/)?(?:www\.)?(?:youtube\.com|youtu\.be\.com)\/(?:user|channel)\/[\w-]+\/?$",
                    RegexOptions.IgnoreCase
                )
            ),
            ["tiktok"] = new SocialNetwork(
                name: "tiktok",
                exclusion: @"(.*)(\.jpg|\.webp|\.png|\/embed\/|\/v\/)",
                regex: new Regex(
                    @"(https?:\/\/)?(?:www\.)?tiktok\.com\/@[-\w.]+",
                    RegexOptions.IgnoreCase
                ),
                validatePattern: new Regex(
                    @"^(?:https?:)?(?:\/\/)?(?:www\.)?tiktok\.com\/@[-\w.]+$",
                    RegexOptions.IgnoreCase
                )
            )
        };

        public static string? GetSocialNetworkTypeFromUrl(string url)
        {
            foreach (var kvp in Networks)
            {
                var network = kvp.Value;
                if (network.ValidateUrl(url) && !network.IsExcluded(url))
                {
                    return kvp.Key;
                }
            }
            return null;
        }

        public static SocialNetwork? GetSocialNetworkFromUrl(string url)
        {
            var networkType = GetSocialNetworkTypeFromUrl(url);
            return networkType != null && Networks.TryGetValue(networkType, out SocialNetwork? value)
                ? value : null;
        }

        public static Regex? GetSocialNetworkRegex(string networkType)
        {
            return Networks.TryGetValue(networkType, out SocialNetwork? value)
                ? value.Regex
                : null;
        }
    }
}
