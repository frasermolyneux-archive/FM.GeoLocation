using System.Threading.Tasks;
using FM.GeoLocation.Contract.Models;

namespace FM.GeoLocation.Client
{
    public interface IGeoLocationClient
    {
        Task<GeoLocationDto> LookupAddress(string address);
    }
}