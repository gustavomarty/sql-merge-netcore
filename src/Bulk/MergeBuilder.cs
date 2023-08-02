using System.Data;
using System.Linq.Expressions;
using Bulk.Models.Enumerators;
using Microsoft.Data.SqlClient;
using System.Runtime.CompilerServices;

namespace Bulk
{
    public class MergeBuilder<TEntity> where TEntity : class
    {
        private readonly string _tableName;

        private List<string> AllColumns { get; set; } = new();
        private List<string> MergedColumns { get; set; } = new();
        private List<string> UpdatedColumns { get; set; } = new();
        private List<string> IgnoredOnInsertOperation { get; set; } = new();
        private List<(string field, ConditionTypes op)> Conditions { get; set; } = new();
        private string StatusColumn { get; set; } = string.Empty;
        private List<TEntity> DataSource { get; set; } = new();
        private IDbTransaction? DbTransaction { get; set; }

        public MergeBuilder()
        {
            _tableName = typeof(TEntity).Name;
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

        public MergeBuilder<TEntity> SetIgnoreOnIsertOperation(params Expression<Func<TEntity, object>>[] expressions)
        {
            IgnoredOnInsertOperation.AddRange(GetColumns(expressions));
            return this;
        }

        private void SetAllColumns()
        {
            var names = typeof(TEntity).GetProperties().Select(x => x.Name);
            AllColumns.AddRange(names);
        }

        public MergeBuilder<TEntity> SetDataSource(List<TEntity> datasource)
        {
            DataSource = datasource;
            return this;
        }

        public MergeBuilder<TEntity> SetTransaction(IDbTransaction transaction)
        {
            if(transaction == null || transaction.Connection == null)
            {
                throw new Exception("");
            }

            DbTransaction = transaction;
            return this;
        }

        public string Execute()
        {
            if(DbTransaction == null || DbTransaction.Connection == null)
            {
                throw new Exception("");
            }

            SetAllColumns();

            CreateTempTable(DbTransaction.Connection);
            PopulateTempTable(DbTransaction.Connection);
            ExecuteMergeCommand(DbTransaction.Connection);

            return "Deu boa!!";
        }

        private void ExecuteMergeCommand(IDbConnection dbConnection)
        {
            var allColumnsWithoutIgnoredInsert = AllColumns.Except(IgnoredOnInsertOperation).ToList();

            var stringBuilderQuery = SqlBuilder.BuildMerge(_tableName, MergedColumns, UpdatedColumns, allColumnsWithoutIgnoredInsert, Conditions, StatusColumn);
            var sqlCommand = dbConnection.CreateCommand();

            sqlCommand.Transaction = DbTransaction;
            sqlCommand.CommandText = stringBuilderQuery.ToString();

            sqlCommand.ExecuteNonQuery();
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
                TableName = _tableName
            };

            using var bulkInsert = new SqlBulkCopy(dbConnection as SqlConnection, SqlBulkCopyOptions.Default, DbTransaction as SqlTransaction);
            bulkInsert.DestinationTableName = table.TableName;

            using var dataReader = new ObjectDataReader<TEntity>(DataSource.GetEnumerator());
            bulkInsert.WriteToServer(dataReader);
        }

        private static List<string> GetColumns(params Expression<Func<TEntity, object>>[] expressions)
        {
            var names = GetMemberNames(expressions);
            if (names != null && names.Any())
            {
                return names;
            }

            return new ();
        }

        private static List<string> GetMemberNames<T>(params Expression<Func<T, object>>[] expressions)
        {
            if (expressions.Length == 1 && IsAnonymousType(expressions.First().Body.Type))
            {
                return expressions.First().Body.Type.GetProperties().Select(m => m.Name).ToList();
            }

            return expressions.Select(expression => GetMemberName(expression.Body)).ToList();
        }

        private static bool IsAnonymousType(Type type)
        {
            var hasCompilerGeneratedAttribute = type.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Any();
            var nameContainsAnonymousType = type.FullName != null && type.FullName.Contains("AnonymousType");
            return hasCompilerGeneratedAttribute && nameContainsAnonymousType;
        }

        private static string GetMemberName(Expression expression)
        {
            return expression switch
            {
                null => throw new ArgumentException(""),
                MemberExpression memberExpression => memberExpression.Member.Name,
                MethodCallExpression methodCallExpression => methodCallExpression.Method.Name,
                UnaryExpression unaryExpression => GetMemberName(unaryExpression),
                _ => throw new ArgumentException(""),
            };
        }

        private static string GetMemberName(UnaryExpression unaryExpression)
        {
            if (unaryExpression.Operand is MethodCallExpression methodExpression)
            {
                return methodExpression.Method.Name;
            }

            return ((MemberExpression)unaryExpression.Operand).Member.Name;
        }

        public MergeBuilder<TEntity> SetConditions<TConditionType>(TConditionType conditionType, params Expression<Func<TEntity, object>>[] expressions)
            where TConditionType : Enum
        {
            var enumValue = (ConditionTypes)Enum.Parse(typeof(TConditionType), conditionType.ToString());
            
            foreach(var expression in GetColumns(expressions))
                Conditions.Add((expression, enumValue));

            return this;
        }
    }
}