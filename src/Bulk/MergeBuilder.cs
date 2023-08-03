using Bulk.Extensions;
using Bulk.Models;
using Bulk.Models.Enumerators;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Linq.Expressions;

namespace Bulk
{
    public class MergeBuilder<TEntity> where TEntity : class
    {
        private string _tableName;

        private string StatusColumn { get; set; } = string.Empty;
        private string PrimaryKey { get; set; } = string.Empty;
        private bool SnakeCaseNamingConvention { get; set; }

        private List<TEntity> DataSource { get; set; } = new();
        private IDbTransaction? DbTransaction { get; set; }

        private List<string> AllColumns { get; set; } = new();
        private List<string> MergedColumns { get; set; } = new();
        private List<string> UpdatedColumns { get; set; } = new();
        private List<string> IgnoredOnInsertOperation { get; set; } = new();

        private List<ConditionBuilder> Conditions { get; set; } = new();

        public MergeBuilder()
        {
            _tableName = typeof(TEntity).Name;
        }

        public MergeBuilder<TEntity> UseSnakeCaseNamingConvention()
        {
            SnakeCaseNamingConvention = true;
            return this;
        }

        public MergeBuilder<TEntity> UseStatusConfiguration(Expression<Func<TEntity, object>> expression)
        {
            StatusColumn = expression.Body.Type.GetProperties().Select(m => m.Name).First();
            return this;
        }

        public MergeBuilder<TEntity> SetMergeColumns(params Expression<Func<TEntity, object>>[] expressions)
        {
            MergedColumns.AddRange(GetColumns(expressions));
            return this;
        }

        public MergeBuilder<TEntity> SetUpdatedColumns(params Expression<Func<TEntity, object>>[] expressions)
        {
            UpdatedColumns.AddRange(GetColumns(expressions));
            return this;
        }

        public MergeBuilder<TEntity> SetConditions<TConditionType, TConditionOperator>(TConditionType conditionType, TConditionOperator conditionOperator, params Expression<Func<TEntity, object>>[] expressions)
            where TConditionType : Enum
            where TConditionOperator : Enum
        {
            var cTypeValue = (ConditionTypes)Enum.Parse(typeof(TConditionType), conditionType.ToString());
            var cOperatorValue = (ConditionOperator)Enum.Parse(typeof(TConditionOperator), conditionOperator.ToString());
            var columns = GetColumns(expressions);

            var condition = new ConditionBuilder(columns ?? new List<string>(), cTypeValue, cOperatorValue);
            Conditions.Add(condition);

            return this;
        }

        public MergeBuilder<TEntity> SetIgnoreOnIsertOperation(params Expression<Func<TEntity, object>>[] expressions)
        {
            IgnoredOnInsertOperation.AddRange(GetColumns(expressions));
            return this;
        }

        private void SetAllColumns()
        {
            var properties = typeof(TEntity).GetProperties();
            properties = properties.Where(x => !x.GetGetMethod()?.IsVirtual ?? false).ToArray();

            var names = properties.Select(x => x.Name);

            if(SnakeCaseNamingConvention)
                names = names.Select(x => x.ToSnakeCase());

            AllColumns.AddRange(names);
        }

        private void SetPrimaryKeyColumn(IDbConnection dbConnection)
        {
            var query = SqlBuilder.BuildPrimaryKeyQuery(_tableName);

            var sqlCommand = dbConnection.CreateCommand();

            sqlCommand.Transaction = DbTransaction;
            sqlCommand.CommandText = query;

            var pkField = sqlCommand.ExecuteScalar();

            PrimaryKey = pkField?.ToString() ?? string.Empty;

            if(SnakeCaseNamingConvention)
                PrimaryKey = PrimaryKey.ToSnakeCase();
        }

        public MergeBuilder<TEntity> SetDataSource(List<TEntity> datasource)
        {
            DataSource = datasource;
            return this;
        }

        public MergeBuilder<TEntity> SetTransaction(IDbTransaction transaction)
        {
            if(transaction == null || transaction.Connection == null)
                throw new Exception("");

            DbTransaction = transaction;
            return this;
        }

        public string Execute()
        {
            if(DbTransaction == null || DbTransaction.Connection == null)
                throw new Exception("");

            SetAllColumns();
            SetPrimaryKeyColumn(DbTransaction.Connection);

            CheckSnakeCaseOnExecuteCommand();

            CreateTempTable(DbTransaction.Connection);
            PopulateTempTable(DbTransaction.Connection);
            ExecuteMergeCommand(DbTransaction.Connection);

            DropTempTable(DbTransaction.Connection);

            return "Deu boa!!";
        }

        private void ExecuteMergeCommand(IDbConnection dbConnection)
        {
            var allColumnsWithoutIgnoredInsert = AllColumns.Except(IgnoredOnInsertOperation).ToList();
            var allColumnsWithoutIgnoredUpdate = UpdatedColumns.Where(x => !x.Equals(PrimaryKey, StringComparison.OrdinalIgnoreCase)).ToList();

            var stringBuilderQuery = SqlBuilder.BuildMerge(_tableName, MergedColumns, allColumnsWithoutIgnoredUpdate, allColumnsWithoutIgnoredInsert, Conditions, StatusColumn);
            var sqlCommand = dbConnection.CreateCommand();

            sqlCommand.Transaction = DbTransaction;
            sqlCommand.CommandText = stringBuilderQuery.ToString();

            sqlCommand.ExecuteNonQuery();
        }

        private void CheckSnakeCaseOnExecuteCommand()
        {
            if(!SnakeCaseNamingConvention)
                return;

            _tableName = _tableName.ToSnakeCase();
            StatusColumn = StatusColumn.ToSnakeCase();

            IgnoredOnInsertOperation = IgnoredOnInsertOperation.Select(x => x.ToSnakeCase()).ToList();
            UpdatedColumns = UpdatedColumns.Select(x => x.ToSnakeCase()).ToList();
            MergedColumns = MergedColumns.Select(x => x.ToSnakeCase()).ToList();

            Conditions = Conditions.Select(x => {
                var fields = x.Fields.Select(y => y.ToSnakeCase()).ToList();
                return new ConditionBuilder(fields, x.ConditionType, x.ConditionOperator);
            }).ToList();
        }

        private void CreateTempTable(IDbConnection dbConnection)
        {
            var sqlCommand = dbConnection.CreateCommand();

            sqlCommand.Transaction = DbTransaction;
            sqlCommand.CommandText = SqlBuilder.BuildTempTable(_tableName);

            sqlCommand.ExecuteNonQuery();
        }

        private void PopulateTempTable(IDbConnection dbConnection)
        {
            DataTable table = new()
            {
                TableName = $"#{_tableName}"
            };

            using var bulkInsert = new SqlBulkCopy(dbConnection as SqlConnection, SqlBulkCopyOptions.Default, DbTransaction as SqlTransaction);
            bulkInsert.DestinationTableName = table.TableName;

            using var dataReader = new ObjectDataReader<TEntity>(DataSource.GetEnumerator());
            bulkInsert.WriteToServer(dataReader);
        }

        private void DropTempTable(IDbConnection dbConnection)
        {
            var sqlCommand = dbConnection.CreateCommand();

            sqlCommand.Transaction = DbTransaction;
            sqlCommand.CommandText = SqlBuilder.BuildDropTempTable(_tableName);

            sqlCommand.ExecuteNonQuery();
        }

        private static List<string> GetColumns(params Expression<Func<TEntity, object>>[] expressions)
        {
            var names = ExpressionExtension.GetMemberNames(expressions);
            if (names != null && names.Any())
                return names;

            return new();
        }
    }
}