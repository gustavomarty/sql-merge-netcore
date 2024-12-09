﻿using Microsoft.Data.SqlClient;
using Npgsql;
using SqlComplexOperations.Extensions;
using SqlComplexOperations.Models.Enumerators;
using SqlComplexOperations.Models.Output;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SqlComplexOperations.Services
{
    [ExcludeFromCodeCoverage]
    public class DatabaseService : IDatabaseService
    {
        public async Task BulkInsert<TEntity>(IDbTransaction dbTransaction, List<TEntity> dataSource, string tableName, string schema, List<string> columnOrder, bool isSnakeCase, bool propNameAttr)
        {
            DataTable table = new()
            {
                TableName = string.IsNullOrWhiteSpace(schema) ? tableName : $"[{schema}].{tableName}"
            };

            using var bulkInsert = new SqlBulkCopy(dbTransaction.Connection as SqlConnection, SqlBulkCopyOptions.Default, dbTransaction as SqlTransaction);
            bulkInsert.DestinationTableName = table.TableName;

            using var dataReader = new ObjectDataReader<TEntity>(dataSource.GetEnumerator(), columnOrder, isSnakeCase, propNameAttr);
            await bulkInsert.WriteToServerAsync(dataReader);
        }

        public async Task BulkInsertToPostgre<TEntity>(IDbTransaction dbTransaction, List<TEntity> dataSource, string tableName, string schema, List<string> columnOrder, bool isSnakeCase, bool propNameAttr, bool useStringType)
        {
            if (dbTransaction.Connection is not NpgsqlConnection connection)
                throw new InvalidOperationException("A conexão deve ser um NpgsqlConnection.");

            DataTable table = new()
            {
                TableName = string.IsNullOrWhiteSpace(schema) ? tableName : $"{schema}.{tableName}"
            };

            var fullTableName = table.TableName;

            using var dataReader = new ObjectDataReader<TEntity>(dataSource.GetEnumerator(), columnOrder, isSnakeCase, propNameAttr);

            await using var importer = await connection.BeginBinaryImportAsync($"COPY {fullTableName} ({string.Join(", ", columnOrder)}) FROM STDIN (FORMAT BINARY)");

            while (dataReader.Read())
            {
                importer.StartRow();
                for (int i = 0; i < dataReader.FieldCount; i++)
                {
                    var value = dataReader.GetValue(i);
                    if (value.GetType().IsEnum)
                    {
                        value = (useStringType) ? value.ToString() : (int)value;
                    }

                    importer.Write(value);
                }
            }

            await importer.CompleteAsync();
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

        public OutputModelComplete<T> ExecuteMergeCommandComplete<T>(IDbTransaction dbTransaction, string command, List<string> columns, bool isSnakeCase, bool propNameAttr)
            where T : class
        {
            var dataList = new List<OutputDataComplete<T>>();

            var sqlCommand = dbTransaction!.Connection!.CreateCommand();

            sqlCommand.Transaction = dbTransaction;
            sqlCommand.CommandText = command;

            List<string> codeColumns = columns;
            if(propNameAttr)
            {
                var newCodeColumns = new List<string>();

                foreach(var c in columns)
                {
                    var field = typeof(T).GetProperties().First(x => x.GetPropName(true).Equals(c));
                    newCodeColumns.Add(field.Name);
                }

                codeColumns = newCodeColumns;
            }
            else if(isSnakeCase)
            {
                codeColumns = columns.Select(x => x.ToPascalCase()).ToList();
            }

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

            var jsonSettings = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNameCaseInsensitive = true,
                UnknownTypeHandling = JsonUnknownTypeHandling.JsonElement
            };

            T? srcObj = null;
            T? tgtObj = null;

            if(act != OutputAction.DELETE)
            {
                var src = new ExpandoObject();

                for(int i = 0; i < dbColumns.Count; i++)
                {
                    var attName = codeColumns[i];
                    var attValue = rdr[$"src{dbColumns[i]}"] is DBNull ? null : rdr[$"src{dbColumns[i]}"];

                    src.TryAdd(attName, attValue);
                }

                var srcJson = JsonSerializer.Serialize(src, jsonSettings);
                srcObj = JsonSerializer.Deserialize<T>(srcJson, jsonSettings);
            }

            if(act != OutputAction.INSERT)
            {
                var tgt = new ExpandoObject();

                for(int i = 0; i < dbColumns.Count; i++)
                {
                    var attName = codeColumns[i];
                    var attValue = rdr[$"tgt{dbColumns[i]}"] is DBNull ? null : rdr[$"tgt{dbColumns[i]}"];

                    tgt.TryAdd(attName, attValue);
                }

                var tgtJson = JsonSerializer.Serialize(tgt, jsonSettings);
                tgtObj = JsonSerializer.Deserialize<T>(tgtJson, jsonSettings);
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
