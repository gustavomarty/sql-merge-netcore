using System.Text;
using Bulk.Extensions;
using Bulk.Models.Enumerators;

namespace Bulk
{
    internal static class SqlBuilder
    {
        public static string BuildTempTable(string tableName)
        {
            return $@"Select Top 0 * into #{tableName} from {tableName}";
        }

        public static StringBuilder BuildMerge(
            string tableName, 
            List<string> MergedColumns, 
            List<string> UpdatedColumns, 
            List<string> InsertedColumns,
            List<(string field, ConditionTypes op)> Conditions,
            string StatusColumn)
        {
            var stringBuilderQuery = new StringBuilder($"MERGE {tableName} as tgt \n using (select * from #{tableName}) as src on ");
            BuildMergedColumns(stringBuilderQuery, MergedColumns);

            stringBuilderQuery.Append($"\n when matched");
            BuildConditions(stringBuilderQuery, Conditions);

            stringBuilderQuery.Append($" then \n update set ");
            BuildUpdatedColumns(stringBuilderQuery, UpdatedColumns, StatusColumn);

            stringBuilderQuery.Append($"\n when not matched then \n insert values (");
            BuildInsertedColumns(stringBuilderQuery, InsertedColumns, StatusColumn);

            BuildOutput(stringBuilderQuery);

            return stringBuilderQuery;
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
        private static void BuildConditions(StringBuilder stringBuilderQuery, List<(string field, ConditionTypes op)> Conditions)
        {
            for (int i = 0; i < Conditions.Count; i++)
            {
                var operation = Conditions[i].op.DisplayName();
                var field = Conditions[i].field;

                stringBuilderQuery.Append($" AND tgt.{field} {operation} src.{field}");

                if (i != (Conditions.Count - 1))
                    stringBuilderQuery.Append(" AND ");
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