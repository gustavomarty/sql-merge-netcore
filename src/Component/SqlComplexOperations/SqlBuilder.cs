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

        internal static string BuildAllColumnsDbOrderQuery(string tableName, string schema)
        {
            if(!string.IsNullOrWhiteSpace(schema))
            {
                return $@"select name from sys.columns where object_id = object_id('{schema}.{tableName}') order by column_id";
            }

            return $@"select name from sys.columns where object_id = object_id('{tableName}') order by column_id";
        }

        internal static string BuildMerge(MergeBuilderSqlConfiguration mergeBuilderSqlConfiguration)
        {
            var stringBuilderQuery = string.IsNullOrWhiteSpace(mergeBuilderSqlConfiguration.Schema) 
                ? new StringBuilder($"MERGE {mergeBuilderSqlConfiguration.TableName} as tgt \n using (select * from #{mergeBuilderSqlConfiguration.TableName}) as src on ") 
                : new StringBuilder($"MERGE [{mergeBuilderSqlConfiguration.Schema}].{mergeBuilderSqlConfiguration.TableName} as tgt \n using (select * from [{mergeBuilderSqlConfiguration.Schema}].#{mergeBuilderSqlConfiguration.TableName}) as src on ");
            
            BuildMergedColumns(stringBuilderQuery, mergeBuilderSqlConfiguration.MergedColumns);

            stringBuilderQuery.Append($"\n when matched");
            BuildConditions(stringBuilderQuery, mergeBuilderSqlConfiguration.Conditions);

            stringBuilderQuery.Append($" then \n update set ");
            BuildUpdatedColumns(stringBuilderQuery, mergeBuilderSqlConfiguration.UpdatedColumns, mergeBuilderSqlConfiguration.StatusColumn, mergeBuilderSqlConfiguration.UseEnumStatus);

            if(mergeBuilderSqlConfiguration.UseDeleteClause)
                BuildDeletedClause(stringBuilderQuery, mergeBuilderSqlConfiguration.StatusColumn, mergeBuilderSqlConfiguration.UseEnumStatus);

            stringBuilderQuery.Append($"\n when not matched by target then \n insert ");
            BuildInsertedColumns(stringBuilderQuery, mergeBuilderSqlConfiguration.InsertedColumns, mergeBuilderSqlConfiguration.StatusColumn, mergeBuilderSqlConfiguration.UseEnumStatus);

            BuildOutput(stringBuilderQuery, mergeBuilderSqlConfiguration.ResponseType, mergeBuilderSqlConfiguration.AllColumns);

            return stringBuilderQuery.ToString();
        }

        internal static string BuildDropTempTable(string tableName, string schema)
        {
            if(!string.IsNullOrWhiteSpace(schema))
            {
                return $@"drop table [{schema}].#{tableName}";
            }

            return $@"drop table #{tableName}";
        }

        private static void BuildMergedColumns(StringBuilder stringBuilderQuery, List<string> mergedColumns)
        {
            for (int i = 0; i < mergedColumns.Count; i++)
            {
                stringBuilderQuery.Append($"tgt.{mergedColumns[i]} = src.{mergedColumns[i]}");

                if (i != (mergedColumns.Count - 1))
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

        private static void BuildUpdatedColumns(StringBuilder stringBuilderQuery, List<string> updatedColumns, string statusColumn, bool useEnumStatus)
        {
            for (int i = 0; i < updatedColumns.Count; i++)
            {
                stringBuilderQuery.Append($"tgt.{updatedColumns[i]} = src.{updatedColumns[i]}");

                if (i != (updatedColumns.Count - 1))
                    stringBuilderQuery.Append($", ");
            }

            if (!string.IsNullOrWhiteSpace(statusColumn))
            {
                if (useEnumStatus)
                    stringBuilderQuery.Append($", tgt.{statusColumn} = {(int)BulkMergeStatus.UPDATED}");
                else
                    stringBuilderQuery.Append($", tgt.{statusColumn} = '{BulkMergeStatus.UPDATED}'");

            }
        }

        private static void BuildDeletedClause(StringBuilder stringBuilderQuery, string statusColumn, bool useEnumStatus)
        {
            stringBuilderQuery.Append($"\n when not matched by source then ");

            if(string.IsNullOrWhiteSpace(statusColumn))
            {
                stringBuilderQuery.Append($"\n delete ");
                return;
            }

            if(useEnumStatus)
            {
                stringBuilderQuery.Append($"\n update set tgt.{statusColumn} = {(int)BulkMergeStatus.DELETED} ");
            }
            else
            {
                stringBuilderQuery.Append($"\n update set tgt.{statusColumn} = '{BulkMergeStatus.DELETED}' ");
            }
        }

        private static void BuildInsertedColumns(StringBuilder stringBuilderQuery, List<string> insertedColumns, string statusColumn, bool useEnumStatus)
        {
            stringBuilderQuery.Append($" (");
            for (int i = 0; i < insertedColumns.Count; i++)
            {
                stringBuilderQuery.Append($"{insertedColumns[i]}");

                if (i != (insertedColumns.Count - 1))
                    stringBuilderQuery.Append(", ");
            }
            if (!string.IsNullOrWhiteSpace(statusColumn))
                stringBuilderQuery.Append($", {statusColumn}");

            stringBuilderQuery.Append($") values (");
            for (int i = 0; i < insertedColumns.Count; i++)
            {
                stringBuilderQuery.Append($"src.{insertedColumns[i]}");

                if (i != (insertedColumns.Count - 1))
                    stringBuilderQuery.Append(", ");
            }

            if (!string.IsNullOrWhiteSpace(statusColumn))
            {
                if (useEnumStatus)
                    stringBuilderQuery.Append($", {(int)BulkMergeStatus.INSERTED}");
                else
                    stringBuilderQuery.Append($", '{BulkMergeStatus.INSERTED}'");
            }
        }

        private static void BuildOutput(StringBuilder stringBuilderQuery, ResponseType responseType, List<string> updateColumns)
        {
            stringBuilderQuery.Append(')');

            if(responseType == ResponseType.SIMPLE || responseType == ResponseType.ROW_COUNT)
            {
                stringBuilderQuery.Append($" \n output $action");
            }
            else if(responseType == ResponseType.COMPLETE)
            {
                stringBuilderQuery.Append($" \n output $action");

                foreach(var column in updateColumns)
                {
                    stringBuilderQuery.Append($", inserted.{column} as src{column}");
                    stringBuilderQuery.Append($", deleted.{column} as tgt{column}");
                }

            }

            stringBuilderQuery.Append(';');
        }
    }
}