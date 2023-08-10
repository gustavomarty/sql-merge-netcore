using System.Data;

namespace Bulk.Services
{
    public interface IDatabaseService
    {
        Task PopulateTempTable<TEntity>(IDbTransaction dbTransaction, List<TEntity> dataSource, string tableName);
        object? ExecuteScalarCommand(IDbTransaction dbTransaction, string command);
        void ExecuteNonQueryCommand(IDbTransaction dbTransaction, string command);
    }
}
