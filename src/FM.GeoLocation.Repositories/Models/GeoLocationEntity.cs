using System.Collections.Generic;
using Microsoft.Azure.Cosmos.Table;

namespace FM.GeoLocation.Repositories.Models
{
    public class GeoLocationEntity : TableEntity
    {
        public GeoLocationEntity(string type, string address)
        {
            PartitionKey = type;
            RowKey = address;
        }

        public string ContinentCode { get; set; }
        public string ContinentName { get; set; }
        public string CountryCode { get; set; }
        public string CountryName { get; set; }
        public bool IsEuropeanUnion { get; set; }
        public string CityName { get; set; }
        public string PostalCode { get; set; }
        public string RegisteredCountry { get; set; }
        public string RepresentedCountry { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int AccuracyRadius { get; set; }
        public string Timezone { get; set; }
        public Dictionary<string, string> Traits { get; set; }
    }
}