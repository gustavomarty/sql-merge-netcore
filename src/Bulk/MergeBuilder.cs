using Bulk.Extensions;
using Bulk.Models;
using Bulk.Models.Enumerators;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Linq.Expressions;

namespace Bulk
{
    /// <summary>
    /// Classe para construir o comando SQL Merge.
    /// </summary>
    /// <typeparam name="TEntity">
    /// O Tipo <see cref="TEntity"/> é a entidade (Banco) onde o merge será executado.
    /// </typeparam>
    public class MergeBuilder<TEntity> where TEntity : class
    {
        private string _tableName;

        private string StatusColumn { get; set; } = string.Empty;
        private string PrimaryKey { get; set; } = string.Empty;
        private bool SnakeCaseNamingConvention { get; set; }

        private List<TEntity> DataSource { get; set; } = new();
        private IDbTransaction? DbTransaction { get; set; }

        private List<string> AllColumns { get; set; } = new();
        private List<string> MergedColumns { get; set; } = new();
        private List<string> UpdatedColumns { get; set; } = new();
        private List<string> IgnoredOnInsertOperation { get; set; } = new();

        private List<ConditionBuilder> Conditions { get; set; } = new();

        /// <summary>
        /// Cria uma nova instancia do MergeBuilder.
        /// </summary>
        /// <remarks>
        /// O Tipo <see cref="TEntity"/> é a entidade (Banco) onde o merge será executado.
        /// </remarks>
        public MergeBuilder()
        {
            _tableName = typeof(TEntity).Name;
        }

        /// <summary>
        /// Torna todos os nomes de atributos/tabelas do banco snake_case. (Default = FALSE).
        /// </summary>
        /// <remarks>
        /// Exemplo:
        /// Entidade.NomeEntidade => entidade.nome_entidade
        /// </remarks>
        public MergeBuilder<TEntity> UseSnakeCaseNamingConvention()
        {
            SnakeCaseNamingConvention = true;
            return this;
        }

        /// <summary>
        /// Caso queira usar uma coluna(STRUCT) de status para executar o merge. (Default = FALSE).
        /// </summary>
        /// <typeparam name="ColumnType">O tipo de coluna que será utilizada como status</typeparam>
        /// <param name="expression">A coluna deve estar dentro de <see cref="TEntity"/></param>
        /// <returns>
        /// Retorna o MergeBuilder atual.
        /// </returns>
        public MergeBuilder<TEntity> UseStatusConfiguration<ColumnType>(Expression<Func<TEntity, ColumnType>> expression)
            where ColumnType : struct
        {
            StatusColumn = expression.Body.Type.GetProperties().Select(m => m.Name).First();
            return this;
        }

        /// <summary>
        /// Caso queira usar uma coluna(STRING) de status para executar o merge. (Default = FALSE).
        /// </summary>
        /// <param name="expression">A coluna deve estar dentro de <see cref="TEntity"/></param>
        /// <returns>
        /// Retorna o MergeBuilder atual.
        /// </returns>
        public MergeBuilder<TEntity> UseStatusConfiguration(Expression<Func<TEntity, string>> expression)
        {
            StatusColumn = expression.Body.Type.GetProperties().Select(m => m.Name).First();
            return this;
        }

        /// <summary>
        /// Configura quais colunas serão utilizadas no comando merge.
        /// </summary>
        /// <remarks>
        /// Você pode passar uma coluna individualmente ou mais colunas
        /// 
        /// <para> Exemplo: </para>
        /// 
        /// SetMergeColumns(x => new { x.ColunaUm, x.ColunaDois })
        /// <br></br>
        /// SetMergeColumns(x => x.ColunaUm)
        /// </remarks>
        /// <param name="expression">As colunas deve estar dentro de <see cref="TEntity"/></param>
        /// <returns>
        /// Retorna o MergeBuilder atual.
        /// </returns>
        public MergeBuilder<TEntity> SetMergeColumns(Expression<Func<TEntity, object>> expression)
        {
            MergedColumns.AddRange(GetColumns(expression));
            return this;
        }

        /// <summary>
        /// Configura quais colunas serão atualizadas no comando merge (update).
        /// </summary>
        /// <remarks>
        /// Você pode passar uma coluna individualmente, varias colunas ou a entidade toda
        /// 
        /// <para> Exemplo: </para>
        /// 
        /// SetUpdatedColumns(x => new { x.ColunaUm, x.ColunaDois })
        /// <br></br>
        /// SetUpdatedColumns(x => x.ColunaUm)
        /// <br></br>
        /// SetUpdatedColumns(x => x)
        /// </remarks>
        /// <param name="expression">As colunas deve estar dentro de <see cref="TEntity"/></param>
        /// <returns>
        /// Retorna o MergeBuilder atual.
        /// </returns>
        public MergeBuilder<TEntity> SetUpdatedColumns(Expression<Func<TEntity, object>> expression)
        {
            UpdatedColumns.AddRange(GetColumns(expression));
            return this;
        }

        /// <summary>
        /// Configura as condições antes de seguir com o comando merge.
        /// </summary>
        /// <remarks>
        /// Espera-se que você passe varias colunas, com o Operador (AND, OR) e o tipo de condição (EQUALS, NOT_EQUALS, ETC)
        /// 
        /// <para> Esse comando pode ser usado quantas vezes você precisar (Ele sempre adiciona condições e não substitui) </para>
        /// 
        /// <para> Exemplo: </para>
        /// 
        /// WithCondition(ConditionTypes.NOT_EQUAL, ConditionOperator.OR, x => new { x.ColunaUm, x.ColunaDois })
        /// </remarks>
        /// <param name="conditionType">Deve ser um enumerador do tipo <see cref="ConditionTypes"/></param>
        /// <param name="conditionOperator">Deve ser um enumerador do tipo <see cref="ConditionOperator"/></param>
        /// <param name="expression">As colunas deve estar dentro de <see cref="TEntity"/></param>
        /// <returns>
        /// Retorna o MergeBuilder atual.
        /// </returns>
        public MergeBuilder<TEntity> WithCondition<TConditionType, TConditionOperator>(TConditionType conditionType, TConditionOperator conditionOperator, Expression<Func<TEntity, object>> expression)
            where TConditionType : Enum
            where TConditionOperator : Enum
        {
            var cTypeValue = (ConditionTypes)Enum.Parse(typeof(TConditionType), conditionType.ToString());
            var cOperatorValue = (ConditionOperator)Enum.Parse(typeof(TConditionOperator), conditionOperator.ToString());
            var columns = GetColumns(expression);

            var condition = new ConditionBuilder(columns ?? new List<string>(), cTypeValue, cOperatorValue);
            Conditions.Add(condition);

            return this;
        }

        /// <summary>
        /// Configura as condições antes de seguir com o comando merge.
        /// </summary>
        /// <remarks>
        /// Espera-se que você passe uma coluna (struct), com o tipo de condição (EQUALS, NOT_EQUALS, ETC)
        /// 
        /// <para> Esse comando pode ser usado quantas vezes você precisar (Ele sempre adiciona condições e não substitui) </para>
        /// 
        /// <para> Exemplo: </para>
        /// 
        /// WithCondition(ConditionTypes.NOT_EQUAL, x => x.ColunaUm)
        /// </remarks>
        /// <param name="conditionType">Deve ser um enumerador do tipo <see cref="ConditionTypes"/></param>
        /// <param name="expression">As colunas deve estar dentro de <see cref="TEntity"/></param>
        /// <returns>
        /// Retorna o MergeBuilder atual.
        /// </returns>
        public MergeBuilder<TEntity> WithCondition<TConditionType, TFieldType>(TConditionType conditionType, Expression<Func<TEntity, TFieldType>> expression)
            where TConditionType : Enum
            where TFieldType : struct
        {
            var cTypeValue = (ConditionTypes)Enum.Parse(typeof(TConditionType), conditionType.ToString());
            var column = expression.Body.Type.GetProperties().Select(m => m.Name).First();

            var condition = new ConditionBuilder(new List<string> { column }, cTypeValue, ConditionOperator.NONE);
            Conditions.Add(condition);

            return this;
        }

        /// <summary>
        /// Configura as condições antes de seguir com o comando merge.
        /// </summary>
        /// <remarks>
        /// Espera-se que você passe uma coluna (string), com o tipo de condição (EQUALS, NOT_EQUALS, ETC)
        /// 
        /// <para> Esse comando pode ser usado quantas vezes você precisar (Ele sempre adiciona condições e não substitui) </para>
        /// 
        /// <para> Exemplo: </para>
        /// 
        /// WithCondition(ConditionTypes.NOT_EQUAL, x => x.ColunaUm)
        /// </remarks>
        /// <param name="conditionType">Deve ser um enumerador do tipo <see cref="ConditionTypes"/></param>
        /// <param name="expression">As colunas deve estar dentro de <see cref="TEntity"/></param>
        /// <returns>
        /// Retorna o MergeBuilder atual.
        /// </returns>
        public MergeBuilder<TEntity> WithCondition<TConditionType>(TConditionType conditionType, Expression<Func<TEntity, string>> expression)
            where TConditionType : Enum
        {
            var cTypeValue = (ConditionTypes)Enum.Parse(typeof(TConditionType), conditionType.ToString());
            var column = expression.Body.Type.GetProperties().Select(m => m.Name).First();

            var condition = new ConditionBuilder(new List<string> { column }, cTypeValue, ConditionOperator.NONE);
            Conditions.Add(condition);

            return this;
        }

        /// <summary>
        /// Configura as colunas que devem ser ignoradas na clausula INSERT
        /// </summary>
        /// <remarks>
        /// Você pode passar uma coluna individualmente ou varias colunas
        /// 
        /// <para> Esse comando pode ser utilizado para colunas auto_increment por exemplo. </para>
        /// 
        /// <para> Exemplo: </para>
        /// 
        /// SetIgnoreOnIsertOperation(x => x.ColunaAutoIncrement)
        /// </remarks>
        /// <param name="expression">As colunas deve estar dentro de <see cref="TEntity"/></param>
        /// <returns>
        /// Retorna o MergeBuilder atual.
        /// </returns>
        public MergeBuilder<TEntity> SetIgnoreOnIsertOperation(Expression<Func<TEntity, object>> expression)
        {
            IgnoredOnInsertOperation.AddRange(GetColumns(expression));
            return this;
        }

        /// <summary>
        /// Configura os dados que iram ser utilizados no comando merge.
        /// <br>
        /// Os dados devem ser uma lista de <see cref="TEntity"/>.
        /// </br>
        /// </summary>
        /// <param name="datasource">Uma lista de entidades do tipo <see cref="TEntity"/> (Não persistidas no banco) para realizar o merge.</param>
        /// <returns>
        /// Retorna o MergeBuilder atual.
        /// </returns>
        public MergeBuilder<TEntity> SetDataSource(List<TEntity> datasource)
        {
            DataSource = datasource;
            return this;
        }

        /// <summary>
        /// Configura a transação de banco que será usada para realizar o comando.
        /// <br>
        /// Lembrando que NADA é commitado.
        /// </br>
        /// </summary>
        /// <param name="transaction">A transação deve ser do tipo <see cref="IDbTransaction"/>.</param>
        /// <returns>
        /// Retorna o MergeBuilder atual.
        /// </returns>
        public MergeBuilder<TEntity> SetTransaction(IDbTransaction transaction)
        {
            if(transaction == null || transaction.Connection == null)
                throw new Exception("");

            DbTransaction = transaction;
            return this;
        }

        /// <summary>
        /// Executa o comando montado até agora.
        /// </summary>
        /// <returns>
        /// Retorna o resultado do merge.
        /// </returns>
        public string Execute()
        {
            if(DbTransaction == null || DbTransaction.Connection == null)
                throw new Exception("");

            SetAllColumns();
            SetPrimaryKeyColumn(DbTransaction.Connection);

            CheckSnakeCaseOnExecuteCommand();

            CreateTempTable(DbTransaction.Connection);
            PopulateTempTable(DbTransaction.Connection);
            ExecuteMergeCommand(DbTransaction.Connection);

            DropTempTable(DbTransaction.Connection);

            return "Deu boa!!";
        }

        private void SetAllColumns()
        {
            var properties = typeof(TEntity).GetProperties();
            properties = properties.Where(x => !x.GetGetMethod()?.IsVirtual ?? false).ToArray();

            var names = properties.Select(x => x.Name);

            if(SnakeCaseNamingConvention)
                names = names.Select(x => x.ToSnakeCase());

            AllColumns.AddRange(names);
        }

        private void SetPrimaryKeyColumn(IDbConnection dbConnection)
        {
            var query = SqlBuilder.BuildPrimaryKeyQuery(_tableName);

            var sqlCommand = dbConnection.CreateCommand();

            sqlCommand.Transaction = DbTransaction;
            sqlCommand.CommandText = query;

            var pkField = sqlCommand.ExecuteScalar();

            PrimaryKey = pkField?.ToString() ?? string.Empty;

            if(SnakeCaseNamingConvention)
                PrimaryKey = PrimaryKey.ToSnakeCase();
        }

        private void ExecuteMergeCommand(IDbConnection dbConnection)
        {
            var allColumnsWithoutIgnoredInsert = AllColumns.Except(IgnoredOnInsertOperation).ToList();
            var allColumnsWithoutIgnoredUpdate = UpdatedColumns.Where(x => !x.Equals(PrimaryKey, StringComparison.OrdinalIgnoreCase)).ToList();

            var stringBuilderQuery = SqlBuilder.BuildMerge(_tableName, MergedColumns, allColumnsWithoutIgnoredUpdate, allColumnsWithoutIgnoredInsert, Conditions, StatusColumn);
            var sqlCommand = dbConnection.CreateCommand();

            sqlCommand.Transaction = DbTransaction;
            sqlCommand.CommandText = stringBuilderQuery.ToString();

            sqlCommand.ExecuteNonQuery();
        }

        private void CheckSnakeCaseOnExecuteCommand()
        {
            if(!SnakeCaseNamingConvention)
                return;

            _tableName = _tableName.ToSnakeCase();
            StatusColumn = StatusColumn.ToSnakeCase();

            IgnoredOnInsertOperation = IgnoredOnInsertOperation.Select(x => x.ToSnakeCase()).ToList();
            UpdatedColumns = UpdatedColumns.Select(x => x.ToSnakeCase()).ToList();
            MergedColumns = MergedColumns.Select(x => x.ToSnakeCase()).ToList();

            Conditions = Conditions.Select(x => {
                var fields = x.Fields.Select(y => y.ToSnakeCase()).ToList();
                return new ConditionBuilder(fields, x.ConditionType, x.ConditionOperator);
            }).ToList();
        }

        private void CreateTempTable(IDbConnection dbConnection)
        {
            var sqlCommand = dbConnection.CreateCommand();

            sqlCommand.Transaction = DbTransaction;
            sqlCommand.CommandText = SqlBuilder.BuildTempTable(_tableName);

            sqlCommand.ExecuteNonQuery();
        }

        private void PopulateTempTable(IDbConnection dbConnection)
        {
            DataTable table = new()
            {
                TableName = $"#{_tableName}"
            };

            using var bulkInsert = new SqlBulkCopy(dbConnection as SqlConnection, SqlBulkCopyOptions.Default, DbTransaction as SqlTransaction);
            bulkInsert.DestinationTableName = table.TableName;

            using var dataReader = new ObjectDataReader<TEntity>(DataSource.GetEnumerator());
            bulkInsert.WriteToServer(dataReader);
        }

        private void DropTempTable(IDbConnection dbConnection)
        {
            var sqlCommand = dbConnection.CreateCommand();

            sqlCommand.Transaction = DbTransaction;
            sqlCommand.CommandText = SqlBuilder.BuildDropTempTable(_tableName);

            sqlCommand.ExecuteNonQuery();
        }

        private static List<string> GetColumns(Expression<Func<TEntity, object>> expressions)
        {
            var names = ExpressionExtension.GetMemberNames(expressions);
            if (names != null && names.Any())
                return names;

            return new();
        }
    }
}