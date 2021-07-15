using System.Threading.Tasks;
using FM.GeoLocation.Repositories.Models;

namespace FM.GeoLocation.Repositories
{
    public interface ILocationsRepository
    {
        Task<GeoLocationEntity> StoreEntity(GeoLocationEntity entity);
        Task<GeoLocationEntity> GetGeoLocationEntity(string address);
        Task RemoveGeoLocationEntity(GeoLocationEntity entity);
    }
}