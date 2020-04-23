namespace FM.GeoLocation.Contract.Interfaces
{
    public interface IAddressValidator
    {
        bool ConvertAddress(string address, out string validatedAddress);
    }
}