using GoogleMapsScraper.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleMapsScraper.Mapper
{
    internal class DataMapper
    {
        public static LeadsData MapToLeadsData(BusinessRecord record)
        {
            return new LeadsData
            {
                Name = record.Name,
                Categories = record.Categories,
                Email = record.Email,
                Phone = record.Phone,
                Url = record.Url,
                Domain = record.Domain,
                Facebook = record.Facebook,
                Instagram = record.Instagram,
                Youtube = record.Youtube,
                Tiktok = record.Tiktok,
                Twitter = record.Twitter,
                Cnpj = record.Cnpj,
                Rating = "6"
            };
        }
    }
}
