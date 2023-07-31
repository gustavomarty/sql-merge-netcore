using System.Data;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Bulk
{
    public class MergeBuilder<TEntity> where TEntity : class
    {
        private string _tableName;
        private List<string> _mergeColumns { get; set; } = new List<string>();
        private List<string> _updatedColumns { get; set; } = new List<string>();
        //private Dictionary<string, ConditionTypes> Condition { get; set; }
        private List<TEntity> _dataSource { get; set; }
        private IDbTransaction _dbTransaction { get; set; }

        public MergeBuilder()
        {
            _tableName = typeof(TEntity).Name;
        }

        public MergeBuilder<TEntity> SetMergeColumns(params Expression<Func<TEntity, object>>[] expressions)
        {
            GetColumns(out var columns, expressions);
            _mergeColumns.AddRange(columns);

            return this;
        }

        public MergeBuilder<TEntity> SetUpdatedColumns(params Expression<Func<TEntity, object>>[] expressions)
        {
            GetColumns(out var columns, expressions);
            _updatedColumns.AddRange(columns);

            return this;
        }

        public MergeBuilder<TEntity> SetDataSource(List<TEntity> datasource)
        {
            _dataSource = datasource;
            return this;
        }

        public MergeBuilder<TEntity> SetTransaction(IDbTransaction transaction)
        {
            _dbTransaction = transaction;
            return this;
        }

        public string Execute()
        {
            CreateTempTable();
            PopulateTempTable();
            RunMerge();

            return "Deu boa!!";
        }

        private void RunMerge()
        {
            var mergeQuery = $"MERGE {_tableName} as tgt \n using (select * from #{_tableName}) as src on ";

            foreach (var item in _mergeColumns)
                mergeQuery += $"tgt.{item} = src.{item} and ";
            mergeQuery = mergeQuery.Substring(0, mergeQuery.Length - 5);

            mergeQuery += "\n when matched AND 1 = 1 then ";
            mergeQuery += "\n update set ";

            foreach (var item in _updatedColumns)
                mergeQuery += $"tgt.{item} = src.{item}, ";
            mergeQuery = mergeQuery.Substring(0, mergeQuery.Length - 2);

            mergeQuery += "\n when not matched then ";
            mergeQuery += "\n insert values (";
            foreach (var item in _updatedColumns)
            {
                mergeQuery += $"src.{item}, ";
            }
            mergeQuery = mergeQuery.Substring(0, mergeQuery.Length - 2);
            mergeQuery += ")";
            mergeQuery += "\n output $action;";

            var sqlCommand = _dbTransaction.Connection.CreateCommand();

            sqlCommand.Transaction = _dbTransaction;
            sqlCommand.CommandText = mergeQuery;

            sqlCommand.ExecuteNonQuery();

        }

        private void CreateTempTable()
        {
            var sqlCommand = _dbTransaction.Connection.CreateCommand();

            sqlCommand.Transaction = _dbTransaction;
            sqlCommand.CommandText = $@"Select Top 0 * into #{_tableName} from {_tableName}";

            sqlCommand.ExecuteNonQuery();
        }

        private void PopulateTempTable()
        {
            var sqlCommand = _dbTransaction.Connection.CreateCommand();

            sqlCommand.Transaction = _dbTransaction;
            string query = $@"insert into #{_tableName} values";

            foreach (var property in _dataSource)
            {
                query += $" (";

                foreach (var item in _updatedColumns) { 
                    query += $"{getPropertieValue(property, item)}";
                }
                query = query.Substring(0, query.Length - 2);
                query += $"),";

                //query += "'{time.DataAtualizacao}', '{time.Campeonato}', '{time.Nome}', {time.Titulos}, {time.Participacoes}, {time.Jogos}, {time.Vitorias}, {time.Derrotas}, {time.Empates}),";
            }
            query = query.Substring(0, query.Length - 1);
            sqlCommand.CommandText = query;
            sqlCommand.ExecuteNonQuery();
        }   

        private string getPropertieValue(TEntity property, string name)
        {
            var propertyDetails = property?.GetType()?.GetProperty(name)?.GetValue(property, null);

            if (propertyDetails == null)
                return string.Empty;

            if (propertyDetails.GetType().Name.Equals(typeof(string).Name) || propertyDetails.GetType().Name.Equals(typeof(DateTime).Name))
                return $"'{propertyDetails}', ";
            else
                return $"{propertyDetails}, ";
        }

        private static bool GetColumns(out List<string> columns, params Expression<Func<TEntity, object>>[] expressions)
        {
            columns = null;
            var names = GetMemberNames(expressions);
            if (names != null && names.Any())
            {
                columns = names;
            }

            return columns != null && columns.Any();
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

        //public MergeBuilder<TEntity> SetCondition(params Expression<Func<TEntity, (object, ConditionTypes)>>[] expressions)
        //{
        //    foreach (var expression in expressions)
        //    {
        //        var memberExpression = (MemberExpression)expression.Body;
        //        var fieldName = memberExpression.Member.Name;

        //        if (!string.IsNullOrWhiteSpace(fieldName))
        //            MergeColumns.Add(fieldName);

        //    }

        //    return this;
        //}

    }

}
