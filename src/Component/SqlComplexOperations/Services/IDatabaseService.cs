using SqlComplexOperations.Models;
using System.Data;

namespace SqlComplexOperations.Services
{
    public interface IDatabaseService
    {
        public Task PopulateTempTable<TEntity>(IDbTransaction dbTransaction, List<TEntity> dataSource, string tableName, string schema, List<string> columnOrder, bool isSnakeCase);
        OutputModel ExecuteMergeCommand(IDbTransaction dbTransaction, string command);
        OutputModelWithData<T> ExecuteMergeCommand<T>(IDbTransaction dbTransaction, string command, List<string> columns, bool isSnakeCase) where T : class;
        public object? ExecuteScalarCommand(IDbTransaction dbTransaction, string command);
        List<string> ExecuteReaderCommand(IDbTransaction dbTransaction, string command);
        public void ExecuteNonQueryCommand(IDbTransaction dbTransaction, string command);
    }
}
