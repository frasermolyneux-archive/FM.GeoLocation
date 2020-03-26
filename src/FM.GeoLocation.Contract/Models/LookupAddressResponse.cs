namespace FM.GeoLocation.Contract.Models
{
    public class LookupAddressResponse
    {
        public bool Success => ErrorMessage == null;

        public string ErrorMessage { get; set; } = null;

        public GeoLocationDto GeoLocationDto { get; set; }
    }
}