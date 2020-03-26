using System;
using System.Collections.Generic;

namespace FM.GeoLocation.Contract.Models
{
    public class GeoLocationDto
    {
        public string Address { get; set; }
        public string TranslatedAddress { get; set; }
        
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