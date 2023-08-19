using System.Data;

namespace SqlComplexOperations.Services
{
    public interface IDatabaseService
    {
        public Task PopulateTempTable<TEntity>(IDbTransaction dbTransaction, List<TEntity> dataSource, string tableName, string schema, List<string> columnOrder, bool isSnakeCase);
        public object? ExecuteScalarCommand(IDbTransaction dbTransaction, string command);
        List<string> ExecuteReaderCommand(IDbTransaction dbTransaction, string command);
        public void ExecuteNonQueryCommand(IDbTransaction dbTransaction, string command);
    }
}
