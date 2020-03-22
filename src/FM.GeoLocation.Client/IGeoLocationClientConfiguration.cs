using System;
using System.Collections.Generic;

namespace FM.GeoLocation.Client
{
    public interface IGeoLocationClientConfiguration
    {
        string BaseUrl { get; }
        string ApiKey { get; }
        bool UseMemoryCache { get; }
        int CacheEntryLifeInMinutes { get; }
        IEnumerable<TimeSpan> RetryTimespans { get; }
    }
}