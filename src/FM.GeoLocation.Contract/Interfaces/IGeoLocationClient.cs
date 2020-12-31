using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FM.GeoLocation.Contract.Models;

namespace FM.GeoLocation.Contract.Interfaces
{
    public interface IGeoLocationClient
    {
        Task<LookupAddressResponse> LookupAddress(string address);
        Task<LookupAddressBatchResponse> LookupAddressBatch(List<string> addresses);
        Task<RemoveDataForAddressResponse> RemoveDataForAddress(string address);
        Task<Tuple<bool, string>> HealthCheck();
    }
}