using System;
using System.Collections.Generic;

namespace FM.GeoLocation.Client
{
    public interface IGeoLocationClientConfiguration
    {
        string BaseUrl { get; }
        string ApiKey { get; }
        bool UseMemoryCache { get; }
        DateTime CacheEntryLife { get; }
        IEnumerable<TimeSpan> RetryTimespans { get; }
    }
}