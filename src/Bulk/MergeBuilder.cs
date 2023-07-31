using System.Data;
using System.Linq.Expressions;

namespace Bulk
{
    public class MergeBuilder<TEntity> where TEntity : class
    {
        private string _tableName;
        private List<(string, Type)> _mergeColumns { get; set; }
        private List<(string, Type)> _updatedColumns { get; set; }
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
                var fileType = memberExpression.Member.GetType();

                if (!string.IsNullOrWhiteSpace(fieldName))
                    _mergeColumns.Add(new(fieldName, fileType));

            }

            return this;
        }

        public MergeBuilder<TEntity> SetUpdatedColumns(params Expression<Func<TEntity, object>>[] expressions)
        {
            foreach (var expression in expressions)
            {
                var memberExpression = (MemberExpression)expression.Body;
                var fieldName = memberExpression.Member.Name;
                var fileType = memberExpression.Member.GetType();

                if (!string.IsNullOrWhiteSpace(fieldName))
                    _updatedColumns.Add(new (fieldName, fileType));

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
            CreateTempTable();
            PopulateTempTable();
            RunMerge();

            return "Deu boa!!";
        }

        private void RunMerge()
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

            foreach (var time in _dataSource)
            {
                query += $" (";

                foreach (var item in _updatedColumns) { 
                    query += $"{getPropertie(item.Item1, item.Item2)}";
                }
                //query += "'{time.DataAtualizacao}', '{time.Campeonato}', '{time.Nome}', {time.Titulos}, {time.Participacoes}, {time.Jogos}, {time.Vitorias}, {time.Derrotas}, {time.Empates}),";
            }
            query = query.Substring(0, query.Length - 1);
            query += $")";

            sqlCommand.CommandText = query;
            sqlCommand.ExecuteNonQuery();
        }   

        private string getPropertie(string name, Type type)
        {
            if (type.Equals(typeof(string)) || type.Equals(typeof(DateTime)))
                return $"'{name}', ";
            else
                return $"{name}, ";
            
            
        }


    }

}
