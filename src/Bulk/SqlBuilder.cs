using Bulk.Extensions;
using Bulk.Models;
using Bulk.Models.Enumerators;
using System.Text;

namespace Bulk
{
    internal static class SqlBuilder
    {
        public static string BuildTempTable(string tableName)
        {
            return $@"Select Top 0 * into #{tableName} from {tableName}";
        }

        public static string BuildPrimaryKeyQuery(string tableName)
        {
            return @$"
                select column_name from information_schema.key_column_usage
                where objectproperty(object_id(constraint_schema + '.' + quotename(constraint_name)), 'IsPrimaryKey') = 1
                and table_name = '{tableName}'
            ";
        }

        public static StringBuilder BuildMerge(
            string tableName, 
            List<string> mergedColumns, 
            List<string> updatedColumns, 
            List<string> insertedColumns,
            List<ConditionBuilder> conditions,
            string statusColumn
        )
        {
            var stringBuilderQuery = new StringBuilder($"MERGE {tableName} as tgt \n using (select * from #{tableName}) as src on ");
            BuildMergedColumns(stringBuilderQuery, mergedColumns);

            stringBuilderQuery.Append($"\n when matched");
            BuildConditions(stringBuilderQuery, conditions);

            stringBuilderQuery.Append($" then \n update set ");
            BuildUpdatedColumns(stringBuilderQuery, updatedColumns, statusColumn);

            stringBuilderQuery.Append($"\n when not matched then \n insert values (");
            BuildInsertedColumns(stringBuilderQuery, insertedColumns, statusColumn);

            BuildOutput(stringBuilderQuery);

            return stringBuilderQuery;
        }

        public static string BuildDropTempTable(string tableName)
        {
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
        private static void BuildUpdatedColumns(StringBuilder stringBuilderQuery, List<string> UpdatedColumns, string StatusColumn)
        {
            for (int i = 0; i < UpdatedColumns.Count; i++)
            {
                stringBuilderQuery.Append($"tgt.{UpdatedColumns[i]} = src.{UpdatedColumns[i]}");

                if (i != (UpdatedColumns.Count - 1))
                    stringBuilderQuery.Append($", ");
            }

            if (!string.IsNullOrWhiteSpace(StatusColumn))
                stringBuilderQuery.Append($", tgt.{StatusColumn} = '{BulkStatus.ALTERAR}'");
        }
        private static void BuildInsertedColumns(StringBuilder stringBuilderQuery, List<string> InsertedColumns, string StatusColumn)
        {
            for (int i = 0; i < InsertedColumns.Count; i++)
            {
                stringBuilderQuery.Append($"src.{InsertedColumns[i]}");

                if (i != (InsertedColumns.Count - 1))
                    stringBuilderQuery.Append(", ");
            }

            if (!string.IsNullOrWhiteSpace(StatusColumn))
                stringBuilderQuery.Append($", '{BulkStatus.INSERIR}'");
        }   
        private static void BuildOutput(StringBuilder stringBuilderQuery)
        {
            stringBuilderQuery.Append($") \n output $action;");
        }
    }
}