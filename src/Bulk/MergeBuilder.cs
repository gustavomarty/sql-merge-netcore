using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace Bulk
{
    public class MergeBuilder<TEntity> where TEntity : class
    {
        private string _tableName;

        private List<string> _mergeColumns { get; set; }
        private List<string> _updatedColumns { get; set; }
        //private Dictionary<string, ConditionTypes> Condition { get; set; }
        private List<TEntity> _dataSource { get; set; }

        private IDbTransaction _dbTransaction { get; set; }

        public MergeBuilder()
        {
            _tableName = nameof(TEntity);
        }


        public MergeBuilder<TEntity> SetMergeColumns(params Expression<Func<TEntity, object>>[] expressions)
        {
            foreach (var expression in expressions)
            {
                var memberExpression = (MemberExpression)expression.Body;
                var fieldName = memberExpression.Member.Name;

                if (!string.IsNullOrWhiteSpace(fieldName))
                    _mergeColumns.Add(fieldName);

            }

            return this;
        }

        public MergeBuilder<TEntity> SetUpdatedColumns(params Expression<Func<TEntity, object>>[] expressions)
        {
            foreach (var expression in expressions)
            {
                var memberExpression = (MemberExpression)expression.Body;
                var fieldName = memberExpression.Member.Name;

                if (!string.IsNullOrWhiteSpace(fieldName))
                    _updatedColumns.Add(fieldName);

            }

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
            CreateTempTable(_dbTransaction);
            PopulateTempTable(_dbTransaction);


            return "Deu boa!!";
        }

        private void RunMerge(IDbTransaction dbTransaction)
        {
            var mergeQuery = $@"
                MERGE {_tableName} as tgt
                using (select * from #{_tableName}) as src on tgt.campeonato = src.campeonato and tgt.nome = src.nome
                when matched AND 1=1 then
	                update set tgt.titulos = src.titulos, tgt.dataAtualizacao = GETDATE(), tgt.jogos = src.jogos
                when not matched then
	                insert values (GETDATE(), src.campeonato, src.nome, src.titulos, src.participacoes, src.jogos, src.vitorias, src.derrotas, src.empates)
                output $action;
            ";

            var sqlCommand = dbTransaction.Connection.CreateCommand();

            sqlCommand.Transaction = dbTransaction;
            sqlCommand.CommandText = mergeQuery;

            sqlCommand.ExecuteNonQuery();

        }

        private void CreateTempTable(IDbTransaction dbTransaction)
        {
            var sqlCommand = dbTransaction.Connection.CreateCommand();

            sqlCommand.Transaction = dbTransaction;
            sqlCommand.CommandText = $@"Select Top 0 * into #{_tableName} from {_tableName}";

            sqlCommand.ExecuteNonQuery();
        }

        private void PopulateTempTable(IDbTransaction dbTransaction)
        {
            var sqlCommand = dbTransaction.Connection.CreateCommand();

            sqlCommand.Transaction = dbTransaction;
            string query = $@"insert into #{_tableName} values";

            foreach (var time in _dataSource)
            {
                query += $" ('{time.DataAtualizacao}', '{time.Campeonato}', '{time.Nome}', {time.Titulos}, {time.Participacoes}, {time.Jogos}, {time.Vitorias}, {time.Derrotas}, {time.Empates}),";
            }
            query = query.Substring(0, query.Length - 1);

            sqlCommand.CommandText = query;
            sqlCommand.ExecuteNonQuery();
        }


    }

}
