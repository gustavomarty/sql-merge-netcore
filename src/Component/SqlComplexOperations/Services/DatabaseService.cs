using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using SqlComplexOperations.Extensions;
using SqlComplexOperations.Models.Enumerators;
using SqlComplexOperations.Models.Output;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;

namespace SqlComplexOperations.Services
{
    [ExcludeFromCodeCoverage]
    public class DatabaseService : IDatabaseService
    {
        public async Task PopulateTempTable<TEntity>(IDbTransaction dbTransaction, List<TEntity> dataSource, string tableName, string schema, List<string> columnOrder, bool isSnakeCase)
        {
            DataTable table = new()
            {
                TableName = string.IsNullOrWhiteSpace(schema) ? tableName : $"[{schema}].{tableName}"
            };

            using var bulkInsert = new SqlBulkCopy(dbTransaction.Connection as SqlConnection, SqlBulkCopyOptions.Default, dbTransaction as SqlTransaction);
            bulkInsert.DestinationTableName = table.TableName;

            using var dataReader = new ObjectDataReader<TEntity>(dataSource.GetEnumerator(), columnOrder, isSnakeCase);
            await bulkInsert.WriteToServerAsync(dataReader);
        }

        public OutputModel ExecuteMergeCommand(IDbTransaction dbTransaction, string command)
        {
            var sqlCommand = dbTransaction!.Connection!.CreateCommand();

            sqlCommand.Transaction = dbTransaction;
            sqlCommand.CommandText = command;

            sqlCommand.ExecuteNonQuery();

            return new();
        }

        public OutputModelRowCount ExecuteMergeCommandRowCount(IDbTransaction dbTransaction, string command)
        {
            var actions = new List<string>();

            var sqlCommand = dbTransaction!.Connection!.CreateCommand();

            sqlCommand.Transaction = dbTransaction;
            sqlCommand.CommandText = command;

            using(var rdr = sqlCommand.ExecuteReader())
            {
                while(rdr.Read())
                {
                    var myString = rdr.GetString(0);
                    actions.Add(myString);
                }
            }

            return new OutputModelRowCount
            {
                RowsAffected = actions.Count
            };
        }

        public OutputModelSimple ExecuteMergeCommandSimple(IDbTransaction dbTransaction, string command)
        {
            var actions = new List<string>();

            var sqlCommand = dbTransaction!.Connection!.CreateCommand();

            sqlCommand.Transaction = dbTransaction;
            sqlCommand.CommandText = command;

            using(var rdr = sqlCommand.ExecuteReader())
            {
                while(rdr.Read())
                {
                    var myString = rdr.GetString(0);
                    actions.Add(myString);
                }
            }

            return new OutputModelSimple
            {
                Inserted = actions.Count(x => x.Equals(OutputAction.INSERT.DisplayName())),
                Updated = actions.Count(x => x.Equals(OutputAction.UPDATE.DisplayName())),
                Deleted = actions.Count(x => x.Equals(OutputAction.DELETE.DisplayName()))
            };
        }

        public OutputModelComplete<T> ExecuteMergeCommandComplete<T>(IDbTransaction dbTransaction, string command, List<string> columns, bool isSnakeCase)
            where T : class
        {
            var dataList = new List<OutputDataComplete<T>>();

            var sqlCommand = dbTransaction!.Connection!.CreateCommand();

            sqlCommand.Transaction = dbTransaction;
            sqlCommand.CommandText = command;

            List<string> codeColumns = columns;
            if(isSnakeCase)
                codeColumns = columns.Select(x => x.ToPascalCase()).ToList();

            using(var rdr = sqlCommand.ExecuteReader())
            {
                while(rdr.Read())
                {
                    var data = GenerateOutputData<T>(rdr, columns, codeColumns);
                    dataList.Add(data);
                }
            }

            return new OutputModelComplete<T>
            {
                Inserted = dataList.Count(x => x.Action == OutputAction.INSERT),
                Updated = dataList.Count(x => x.Action == OutputAction.UPDATE),
                Deleted = dataList.Count(x => x.Action == OutputAction.DELETE),
                Data = dataList
            };
        }

        public object? ExecuteScalarCommand(IDbTransaction dbTransaction, string command)
        {
            var sqlCommand = dbTransaction!.Connection!.CreateCommand();

            sqlCommand.Transaction = dbTransaction;
            sqlCommand.CommandText = command;

            var obj = sqlCommand.ExecuteScalar();

            return obj;
        }

        public List<string> ExecuteReaderCommand(IDbTransaction dbTransaction, string command)
        {
            var ret = new List<string>();

            var sqlCommand = dbTransaction!.Connection!.CreateCommand();

            sqlCommand.Transaction = dbTransaction;
            sqlCommand.CommandText = command;

            using(var rdr = sqlCommand.ExecuteReader())
            {
                while(rdr.Read())
                {
                    var myString = rdr.GetString(0);
                    ret.Add(myString);
                }
            }

            return ret;
        }

        public void ExecuteNonQueryCommand(IDbTransaction dbTransaction, string command)
        {
            var sqlCommand = dbTransaction!.Connection!.CreateCommand();

            sqlCommand.Transaction = dbTransaction;
            sqlCommand.CommandText = command;

            sqlCommand.ExecuteNonQuery();
        }

        private static OutputDataComplete<T> GenerateOutputData<T>(IDataReader rdr, List<string> dbColumns, List<string> codeColumns)
            where T : class
        {
            var action = rdr.GetString(0);
            _ = Enum.TryParse(action, out OutputAction act);

            var jsonSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            T? srcObj = null;
            T? tgtObj = null;

            if(act != OutputAction.DELETE)
            {
                var src = new ExpandoObject();

                for(int i = 0; i < dbColumns.Count; i++)
                {
                    var attName = codeColumns[i];
                    var attValue = rdr[$"src{dbColumns[i]}"];

                    src.TryAdd(attName, attValue);
                }

                var srcJson = JsonConvert.SerializeObject(src, jsonSettings);
                srcObj = JsonConvert.DeserializeObject<T>(srcJson, jsonSettings);
            }

            if(act != OutputAction.INSERT)
            {
                var tgt = new ExpandoObject();

                for(int i = 0; i < dbColumns.Count; i++)
                {
                    var attName = codeColumns[i];
                    var attValue = rdr[$"tgt{dbColumns[i]}"];

                    tgt.TryAdd(attName, attValue);
                }

                var tgtJson = JsonConvert.SerializeObject(tgt, jsonSettings);
                tgtObj = JsonConvert.DeserializeObject<T>(tgtJson, jsonSettings);
            }


            var data = new OutputDataComplete<T>
            {
                Action = act,
                InsertedData = srcObj,
                DeletedData = tgtObj
            };

            return data;
        }
    }
}
