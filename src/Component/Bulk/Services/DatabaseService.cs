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

        public object? ExecuteScalarCommand(IDbTransaction dbTransaction, string command)
        {
            var sqlCommand = dbTransaction!.Connection!.CreateCommand();

            sqlCommand.Transaction = dbTransaction;
            sqlCommand.CommandText = command;

            var obj = sqlCommand.ExecuteScalar();

            return obj;
        }

        public void ExecuteNonQueryCommand(IDbTransaction dbTransaction, string command)
        {
            var sqlCommand = dbTransaction!.Connection!.CreateCommand();

            sqlCommand.Transaction = dbTransaction;
            sqlCommand.CommandText = command;

            sqlCommand.ExecuteNonQuery();
        }
    }
}
