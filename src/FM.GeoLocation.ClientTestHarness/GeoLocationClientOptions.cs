using System;
using System.Collections.Generic;
using FM.GeoLocation.Contract.Interfaces;
using Microsoft.Extensions.Configuration;

namespace FM.GeoLocation.ClientTestHarness
{
    internal class GeoLocationClientOptions : IGeoLocationClientOptions
    {
        public GeoLocationClientOptions(IConfiguration configuration)
        {
            BaseUrl = configuration["GeoLocationService:BaseUrl"];
            ApiKey = configuration["GeoLocationService:ApiKey"];
            UseMemoryCache = true;
            BubbleExceptions = true;
            CacheEntryLifeInMinutes = 60;
            var random = new Random();
            RetryTimespans = new[]
            {
                TimeSpan.FromSeconds(random.Next(1)),
                TimeSpan.FromSeconds(random.Next(3)),
                TimeSpan.FromSeconds(random.Next(5))
            };
        }

        public string BaseUrl { get; set; }
        public string ApiKey { get; set; }
        public bool UseMemoryCache { get; set; }
        public bool BubbleExceptions { get; set; }
        public int CacheEntryLifeInMinutes { get; set; }
        public IEnumerable<TimeSpan> RetryTimespans { get; set; }
    }
}