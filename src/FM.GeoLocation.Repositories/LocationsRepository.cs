using System;
using System.Threading.Tasks;
using FM.GeoLocation.Repositories.Models;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Options;

namespace FM.GeoLocation.Repositories
{
    public class LocationsRepository : ILocationsRepository
    {
        private readonly IPartitionKeyHelper _partitionKeyHelper;

        public LocationsRepository(
            IOptions<AppDataOptions> options,
            IPartitionKeyHelper partitionKeyHelper)
        {
            var storageAccount = CloudStorageAccount.Parse(options.Value.StorageConnectionString);
            var cloudTableClient = storageAccount.CreateCloudTableClient();

            LocationsTable = cloudTableClient.GetTableReference(options.Value.LocationsTableName);
            LocationsTable.CreateIfNotExists();

            _partitionKeyHelper = partitionKeyHelper ?? throw new ArgumentNullException(nameof(partitionKeyHelper));
        }

        public CloudTable LocationsTable { get; }

        public async Task<GeoLocationEntity> StoreEntity(GeoLocationEntity entity)
        {
            var insertOrReplaceOperation = TableOperation.InsertOrReplace(entity);

            var result = await LocationsTable.ExecuteAsync(insertOrReplaceOperation);
            var insertedGeoLocationEntity = (GeoLocationEntity) result.Result;

            return insertedGeoLocationEntity;
        }

        public async Task<GeoLocationEntity> GetGeoLocationEntity(string address)
        {
            var retrieveTableOperation =
                TableOperation.Retrieve<GeoLocationEntity>(_partitionKeyHelper.GetPartitionKeyFromAddress(address),
                    address);

            var result = await LocationsTable.ExecuteAsync(retrieveTableOperation);

            var retrievedEntity = result.Result as GeoLocationEntity;

            return retrievedEntity;
        }

        public async Task RemoveGeoLocationEntity(GeoLocationEntity entity)
        {
            var deleteTableOperation = TableOperation.Delete(entity);

            await LocationsTable.ExecuteAsync(deleteTableOperation);
        }

        public async Task CreateTablesIfNotExist()
        {
            await LocationsTable.CreateIfNotExistsAsync();
        }
    }
}