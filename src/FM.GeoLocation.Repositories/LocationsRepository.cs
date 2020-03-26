using System;
using System.Threading.Tasks;
using FM.GeoLocation.Repositories.Models;
using Microsoft.Azure.Cosmos.Table;

namespace FM.GeoLocation.Repositories
{
    public interface ILocationsRepository
    {
        Task<GeoLocationEntity> StoreEntity(GeoLocationEntity entity);
        Task<GeoLocationEntity> GetGeoLocationEntity(string address);
    }

    public class LocationsRepository : ILocationsRepository
    {
        private readonly IPartitionKeyHelper _partitionKeyHelper;
        private readonly ITableStorageConfiguration _tableStorageConfiguration;

        public LocationsRepository(
            ITableStorageConfiguration tableStorageConfiguration,
            IPartitionKeyHelper partitionKeyHelper)
        {
            _tableStorageConfiguration = tableStorageConfiguration ??
                                         throw new ArgumentNullException(nameof(tableStorageConfiguration));
            _partitionKeyHelper = partitionKeyHelper ?? throw new ArgumentNullException(nameof(partitionKeyHelper));
        }

        public async Task<GeoLocationEntity> StoreEntity(GeoLocationEntity entity)
        {
            var storageAccount = CloudStorageAccount.Parse(_tableStorageConfiguration.TableStorageConnectionString);
            var tableClient = storageAccount.CreateCloudTableClient();
            var tableReference = tableClient.GetTableReference("locationsv2");
            await tableReference.CreateIfNotExistsAsync();

            var insertOrReplaceOperation = TableOperation.InsertOrReplace(entity);

            var result = await tableReference.ExecuteAsync(insertOrReplaceOperation);
            var insertedGeoLocationEntity = result.Result as GeoLocationEntity;

            return insertedGeoLocationEntity;
        }

        public async Task<GeoLocationEntity> GetGeoLocationEntity(string address)
        {
            var storageAccount = CloudStorageAccount.Parse(_tableStorageConfiguration.TableStorageConnectionString);
            var tableClient = storageAccount.CreateCloudTableClient();
            var tableReference = tableClient.GetTableReference("locationsv2");
            await tableReference.CreateIfNotExistsAsync();

            var retrieveTableOperation =
                TableOperation.Retrieve(_partitionKeyHelper.GetPartitionKeyFromAddress(address), address);

            var result = await tableReference.ExecuteAsync(retrieveTableOperation);

            var retrievedEntity = result.Result as GeoLocationEntity;

            return retrievedEntity;
        }
    }
}