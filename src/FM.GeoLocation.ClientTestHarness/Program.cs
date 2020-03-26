using System;
using System.Collections.Generic;
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

            var value = await geoLocationClient.LookupAddress("162.65.65.65");

            var addresses = new string[] {"google.com", "sky.com", "bbc.co.uk"};
            var batchValue = await  geoLocationClient.LookupAddressBatch(new List<string>(addresses));

            Console.ReadKey();
        }
    }
}