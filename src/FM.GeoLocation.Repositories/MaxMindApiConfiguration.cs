using System;
using System.Configuration;

namespace FM.GeoLocation.Repositories
{
    public interface IMaxMindApiConfiguration
    {
        int UserId { get; }
        string ApiKey { get; }
    }

    public class MaxMindApiConfiguration : IMaxMindApiConfiguration
    {
        public int UserId
        {
            get
            {
                var userId = ConfigurationManager.AppSettings["MaxMindUserId"] ??
                             Environment.GetEnvironmentVariable("MaxMindUserId");

                return Convert.ToInt32(userId);
            }
        }

        public string ApiKey => ConfigurationManager.AppSettings["MaxMindApiKey"] ??
                                Environment.GetEnvironmentVariable("MaxMindApiKey");
    }
}