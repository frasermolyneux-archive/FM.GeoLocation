using System.Collections.Generic;
using System.Threading.Tasks;
using FM.GeoLocation.Contract.Models;

namespace FM.GeoLocation.Client
{
    public interface IGeoLocationClient
    {
        Task<GeoLocationDto> LookupAddress(string address);
        Task<List<GeoLocationDto>> LookupAddressBatch(List<string> addresses);
    }
}