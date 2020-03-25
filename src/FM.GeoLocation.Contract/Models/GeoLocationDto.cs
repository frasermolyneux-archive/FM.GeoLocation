namespace FM.GeoLocation.Contract.Models
{
    public class GeoLocationDto
    {
        public string Address { get; set; }
        public string TranslatedAddress { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}