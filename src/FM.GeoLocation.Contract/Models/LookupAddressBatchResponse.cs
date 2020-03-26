using System.Collections.Generic;

namespace FM.GeoLocation.Contract.Models
{
    public class LookupAddressBatchResponse
    {
        public bool Success => ErrorMessage == null;

        public string ErrorMessage { get; set; } = null;

        public List<LookupAddressResponse> LookupAddressResponses { get; set; }
    }
}