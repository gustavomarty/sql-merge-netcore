using SqlComplexOperations.Extensions;
using SqlComplexOperations.Models;
using SqlComplexOperations.Models.Enumerators;
using System.Text;

namespace SqlComplexOperations.Common
{
    internal static class PostgreSqlBuilder
    {
        internal static string BuildTempTable(string tableName, string schema)
        {
            if (!string.IsNullOrWhiteSpace(schema))
            {
                return $"create temp table {tableName}_temp as select * from {schema}.\"{tableName}\" where false";
            }

            return $"create temp table {tableName}_temp as select * from \"{tableName}\" where false";
        }

        internal static string BuildPrimaryKeyQuery(string tableName, string schema)
        {
            var sql = new StringBuilder($"select kcu.column_name from information_schema.table_constraints tc join information_schema.key_column_usage kcu on tc.constraint_name = kcu.constraint_name and tc.table_name = kcu.table_name where tc.constraint_type = 'PRIMARY KEY' and tc.table_name = '{tableName}'");

            if (!string.IsNullOrWhiteSpace(schema))
            {
                sql.Append($" and tc.table_schema = '{schema}'");
            }

            return sql.ToString();
        }

        internal static string BuildAllColumnsDbOrderQuery(string tableName, string schema)
        {
            if (!string.IsNullOrWhiteSpace(schema))
            {
                return $@"select column_name from information_schema.columns where table_name = '{tableName}' and table_schema = '{schema}' order by column_name";
            }

            return $@"select column_name from information_schema.columns where table_name = '{tableName}' order by column_name";
        }

        internal static string BuildMerge(MergeBuilderSqlConfiguration mergeBuilderSqlConfiguration)
        {
            var stringBuilderQuery = string.IsNullOrWhiteSpace(mergeBuilderSqlConfiguration.Schema)
                ? new StringBuilder($"MERGE INTO \"{mergeBuilderSqlConfiguration.TableName}\" as tgt \n using {mergeBuilderSqlConfiguration.TableName}_temp as src on ")
                : new StringBuilder($"MERGE INTO [{mergeBuilderSqlConfiguration.Schema}].\"{mergeBuilderSqlConfiguration.TableName}\" as tgt \n using [{mergeBuilderSqlConfiguration.Schema}].{mergeBuilderSqlConfiguration.TableName}_temp as src on ");

            BuildMergedColumns(stringBuilderQuery, mergeBuilderSqlConfiguration.MergedColumns);

            stringBuilderQuery.Append($"\n when matched");
            BuildConditions(stringBuilderQuery, mergeBuilderSqlConfiguration.Conditions);

            stringBuilderQuery.Append($" then \n update set ");
            BuildUpdatedColumns(stringBuilderQuery, mergeBuilderSqlConfiguration.UpdatedColumns, mergeBuilderSqlConfiguration.StatusColumn, mergeBuilderSqlConfiguration.UseEnumStatus);

            stringBuilderQuery.Append($"\n when not matched then \n insert ");
            BuildInsertedColumns(stringBuilderQuery, mergeBuilderSqlConfiguration.InsertedColumns, mergeBuilderSqlConfiguration.StatusColumn, mergeBuilderSqlConfiguration.UseEnumStatus);

            return stringBuilderQuery.ToString();
        }

        internal static string BuildDropTempTable(string tableName, string schema)
        {
            if (!string.IsNullOrWhiteSpace(schema))
            {
                return $"drop table if exists {schema}.{tableName}_temp";
            }

            return $"drop table if exists {tableName}_temp";
        }

        private static void BuildMergedColumns(StringBuilder stringBuilderQuery, List<string> mergedColumns)
        {
            for (int i = 0; i < mergedColumns.Count; i++)
            {
                stringBuilderQuery.Append($"tgt.\"{mergedColumns[i]}\" = src.\"{mergedColumns[i]}\"");

                if (i != mergedColumns.Count - 1)
                    stringBuilderQuery.Append(" AND ");
            }
        }

        private static void BuildConditions(StringBuilder stringBuilderQuery, List<ConditionBuilder> conditions)
        {
            for (int i = 0; i < conditions.Count; i++)
            {
                stringBuilderQuery.Append(" AND (");

                for (int j = 0; j < conditions[i].Fields.Count; j++)
                {
                    var cType = conditions[i].ConditionType.DisplayName();
                    var cOperator = conditions[i].ConditionOperator.DisplayName();
                    var field = conditions[i].Fields[j];

                    if (j != 0)
                    {
                        stringBuilderQuery.Append($" {cOperator} ");
                    }

                    stringBuilderQuery.Append($"tgt.\"{field}\" {cType} src.\"{field}\"");
                }

                stringBuilderQuery.Append(')');
            }
        }

        private static void BuildUpdatedColumns(StringBuilder stringBuilderQuery, List<string> updatedColumns, string statusColumn, bool useEnumStatus)
        {
            for (int i = 0; i < updatedColumns.Count; i++)
            {
                stringBuilderQuery.Append($"\"{updatedColumns[i]}\" = src.\"{updatedColumns[i]}\"");

                if (i != updatedColumns.Count - 1)
                    stringBuilderQuery.Append($", ");
            }

            if (!string.IsNullOrWhiteSpace(statusColumn))
            {
                if (useEnumStatus)
                    stringBuilderQuery.Append($", \"{statusColumn}\" = {(int)BulkMergeStatus.UPDATED}");
                else
                    stringBuilderQuery.Append($", \"{statusColumn}\" = '{BulkMergeStatus.UPDATED}'");
            }
        }

        public static string GetDeleteScript(string tableName, List<string> mergedColumns, string statusColumn, bool useEnumStatus)
        {
            if (string.IsNullOrWhiteSpace(statusColumn))
            {
                var sql = new StringBuilder($"DELETE FROM \"{tableName}\" as tgt WHERE NOT EXISTS (SELECT 1 FROM {tableName}_temp as src WHERE ");

                for (int i = 0; i < mergedColumns.Count; i++)
                {
                    sql.Append($"tgt.\"{mergedColumns[i]}\" = src.\"{mergedColumns[i]}\"");

                    if (i != mergedColumns.Count - 1)
                        sql.Append(" AND ");
                }

                sql.Append(");");

                return sql.ToString();
            }
            else
            {
                var sql = new StringBuilder($"UPDATE \"{tableName}\" as tgt ");

                if (useEnumStatus)
                    sql.Append($"set \"{statusColumn}\" = {(int)BulkMergeStatus.DELETED} ");
                else
                    sql.Append($"set \"{statusColumn}\" = '{BulkMergeStatus.DELETED}' ");

                sql.Append("WHERE NOT EXISTS (SELECT 1 FROM {tableName}_temp as src WHERE ");

                for (int i = 0; i < mergedColumns.Count; i++)
                {
                    sql.Append($"tgt.\"{mergedColumns[i]}\" = src.\"{mergedColumns[i]}\"");

                    if (i != mergedColumns.Count - 1)
                        sql.Append(" AND ");
                }

                sql.Append(");");

                return sql.ToString();
            }
        }

        private static void BuildInsertedColumns(StringBuilder stringBuilderQuery, List<string> insertedColumns, string statusColumn, bool useEnumStatus)
        {
            stringBuilderQuery.Append($"(");
            for (int i = 0; i < insertedColumns.Count; i++)
            {
                stringBuilderQuery.Append($"\"{insertedColumns[i]}\"");

                if (i != insertedColumns.Count - 1)
                    stringBuilderQuery.Append(", ");
            }
            if (!string.IsNullOrWhiteSpace(statusColumn))
                stringBuilderQuery.Append($", \"{statusColumn}\"");

            stringBuilderQuery.Append($") values (");
            for (int i = 0; i < insertedColumns.Count; i++)
            {
                stringBuilderQuery.Append($"src.\"{insertedColumns[i]}\"");

                if (i != insertedColumns.Count - 1)
                    stringBuilderQuery.Append(", ");
            }

            if (!string.IsNullOrWhiteSpace(statusColumn))
            {
                if (useEnumStatus)
                    stringBuilderQuery.Append($", {(int)BulkMergeStatus.INSERTED}");
                else
                    stringBuilderQuery.Append($", '{BulkMergeStatus.INSERTED}'");
            }

            stringBuilderQuery.Append(");");
        }
    }
}