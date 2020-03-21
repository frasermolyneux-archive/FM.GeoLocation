using System.Net;

namespace FM.GeoLocation.Repositories
{
    public interface IPartitionKeyHelper
    {
        string GetPartitionKeyFromAddress(string address);
    }

    public class PartitionKeyHelper : IPartitionKeyHelper
    {
        public string GetPartitionKeyFromAddress(string address)
        {
            return IPAddress.TryParse(address, out var ipAddress) ? address[0].ToString() : "DNS";
        }
    }
}