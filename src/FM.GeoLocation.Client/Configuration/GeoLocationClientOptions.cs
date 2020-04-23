using System;
using System.Collections.Generic;
using FM.GeoLocation.Contract.Interfaces;

namespace FM.GeoLocation.Client.Configuration
{
    internal class GeoLocationClientOptions : IGeoLocationClientOptions
    {
        public string BaseUrl { get; set; }
        public string ApiKey { get; set; }
        public bool UseMemoryCache { get; set; }
        public int CacheEntryLifeInMinutes { get; set; }
        public IEnumerable<TimeSpan> RetryTimespans { get; set; }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(BaseUrl))
                throw new NullReferenceException(nameof(BaseUrl));

            if (string.IsNullOrWhiteSpace(ApiKey))
                throw new NullReferenceException(nameof(ApiKey));
        }
    }
}