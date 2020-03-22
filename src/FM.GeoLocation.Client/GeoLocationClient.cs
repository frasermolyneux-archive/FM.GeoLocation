using FM.GeoLocation.Contract.Models;

namespace FM.GeoLocation.Client
{
    public class GeoLocationClient : IGeoLocationClient
    {
        public GeoLocationDto LookupAddress(string address)
        {
            return new GeoLocationDto();
        }
    }
}