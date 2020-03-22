using FM.GeoLocation.Contract.Models;

namespace FM.GeoLocation.Client
{
    public interface IGeoLocationClient
    {
        GeoLocationDto LookupAddress(string address);
    }
}