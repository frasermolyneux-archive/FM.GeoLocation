using System;
using System.Collections.Generic;

namespace FM.GeoLocation.Contract.Interfaces
{
    public interface IGeoLocationClientOptions
    {
        string BaseUrl { get; set; }
        string ApiKey { get; set; }
        bool UseMemoryCache { get; set; }
        int CacheEntryLifeInMinutes { get; set; }
        IEnumerable<TimeSpan> RetryTimespans { get; set; }
    }
}