using System.Collections.Generic;

namespace FM.GeoLocation.Contract.Models
{
    public class GeoLocationDto
    {
        public string Address { get; set; } = string.Empty;
        public string TranslatedAddress { get; set; } = string.Empty;

        public string ContinentCode { get; set; } = string.Empty;
        public string ContinentName { get; set; } = string.Empty;
        public string CountryCode { get; set; } = string.Empty;
        public string CountryName { get; set; } = string.Empty;
        public bool IsEuropeanUnion { get; set; }
        public string CityName { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string RegisteredCountry { get; set; } = string.Empty;
        public string RepresentedCountry { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int AccuracyRadius { get; set; }
        public string Timezone { get; set; } = string.Empty;
        public Dictionary<string, string> Traits { get; set; } = new Dictionary<string, string>();
    }
}