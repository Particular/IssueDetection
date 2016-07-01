using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace IndexPruner
{
    public static class Extensions
    {
        public const int DeleteRetryCount = 3;
        public static readonly TableEntity Empty = new TableEntity();

        /// <summary>
        /// Tries to delete an entity, returning entity if it fails and <see cref="Empty"/> if removal has been completed.
        /// </summary>
        public static Task<ITableEntity> TryDelete(this CloudTable table, ITableEntity entity)
        {
            entity.ETag = "*";
            return TryDelete(table, entity, DeleteRetryCount);
        }

        private static async Task DeleteIgnoringNotFound(this CloudTable table, ITableEntity entity)
        {
            try
            {
                await table.ExecuteAsync(TableOperation.Delete(entity)).ConfigureAwait(false);
            }
            catch (StorageException ex)
            {
                // Horrible logic to check if item has already been deleted or not
                var webException = ex.InnerException as WebException;
                if (webException?.Response != null)
                {
                    var response = (HttpWebResponse) webException.Response;
                    if ((int) response.StatusCode != 404)
                    {
                        throw;
                    }
                }
            }
        }

        private static async Task<ITableEntity> TryDelete(CloudTable table, ITableEntity entity, int retriesLeft)
        {
            if (retriesLeft == 0)
            {
                return entity;
            }

            try
            {
                await table.DeleteIgnoringNotFound(entity).ConfigureAwait(false);
                return Empty;
            }
            catch (Exception)
            {
                return await TryDelete(table, entity, retriesLeft - 1);
            }
        }
    }
}