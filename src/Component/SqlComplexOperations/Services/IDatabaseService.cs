using SqlComplexOperations.Models.Output;
using System.Data;

namespace SqlComplexOperations.Services
{
    public interface IDatabaseService
    {
        public Task BulkInsert<TEntity>(IDbTransaction dbTransaction, List<TEntity> dataSource, string tableName, string schema, List<string> columnOrder, bool isSnakeCase, bool propNameAttr);
        OutputModel ExecuteMergeCommand(IDbTransaction dbTransaction, string command);
        OutputModelRowCount ExecuteMergeCommandRowCount(IDbTransaction dbTransaction, string command);
        OutputModelSimple ExecuteMergeCommandSimple(IDbTransaction dbTransaction, string command);
        OutputModelComplete<T> ExecuteMergeCommandComplete<T>(IDbTransaction dbTransaction, string command, List<string> columns, bool isSnakeCase, bool propNameAttr) where T : class;
        public object? ExecuteScalarCommand(IDbTransaction dbTransaction, string command);
        List<string> ExecuteReaderCommand(IDbTransaction dbTransaction, string command);
        public void ExecuteNonQueryCommand(IDbTransaction dbTransaction, string command);
    }
}
