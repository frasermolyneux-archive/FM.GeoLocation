using System.Linq;
using System.Net;

namespace FM.GeoLocation.FuncApp
{
    public interface IAddressHelper
    {
        bool ConvertAddress(string address, out string validatedAddress);
    }

    public class AddressHelper : IAddressHelper
    {
        public bool ConvertAddress(string address, out string validatedAddress)
        {
            if (IPAddress.TryParse(address, out var ipAddress))
            {
                validatedAddress = ipAddress.ToString();
                return true;
            }

            try
            {
                var hostEntry = Dns.GetHostEntry(address);

                if (hostEntry.AddressList.FirstOrDefault() != null)
                {
                    validatedAddress = hostEntry.AddressList.First().ToString();
                    return true;
                }
            }
            catch
            {
                validatedAddress = null;
                return false;
            }

            validatedAddress = null;
            return false;
        }
    }
}