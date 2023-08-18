using SqlComplexOperations.Extensions;
using SqlComplexOperations.Models;
using SqlComplexOperations.Models.Enumerators;
using System.Text;

namespace SqlComplexOperations
{
    internal static class SqlBuilder
    {
        internal static string BuildTempTable(string tableName, string schema)
        {
            if(!string.IsNullOrWhiteSpace(schema))
            {
                return $@"Select Top 0 * into [{schema}].#{tableName} from [{schema}].{tableName}";
            }

            return $@"Select Top 0 * into #{tableName} from {tableName}";
        }

        internal static string BuildPrimaryKeyQuery(string tableName, string schema)
        {
            var sql = new StringBuilder($"select column_name from information_schema.key_column_usage where objectproperty(object_id(constraint_schema + '.' + quotename(constraint_name)), 'IsPrimaryKey') = 1 and table_name = '{tableName}'");

            if(!string.IsNullOrWhiteSpace(schema))
            {
                sql.Append($" and table_schema = '{schema}'");
            }

            return sql.ToString();
        }

        internal static StringBuilder BuildMerge(MergeBuilderSqlConfiguration mergeBuilderSqlConfiguration)
        {
            var stringBuilderQuery = string.IsNullOrWhiteSpace(mergeBuilderSqlConfiguration.Schema) 
                ? new StringBuilder($"MERGE {mergeBuilderSqlConfiguration.TableName} as tgt \n using (select * from #{mergeBuilderSqlConfiguration.TableName}) as src on ") 
                : new StringBuilder($"MERGE [{mergeBuilderSqlConfiguration.Schema}].{mergeBuilderSqlConfiguration.TableName} as tgt \n using (select * from [{mergeBuilderSqlConfiguration.Schema}].#{mergeBuilderSqlConfiguration.TableName}) as src on ");
            
            BuildMergedColumns(stringBuilderQuery, mergeBuilderSqlConfiguration.MergedColumns);

            stringBuilderQuery.Append($"\n when matched");
            BuildConditions(stringBuilderQuery, mergeBuilderSqlConfiguration.Conditions);

            stringBuilderQuery.Append($" then \n update set ");
            BuildUpdatedColumns(stringBuilderQuery, mergeBuilderSqlConfiguration.UpdatedColumns, mergeBuilderSqlConfiguration.StatusColumn, mergeBuilderSqlConfiguration.UseEnumStatus);

            stringBuilderQuery.Append($"\n when not matched then \n insert values (");
            BuildInsertedColumns(stringBuilderQuery, mergeBuilderSqlConfiguration.InsertedColumns, mergeBuilderSqlConfiguration.StatusColumn, mergeBuilderSqlConfiguration.UseEnumStatus);

            BuildOutput(stringBuilderQuery);

            return stringBuilderQuery;
        }

        internal static string BuildDropTempTable(string tableName, string schema)
        {
            if(!string.IsNullOrWhiteSpace(schema))
            {
                return $@"drop table [{schema}].#{tableName}";
            }

            return $@"drop table #{tableName}";
        }

        private static void BuildMergedColumns(StringBuilder stringBuilderQuery, List<string> MergedColumns)
        {
            for (int i = 0; i < MergedColumns.Count; i++)
            {
                stringBuilderQuery.Append($"tgt.{MergedColumns[i]} = src.{MergedColumns[i]}");

                if (i != (MergedColumns.Count - 1))
                    stringBuilderQuery.Append(" AND ");
            }
        }
        private static void BuildConditions(StringBuilder stringBuilderQuery, List<ConditionBuilder> conditions)
        {
            for (int i = 0; i < conditions.Count; i++)
            {
                stringBuilderQuery.Append(" AND (");

                for(int j = 0; j < conditions[i].Fields.Count; j++)
                {
                    var cType = conditions[i].ConditionType.DisplayName();
                    var cOperator = conditions[i].ConditionOperator.DisplayName();
                    var field = conditions[i].Fields[j];

                    if(j != 0)
                    {
                        stringBuilderQuery.Append($" {cOperator} ");
                    }
                    
                    stringBuilderQuery.Append($"tgt.{field} {cType} src.{field}");
                }

                stringBuilderQuery.Append(')');
            }
        }
        private static void BuildUpdatedColumns(StringBuilder stringBuilderQuery, List<string> UpdatedColumns, string StatusColumn, bool useEnumStatus)
        {
            for (int i = 0; i < UpdatedColumns.Count; i++)
            {
                stringBuilderQuery.Append($"tgt.{UpdatedColumns[i]} = src.{UpdatedColumns[i]}");

                if (i != (UpdatedColumns.Count - 1))
                    stringBuilderQuery.Append($", ");
            }

            if (!string.IsNullOrWhiteSpace(StatusColumn))
            {
                if (useEnumStatus)
                    stringBuilderQuery.Append($", tgt.{StatusColumn} = {(int)BulkMergeStatus.UPDATED}");
                else
                    stringBuilderQuery.Append($", tgt.{StatusColumn} = '{BulkMergeStatus.UPDATED}'");

            }
        }
        private static void BuildInsertedColumns(StringBuilder stringBuilderQuery, List<string> InsertedColumns, string StatusColumn, bool useEnumStatus)
        {
            for (int i = 0; i < InsertedColumns.Count; i++)
            {
                stringBuilderQuery.Append($"src.{InsertedColumns[i]}");

                if (i != (InsertedColumns.Count - 1))
                    stringBuilderQuery.Append(", ");
            }

            if (!string.IsNullOrWhiteSpace(StatusColumn))
            {
                if (useEnumStatus)
                    stringBuilderQuery.Append($", {(int)BulkMergeStatus.INSERTED}");
                else
                    stringBuilderQuery.Append($", '{BulkMergeStatus.INSERTED}'");
            }
        }   
        private static void BuildOutput(StringBuilder stringBuilderQuery)
        {
            stringBuilderQuery.Append($") \n output $action;");
        }
    }
}