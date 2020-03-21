using Microsoft.Azure.Cosmos.Table;

namespace FM.GeoLocation.Repositories.Models
{
    public class GeoLocationEntity : TableEntity
    {
        public GeoLocationEntity(string type, string address, double latitude, double longitude, string country,
            string city)
        {
            PartitionKey = type;
            RowKey = address;
            Latitude = latitude;
            Longitude = longitude;
            Country = country;
            City = city;
        }

        public double Latitude { get; }
        public double Longitude { get; }
        public string Country { get; }
        public string City { get; }
    }
}