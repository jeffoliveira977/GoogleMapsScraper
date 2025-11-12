using GoogleMapsScraper.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace GoogleMapsScraper.Model
{
        public class BusinessRecord
        {
            public string? SearchId { get; set; }
            public string? Name { get; set; }
            public string? Url { get; set; }
            public string Email { get; set; } = "";
            public string Facebook { get; set; } = "";
            public string Instagram { get; set; } = "";
            public string Linkedin { get; set; } = "";
            public string Twitter { get; set; } = "";
            public string Youtube { get; set; } = "";
            public string Tiktok { get; set; } = "";
            public string? Domain { get; set; }
            public string? FullAddr { get; set; }
            public string? Categories { get; set; }
            public string? LocalName { get; set; }
            public string? LocalFullAddr { get; set; }
            public string? Phone { get; set; }
            public string Cnpj { get; set; } = "";
            public string CreatedAt { get; set; } = "";
            public bool Processed { get; set; } = false;
            public object? Key { get; set; }

            public BusinessRecord()
            {
           
            }

            public BusinessRecord(JsonElement data)
            {
                SearchId = string.Empty;

                Name = SafeGet(data, 11).ToString() ?? string.Empty;
                Url = SafeGet(data, 7, 0).ToString() ?? string.Empty;

                if (!string.IsNullOrEmpty(Url))
                {
                    Url = Helper.RemoveQueryAndFragment(Url);
                }

                Email = string.Empty;
                Facebook = string.Empty;
                Instagram = string.Empty;
                Linkedin = string.Empty;
                Twitter = string.Empty;
                Youtube = string.Empty;
                Tiktok = string.Empty;

                Domain = SafeGet(data, 7, 1).ToString() ?? string.Empty;
                FullAddr = SafeGet(data, 39).ToString() ?? string.Empty;

                LocalName = SafeGet(data, 101).ToString() ?? string.Empty;
                LocalFullAddr = SafeGet(data, 149).ToString() ?? string.Empty;

                var categoriesData = SafeGet(data, 13);
                List<string>? categories = null;
                if (categoriesData.ValueKind == JsonValueKind.Array)
                {
                    categories = [.. categoriesData.EnumerateArray().Select(item => item.ToString())];
                }
                Categories = categories != null ? string.Join(", ", categories) : string.Empty;

                var phonesData = SafeGet(data, 178);
                List<string>? phone = null;
                if (phonesData.ValueKind == JsonValueKind.Array)
                {
                    phone = [.. phonesData.EnumerateArray().Select(item => item[0].ToString())];
                }
                Phone = phone != null ? string.Join(", ", phone) : string.Empty;

                Cnpj = string.Empty;
                CreatedAt = string.Empty;
                Processed = false;
                Key = SafeGet(data, 78);
            }

            private static JsonElement SafeGet(JsonElement data, params int[] indices)
            {
                try
                {
                    var currentData = data[1];

                    foreach (var index in indices)
                    {
                        currentData = currentData[index];
                    }

                    return currentData;
                }
                catch (Exception)
                {
                    return default;
                }
            }

            public bool IsValid()
            {
                return !string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(Url);
            }

            public Dictionary<string, object> ToDictionary()
            {
                return new Dictionary<string, object>
                {
                    ["name"] = Name ?? "",
                    ["url"] = Url ?? "",
                    ["email"] = Email,
                    ["facebook"] = Facebook,
                    ["instagram"] = Instagram,
                    ["linkedin"] = Linkedin,
                    ["twitter"] = Twitter,
                    ["youtube"] = Youtube,
                    ["tiktok"] = Tiktok,
                    ["domain"] = Domain ?? "",
                    ["fulladdr"] = FullAddr ?? "",
                    ["categories"] = Categories ?? "",
                    ["local_name"] = LocalName ?? "",
                    ["local_fulladdr"] = LocalFullAddr ?? "",
                    ["phone"] = Phone ?? "",
                    ["cnpj"] = Cnpj,
                    ["created_at"] = CreatedAt
                };
            }
        }
    
}