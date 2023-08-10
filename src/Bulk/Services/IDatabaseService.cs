using System.Data;

namespace Bulk.Services
{
    public interface IDatabaseService
    {
        Task PopulateTempTable<TEntity>(IDbTransaction dbTransaction, List<TEntity> dataSource, string tableName);
        string GetPrimaryKeyByTableName(IDbTransaction dbTransaction, string tableName);
    }
}
