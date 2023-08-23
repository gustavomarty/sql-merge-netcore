using System.Data;
using SqlComplexOperations.Models;
using SqlComplexOperations.Services;
using SqlComplexOperations.Extensions;
using SqlComplexOperations.Models.Enumerators;
using System.Linq.Expressions;
using SqlComplexOperations.Models.Output;
using SqlComplexOperations.Exceptions;

namespace SqlComplexOperations
{
    /// <summary>
    /// Classe para construir o comando SQL Merge.
    /// </summary>
    public class MergeBuilder : IMergeBuilder
    {
        private readonly IDatabaseService _databaseService;

        public MergeBuilder(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        /// <summary>
        /// Cria uma nova instancia do MergeBuilder.
        /// </summary>
        /// <remarks>
        /// O Tipo <see cref="TEntity"/> é a entidade (Banco) onde o merge será executado.
        /// </remarks>
        public MergeBuilder<TEntity> Create<TEntity>() where TEntity : class
        {
            return new MergeBuilder<TEntity>(_databaseService);
        }

        /// <summary>
        /// Cria uma nova instancia do MergeBuilder.
        /// </summary>
        /// <param name="tableName">Caso necessario voce pode passar o tableName</param>
        /// <remarks>
        /// O Tipo <see cref="TEntity"/> é a entidade (Banco) onde o merge será executado.
        /// </remarks>
        public MergeBuilder<TEntity> Create<TEntity>(string tableName) where TEntity : class
        {
            return new MergeBuilder<TEntity>(_databaseService, tableName);
        }
    }

    /// <summary>
    /// Classe para construir o comando SQL Merge.
    /// </summary>
    /// <typeparam name="TEntity">
    /// O Tipo <see cref="TEntity"/> é a entidade (Banco) onde o merge será executado.
    /// </typeparam>
    public class MergeBuilder<TEntity> where TEntity : class
    {
        private readonly IDatabaseService _databaseService;

        private string _tableName;

        private string StatusColumn { get; set; } = string.Empty;
        private string DbSchema { get; set; } = string.Empty;
        private bool UseEnumStatus { get; set; } = false;
        private string PrimaryKey { get; set; } = string.Empty;
        private bool SnakeCaseNamingConvention { get; set; }

        private List<TEntity> DataSource { get; set; } = new();
        private IDbTransaction? DbTransaction { get; set; }

        private List<string> AllColumns { get; set; } = new();
        private List<string> MergedColumns { get; set; } = new();
        private List<string> UpdatedColumns { get; set; } = new();
        private List<string> IgnoredOnInsertOperation { get; set; } = new();
        private List<string> AllColumnsInDatabaseOrder { get; set; } = new();

        private ResponseType ResponseTypeValue { get; set; } = ResponseType.ROW_COUNT;

        private List<ConditionBuilder> Conditions { get; set; } = new();

        /// <summary>
        /// Cria uma nova instancia do MergeBuilder.
        /// </summary>
        /// <remarks>
        /// O Tipo <see cref="TEntity"/> é a entidade (Banco) onde o merge será executado.
        /// </remarks>
        public MergeBuilder(IDatabaseService databaseService)
        {
            _databaseService = databaseService;

            _tableName = typeof(TEntity).Name;
        }

        /// <summary>
        /// Cria uma nova instancia do MergeBuilder.
        /// </summary>
        /// <param name="tableName">Caso necessario voce pode passar o tableName</param>
        /// <remarks>
        /// O Tipo <see cref="TEntity"/> é a entidade (Banco) onde o merge será executado.
        /// </remarks>
        public MergeBuilder(IDatabaseService databaseService, string tableName)
        {
            _databaseService = databaseService;

            _tableName = tableName;
        }

        /// <summary>
        /// <para>[Opcional]</para>
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
        /// <para>[Opcional]</para>
        /// Define um database schema para rodar os comandos. (Default = FALSE).
        /// </summary>
        /// <remarks>
        /// Exemplo:
        /// select * from table_name -> select * from schema.table_name 
        /// </remarks>
        public MergeBuilder<TEntity> UseDatabaseSchema(string schema)
        {
            DbSchema = schema;
            return this;
        }

        /// <summary>
        /// <para>[Opcional]</para>
        /// Caso queira usar uma coluna(ENUM BulkStatus) de status para executar o merge. (Default = FALSE).
        /// </summary>
        /// <param name="useStringType">Quando o parametro for true ele salvara na coluna de status uma string quando for false ele salvara um int</param>
        /// <param name="expression">A coluna deve estar dentro de <see cref="TEntity"/> e deve ser do tipo <see cref="BulkMergeStatus"/></param>
        /// <returns>
        /// Retorna o MergeBuilder atual.
        /// </returns>
        public MergeBuilder<TEntity> UseStatusConfiguration(bool useStringType, Expression<Func<TEntity, BulkMergeStatus>> expression)
        {
            UseEnumStatus = !useStringType;

            var member = (MemberExpression)expression.Body;
            StatusColumn = member.Member.Name;

            return this;
        }

        /// <summary>
        /// <para>[Opcional]</para>
        /// Define o tipo de resposta na execução do bulk. (Default = ROW_COUNT).
        /// </summary>
        /// <param name="responseType">Tipo do response deve ser do enumerador <see cref="ResponseType"/>.</param>
        /// <returns>
        /// Retorna o MergeBuilder atual.
        /// </returns>
        public MergeBuilder<TEntity> SetResponseType(ResponseType responseType)
        {
            ResponseTypeValue = responseType;
            return this;
        }

        /// <summary>
        /// <para>[Opcional]</para>
        /// Caso queira usar uma coluna(STRING) de status para executar o merge. (Default = FALSE).
        /// </summary>
        /// <param name="useStringType">Quando o parametro for true ele salvara na coluna de status uma string quando for false ele salvara um int</param>
        /// <param name="expression">A coluna deve estar dentro de <see cref="TEntity"/></param>
        /// <returns>
        /// Retorna o MergeBuilder atual.
        /// </returns>
        public MergeBuilder<TEntity> UseStatusConfiguration(bool useStringType, Expression<Func<TEntity, string>> expression)
        {
            UseEnumStatus = !useStringType;

            var member = (MemberExpression)expression.Body;
            StatusColumn = member.Member.Name;

            return this;
        }

        /// <summary>
        /// <para>[Obrigatório]</para>
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
        /// <para>[Obrigatório]</para>
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
        /// <para>[Opcional]</para>
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
        /// <param name="conditionType">Deve ser um enumerador do tipo <see cref="ConditionType"/></param>
        /// <param name="conditionOperator">Deve ser um enumerador do tipo <see cref="ConditionOperator"/></param>
        /// <param name="expression">As colunas deve estar dentro de <see cref="TEntity"/></param>
        /// <returns>
        /// Retorna o MergeBuilder atual.
        /// </returns>
        public MergeBuilder<TEntity> WithCondition(ConditionType conditionType, ConditionOperator conditionOperator, Expression<Func<TEntity, object>> expression)
        {
            var columns = GetColumns(expression);

            var condition = new ConditionBuilder(columns ?? new List<string>(), conditionType, conditionOperator);
            Conditions.Add(condition);

            return this;
        }

        /// <summary>
        /// <para>[Opcional]</para>
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
        /// <param name="conditionType">Deve ser um enumerador do tipo <see cref="ConditionType"/></param>
        /// <param name="expression">As colunas deve estar dentro de <see cref="TEntity"/></param>
        /// <returns>
        /// Retorna o MergeBuilder atual.
        /// </returns>
        public MergeBuilder<TEntity> WithCondition<TFieldType>(ConditionType conditionType, Expression<Func<TEntity, TFieldType>> expression)
            where TFieldType : struct
        {
            var member = (MemberExpression)expression.Body;
            var column = member.Member.Name;

            var condition = new ConditionBuilder(new List<string> { column }, conditionType, ConditionOperator.NONE);
            Conditions.Add(condition);

            return this;
        }

        /// <summary>
        /// <para>[Opcional]</para>
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
        /// <param name="conditionType">Deve ser um enumerador do tipo <see cref="ConditionType"/></param>
        /// <param name="expression">As colunas deve estar dentro de <see cref="TEntity"/></param>
        /// <returns>
        /// Retorna o MergeBuilder atual.
        /// </returns>
        public MergeBuilder<TEntity> WithCondition(ConditionType conditionType, Expression<Func<TEntity, string>> expression)
        {
            var member = (MemberExpression)expression.Body;
            var column = member.Member.Name;

            var condition = new ConditionBuilder(new List<string> { column }, conditionType, ConditionOperator.NONE);
            Conditions.Add(condition);

            return this;
        }

        /// <summary>
        /// <para>[Opcional]</para>
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
        /// <para>[Obrigatório]</para>
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
        /// <para>[Obrigatório]</para>
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
            DbTransaction = transaction;
            return this;
        }

        /// <summary>
        /// Executa o comando montado até agora.
        /// </summary>
        /// <returns>
        /// Retorna o resultado do merge.
        /// </returns>
        public async Task<OutputModel> Execute()
        {
            ValidateBuilderPreExecute();
            CheckSnakeCaseOnExecuteCommand();
            
            SetAllColumnsInDatabaseOrder(DbTransaction!);
            SetPrimaryKeyColumn(DbTransaction!);
            CreateTempTable(DbTransaction!);
            await PopulateTempTable(DbTransaction!);
            var result = ExecuteMergeCommand(DbTransaction!);

            DropTempTable(DbTransaction!);

            return result;
        }

        private void ValidateBuilderPreExecute()
        {
            if(DbTransaction == null)
                throw new InvalidDbTransactionException<TEntity>("You need to inform the DbTransaction, call the method SetTransaction(IDbTransaction transaction) before execute merge.", DbTransaction, this);

            if(DbTransaction.Connection == null)
                throw new InvalidDbTransactionException<TEntity>("The DbTransaction informed is without one active Connection.", DbTransaction, this);

            if(!MergedColumns.Any())
                throw new InvalidMergedColumnsException<TEntity>("You need to inform the MergedColumns, call the method SetMergeColumns(...) before execute merge.", MergedColumns, this);

            if(!UpdatedColumns.Any())
                throw new InvalidUpdatedColumnsException<TEntity>("You need to inform the UpdatedColumns, call the method SetUpdatedColumns(...) before execute merge.", UpdatedColumns, this);

            if(!DataSource.Any())
                throw new InvalidDataSourceException<TEntity>("You need to inform the DataSource, call the method SetDataSource(...) before execute merge.", DataSource, this);
        }

        private void SetPrimaryKeyColumn(IDbTransaction dbTransaction)
        {
            var query = SqlBuilder.BuildPrimaryKeyQuery(_tableName, DbSchema);

            var result = _databaseService.ExecuteScalarCommand(dbTransaction, query);

            PrimaryKey = result?.ToString() ?? string.Empty;

            if(SnakeCaseNamingConvention)
                PrimaryKey = PrimaryKey.ToSnakeCase();
        }

        private void SetAllColumnsInDatabaseOrder(IDbTransaction dbTransaction)
        {
            var query = SqlBuilder.BuildAllColumnsDbOrderQuery(_tableName, DbSchema);
            var result = _databaseService.ExecuteReaderCommand(dbTransaction, query);

            AllColumnsInDatabaseOrder = result;
            AllColumns = result;
        }

        private OutputModel ExecuteMergeCommand(IDbTransaction dbTransaction)
        {
            var mergeBuilderSqlConfiguration = new MergeBuilderSqlConfiguration
            {
                TableName = _tableName,
                Schema = DbSchema,
                AllColumns = AllColumns,
                MergedColumns = MergedColumns,
                UpdatedColumns = RemoveIgnoredAndDuplicatedUpdateColumns(UpdatedColumns),
                InsertedColumns = RemoveIgnoredAndDuplicatedInsertColumns(AllColumns),
                Conditions = Conditions,
                StatusColumn = StatusColumn,
                UseEnumStatus = UseEnumStatus,
                ResponseType = ResponseTypeValue
            };

            var stringQuery = SqlBuilder.BuildMerge(mergeBuilderSqlConfiguration);
            
            OutputModel result = ResponseTypeValue switch
            {
                ResponseType.NONE => _databaseService.ExecuteMergeCommand(dbTransaction, stringQuery),
                ResponseType.SIMPLE => _databaseService.ExecuteMergeCommandSimple(dbTransaction, stringQuery),
                ResponseType.COMPLETE => _databaseService.ExecuteMergeCommandComplete<TEntity>(dbTransaction, stringQuery, AllColumns, SnakeCaseNamingConvention),
                _ => _databaseService.ExecuteMergeCommandRowCount(dbTransaction, stringQuery),
            };

            return result;
        }

        private List<string> RemoveIgnoredAndDuplicatedInsertColumns(List<string> listColumn)
        {
            ValidateAndRemoveStatusColumn(listColumn);
            listColumn = listColumn.Except(IgnoredOnInsertOperation).ToList();
            return listColumn;
        }

        private List<string> RemoveIgnoredAndDuplicatedUpdateColumns(List<string> listColumn)
        {
            ValidateAndRemoveStatusColumn(listColumn);
            listColumn = listColumn.Where(x => !x.Equals(PrimaryKey, StringComparison.OrdinalIgnoreCase)).ToList();
            return listColumn;
        }

        private void ValidateAndRemoveStatusColumn(List<string> listColumns)
        {
            if (StatusColumn != null)
            {
                listColumns.Remove(StatusColumn);
                listColumns.Remove(StatusColumn.ToSnakeCase());
            }
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

        private void CreateTempTable(IDbTransaction dbTransaction)
        {
            var sqlCommand = SqlBuilder.BuildTempTable(_tableName, DbSchema);

            _databaseService.ExecuteNonQueryCommand(dbTransaction, sqlCommand);
        }

        private async Task PopulateTempTable(IDbTransaction dbTransaction)
        {
            await _databaseService.PopulateTempTable(dbTransaction, DataSource, $"#{_tableName}", DbSchema, AllColumnsInDatabaseOrder, SnakeCaseNamingConvention);
        }

        private void DropTempTable(IDbTransaction dbTransaction)
        {
            var sqlCommand = SqlBuilder.BuildDropTempTable(_tableName, DbSchema);

            _databaseService.ExecuteNonQueryCommand(dbTransaction, sqlCommand);
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