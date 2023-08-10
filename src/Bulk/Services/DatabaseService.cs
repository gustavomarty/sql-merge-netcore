using Bulk.Extensions;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Bulk.Services
{
    public class DatabaseService : IDatabaseService
    {
        public async Task PopulateTempTable<TEntity>(IDbTransaction dbTransaction, List<TEntity> dataSource, string tableName)
        {
            DataTable table = new()
            {
                TableName = tableName
            };

            using var bulkInsert = new SqlBulkCopy(dbTransaction.Connection as SqlConnection, SqlBulkCopyOptions.Default, dbTransaction as SqlTransaction);
            bulkInsert.DestinationTableName = table.TableName;

            using var dataReader = new ObjectDataReader<TEntity>(dataSource.GetEnumerator());
            await bulkInsert.WriteToServerAsync(dataReader);
        }

        public string GetPrimaryKeyByTableName(IDbTransaction dbTransaction, string tableName)
        {
            var query = SqlBuilder.BuildPrimaryKeyQuery(tableName);

            var sqlCommand = dbTransaction!.Connection!.CreateCommand();

            sqlCommand.Transaction = dbTransaction;
            sqlCommand.CommandText = query;

            var pkField = sqlCommand.ExecuteScalar();

            return pkField?.ToString() ?? string.Empty;
        }
    }
}
