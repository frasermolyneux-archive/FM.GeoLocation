using System;
using System.Reflection;
using System.Threading.Tasks;
using FM.GeoLocation.Client;
using Microsoft.Extensions.Configuration;

namespace FM.GeoLocation.ClientTestHarness
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddUserSecrets(Assembly.GetExecutingAssembly(), false)
                .Build();


            var geoLocationClientConfiguration = new GeoLocationClientConfiguration(config);
            var geoLocationClient = new GeoLocationClient(geoLocationClientConfiguration, null);

            await geoLocationClient.LookupAddress("162.25.35.21");

            Console.ReadKey();
        }
    }
}