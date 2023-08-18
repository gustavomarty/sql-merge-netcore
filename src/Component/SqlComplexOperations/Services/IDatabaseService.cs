using System.Data;

namespace SqlComplexOperations.Services
{
    public interface IDatabaseService
    {
        public Task PopulateTempTable<TEntity>(IDbTransaction dbTransaction, List<TEntity> dataSource, string tableName);
        public object? ExecuteScalarCommand(IDbTransaction dbTransaction, string command);
        public void ExecuteNonQueryCommand(IDbTransaction dbTransaction, string command);
    }
}
