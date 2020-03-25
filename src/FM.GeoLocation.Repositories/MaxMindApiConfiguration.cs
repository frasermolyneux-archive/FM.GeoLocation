using System;
using Microsoft.Extensions.Configuration;

namespace FM.GeoLocation.Repositories
{
    public interface IMaxMindApiConfiguration
    {
        int UserId { get; }
        string ApiKey { get; }
    }

    public class MaxMindApiConfiguration : IMaxMindApiConfiguration
    {
        private readonly IConfiguration _configuration;

        public MaxMindApiConfiguration(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public int UserId
        {
            get
            {
                var userId = _configuration["MaxMind:UserId"];

                return Convert.ToInt32(userId);
            }
        }

        public string ApiKey => _configuration["MaxMind:ApiKey"];
    }
}