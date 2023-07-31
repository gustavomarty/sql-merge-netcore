using Bulk.Models;
using Bulk.Models.Enumerators;
using System.Data;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;

namespace Bulk
{
    public class MergeBuilder<TEntity> where TEntity : class
    {
        private readonly string _tableName;

        private List<string> AllColumns { get; set; } = new();
        private List<string> MergeColumns { get; set; } = new();
        private List<string> UpdatedColumns { get; set; } = new();
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
            MergeColumns.AddRange(GetColumns(expressions));
            return this;
        }

        public MergeBuilder<TEntity> SetUpdatedColumns(params Expression<Func<TEntity, object>>[] expressions)
        {
            UpdatedColumns.AddRange(GetColumns(expressions));
            return this;
        }

        public MergeBuilder<TEntity> SetAllColumns()
        {
            var names = typeof(TEntity).GetProperties().Select(x => x.Name);
            AllColumns.AddRange(names);
            
            return this;
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

            CreateTempTable(DbTransaction.Connection);
            PopulateTempTable(DbTransaction.Connection);
            ExecuteMergeCommand(DbTransaction.Connection);

            return "Deu boa!!";
        }

        //private void RunMerge()
        //{
        //    var mergeQuery = $"MERGE {_tableName} as tgt \n using (select * from #{_tableName}) as src on ";

        //    foreach (var item in MergeColumns)
        //        mergeQuery += $"tgt.{item} = src.{item} and ";
        //    mergeQuery = mergeQuery.Substring(0, mergeQuery.Length - 5);

        //    mergeQuery += "\n when matched AND 1 = 1 then "; //TODO: Set conditions
        //    mergeQuery += "\n update set ";

        //    foreach (var item in UpdatedColumns)
        //        mergeQuery += $"tgt.{item} = src.{item}, ";
        //    mergeQuery = mergeQuery.Substring(0, mergeQuery.Length - 2);

        //    mergeQuery += "\n when not matched then ";
        //    mergeQuery += "\n insert values (";
        //    foreach (var item in UpdatedColumns)     //TODO: percorrer todas, não olhar só pra update (datasource tem tudo) 
        //    {
        //        mergeQuery += $"src.{item}, ";
        //    }
        //    mergeQuery = mergeQuery.Substring(0, mergeQuery.Length - 2);
        //    mergeQuery += ")";
        //    mergeQuery += "\n output $action;";

        //    var sqlCommand = DbTransaction.Connection.CreateCommand();

        //    sqlCommand.Transaction = DbTransaction;
        //    sqlCommand.CommandText = mergeQuery;

        //    sqlCommand.ExecuteNonQuery();

        //}

        private void ExecuteMergeCommand(IDbConnection dbConnection)
        {
            var stringBuilderQuery = new StringBuilder($"MERGE {_tableName} as tgt \n using (select * from #{_tableName}) as src on ");

            for(int i=0; i<MergeColumns.Count; i++)
            {
                stringBuilderQuery.Append($"tgt.{MergeColumns[i]} = src.{MergeColumns[i]}");

                if(i != (MergeColumns.Count - 1))
                {
                    stringBuilderQuery.Append($" AND ");
                }
            }

            stringBuilderQuery.Append($"\n when matched AND ");

            for(int i = 0; i < Conditions.Count; i++)
            {
                var operation = Conditions[i].op.DisplayName();
                var field = Conditions[i].field;

                stringBuilderQuery.Append($"tgt.{field} {operation} src.{field}");

                if(i != (Conditions.Count - 1))
                {
                    stringBuilderQuery.Append($" AND ");
                }
            }

            stringBuilderQuery.Append($" then \n update set ");

            for(int i = 0; i < UpdatedColumns.Count; i++)
            {
                stringBuilderQuery.Append($"tgt.{UpdatedColumns[i]} = src.{UpdatedColumns[i]}, ");

                if(i == (UpdatedColumns.Count - 1))
                {
                    stringBuilderQuery.Append($"tgt.{StatusColumn} = {BulkStatus.ALTERAR}");
                }
            }

            stringBuilderQuery.Append($"\n when not matched then \n insert values (");

            for(int i = 0; i < AllColumns.Count; i++)
            {
                stringBuilderQuery.Append($"src.{AllColumns[i]}, ");

                if(i != (AllColumns.Count - 1))
                {
                    stringBuilderQuery.Append($"{BulkStatus.INSERIR}");
                }
            }

            stringBuilderQuery.Append($") \n output $action;");

            var sqlCommand = dbConnection.CreateCommand();

            sqlCommand.Transaction = DbTransaction;
            sqlCommand.CommandText = stringBuilderQuery.ToString();

            sqlCommand.ExecuteNonQuery();
        }

        private void CreateTempTable(IDbConnection dbConnection)
        {
            var sqlCommand = dbConnection.CreateCommand();

            sqlCommand.Transaction = DbTransaction;
            sqlCommand.CommandText = $@"Select Top 0 * into #{_tableName} from {_tableName}";

            sqlCommand.ExecuteNonQuery();
        }

        private void PopulateTempTable(IDbConnection dbConnection)
        {
            var sqlCommand = dbConnection.CreateCommand();

            sqlCommand.Transaction = DbTransaction;
            var stringBuilderQuery = new StringBuilder($"insert into #{_tableName} values");

            foreach (var property in DataSource)
            {
                stringBuilderQuery.Append($" (");

                for(int i = 0; i < AllColumns.Count; i++)
                {
                    stringBuilderQuery.Append($"{MergeBuilder<TEntity>.GetPropertieValue(property, AllColumns[i])}");

                    if(i != (AllColumns.Count - 1))
                    {
                        stringBuilderQuery.Append($", ");
                    }
                }
                stringBuilderQuery.Append($"),");
            }

            stringBuilderQuery.Remove(0, stringBuilderQuery.Length - 1);

            sqlCommand.CommandText = stringBuilderQuery.ToString();
            sqlCommand.ExecuteNonQuery();
        }   

        private static string GetPropertieValue(TEntity property, string name)
        {
            var propertyDetails = property?.GetType()?.GetProperty(name)?.GetValue(property, null);

            if (propertyDetails == null)
                return string.Empty;

            if (propertyDetails.GetType().Name.Equals(typeof(string).Name) || propertyDetails.GetType().Name.Equals(typeof(DateTime).Name))
                return $"'{propertyDetails}', ";
            else
                return $"{propertyDetails}, ";
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

        //private static bool GetConditionNames<T>(out List<(string, ConditionTypes)> conditions, Expression<Func<T, ConditionBuilder>> expression)
        //{
        //    conditions = new();

        //    var test = ((MemberInitExpression)expression.Body).Bindings;
        //    var a = (MemberAssignment)test.First();

        //    var aa = a.Expression.Type.GetDefaultMembers()[0];

        //    var property = expression.Body.Type.GetProperty(nameof(ConditionBuilder.Conditions));
        //    var list = (List<ConditionTypeDto>)property.GetValue(expression.Body);

        //    foreach(var cBuilder in list)
        //    {
        //        var fieldName = cBuilder.Field.GetType().GetProperties().Select(x => x.Name).First();
        //        conditions.Add((fieldName, cBuilder.ConditionType));
        //    }

        //    return conditions != null && conditions.Any();
        //}

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

        //public MergeBuilder<TEntity> SetConditions(Expression<Func<TEntity, ConditionBuilder>> expressions)
        //{
        //    GetConditionNames(out var conditions, expressions);

        //    foreach(var expression in conditions)
        //        _conditions.Add(expression);

        //    return this;
        //}

        public MergeBuilder<TEntity> SetConditions<TConditionType>(TConditionType conditionType, params Expression<Func<TEntity, object>>[] expressions)
            where TConditionType : Enum
        {
            var enumValue = (ConditionTypes)Enum.Parse(typeof(TConditionType), conditionType.ToString());
            
            foreach(var expression in GetColumns(expressions))
                Conditions.Add((expression, enumValue));

            return this;
        }

    }

    //public class ConditionTypeDto
    //{
    //    public object Field { get; set; }
    //    public ConditionTypes ConditionType { get; set; }
    //}

    //public class ConditionBuilder
    //{
    //    public List<ConditionTypeDto> Conditions { get; set; }
    //}

}
