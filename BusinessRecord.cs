using System;

using System;
using System.Collections.Generic;
using System.Linq;

namespace MyApp.Models
{
    public static class Utils
    {
        // Equivalent to Python: Utils.remove_query_and_fragment(url)
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
    }

    public static class SafeAccess
    {
        // Equivalent to safe_get(data, indices...)
        public static object SafeGet(object data, params int[] indices)
        {
            try
            {
                if (data is not List<object> list)
                    return null;

                object currentData = list[1]; // same as data[1] in Python

                foreach (var index in indices)
                {
                    if (currentData is List<object> innerList)
                    {
                        if (index < 0 || index >= innerList.Count)
                            return null;
                        currentData = innerList[index];
                    }
                    else
                    {
                        return null;
                    }
                }

                return currentData;
            }
            catch
            {
                return null;
            }
        }
    }

    public class BusinessRecord
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public string Email { get; set; } = "";
        public string Facebook { get; set; } = "";
        public string Instagram { get; set; } = "";
        public string Linkedin { get; set; } = "";
        public string Twitter { get; set; } = "";
        public string Youtube { get; set; } = "";
        public string Tiktok { get; set; } = "";
        public string Domain { get; set; }
        public string FullAddr { get; set; }
        public string Categories { get; set; }
        public string LocalName { get; set; }
        public string LocalFullAddr { get; set; }
        public string Phone { get; set; }
        public string Cnpj { get; set; } = "";
        public string CreatedAt { get; set; } = "";
        public bool Processed { get; set; } = false;
        public object Key { get; set; }

        public BusinessRecord(object rawData)
        {
            // Converte de forma segura
            var data = rawData as List<object> ?? new List<object>();

            Name = data.ElementAtOrDefault(11)?.ToString();

            var list7 = data.ElementAtOrDefault(7) as List<object>;
            Url = list7?.ElementAtOrDefault(0)?.ToString();
            Domain = list7?.ElementAtOrDefault(1)?.ToString();

            if (!string.IsNullOrEmpty(Url))
                Url = Utils.RemoveQueryAndFragment(Url);

            FullAddr = data.ElementAtOrDefault(39)?.ToString();
            LocalName = data.ElementAtOrDefault(101)?.ToString();
            LocalFullAddr = data.ElementAtOrDefault(149)?.ToString();

            var categories = data.ElementAtOrDefault(13) as List<object>;
            Categories = categories != null ? string.Join(", ", categories.Select(c => c.ToString())) : "";

            var phones = data.ElementAtOrDefault(178) as List<object>;
            var phoneList = phones?
                .OfType<List<object>>()
                .Select(inner => inner.FirstOrDefault()?.ToString())
                .Where(p => !string.IsNullOrEmpty(p))
                .ToList();
            Phone = phoneList != null ? string.Join(", ", phoneList) : "";

            Key = data.ElementAtOrDefault(78);
        }

        // Equivalent to to_dict()
        public Dictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>
            {
                ["name"] = Name,
                ["url"] = Url,
                ["email"] = Email,
                ["facebook"] = Facebook,
                ["instagram"] = Instagram,
                ["linkedin"] = Linkedin,
                ["twitter"] = Twitter,
                ["youtube"] = Youtube,
                ["tiktok"] = Tiktok,
                ["domain"] = Domain,
                ["fulladdr"] = FullAddr,
                ["categories"] = Categories,
                ["local_name"] = LocalName,
                ["local_fulladdr"] = LocalFullAddr,
                ["phone"] = Phone,
                ["cnpj"] = Cnpj,
                ["created_at"] = CreatedAt
            };
        }

        // Equivalent to is_valid()
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(Name) || !string.IsNullOrEmpty(FullAddr);
        }
    }
}


