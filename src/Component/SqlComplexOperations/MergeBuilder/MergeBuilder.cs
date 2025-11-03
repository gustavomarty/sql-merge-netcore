using System.Data;
using SqlComplexOperations.Models;
using SqlComplexOperations.Services;
using SqlComplexOperations.Extensions;
using SqlComplexOperations.Models.Enumerators;
using System.Linq.Expressions;
using SqlComplexOperations.Models.Output;
using SqlComplexOperations.Exceptions;
using SqlComplexOperations.Attributes;
using SqlComplexOperations.Common;

namespace SqlComplexOperations
{
    /// <summary>
    /// Classe para construir o comando SQL Merge.
    /// </summary>
    public class MergeBuilder : IMergeBuilder
    {
        private readonly IDatabaseService _databaseService;
        private readonly DatabaseType _databaseType;

        /// <summary>
        /// Construtor
        /// </summary>
        /// <param name="databaseService"></param>
        /// <param name="databaseType"></param>
        public MergeBuilder(IDatabaseService databaseService, DatabaseType databaseType)
        {
            _databaseService = databaseService;
            _databaseType = databaseType;
        }

        /// <summary>
        /// Cria uma nova instancia do MergeBuilder.
        /// </summary>
        /// <remarks>
        /// O Tipo <see cref="!:TEntity"/> é a entidade (Banco) onde o merge será executado.
        /// </remarks>
        public MergeBuilder<TEntity> Create<TEntity>() where TEntity : class
        {
            return new MergeBuilder<TEntity>(_databaseService, _databaseType);
        }

        /// <summary>
        /// Cria uma nova instancia do MergeBuilder.
        /// </summary>
        /// <param name="tableName">Caso necessario voce pode passar o tableName</param>
        /// <remarks>
        /// O Tipo <see cref="!:TEntity"/> é a entidade (Banco) onde o merge será executado.
        /// </remarks>
        public MergeBuilder<TEntity> Create<TEntity>(string tableName) where TEntity : class
        {
            return new MergeBuilder<TEntity>(_databaseService, _databaseType, tableName);
        }
    }

    /// <summary>
    /// Classe para construir o comando SQL Merge.
    /// </summary>
    /// <typeparam name="TEntity">
    /// O Tipo <see cref="!:TEntity"/> é a entidade (Banco) onde o merge será executado.
    /// </typeparam>
    public class MergeBuilder<TEntity> where TEntity : class
    {
        private readonly IDatabaseService _databaseService;
        private readonly DatabaseType _databaseType;
        private string _tableName;

        private string StatusColumn { get; set; } = string.Empty;
        private string DbSchema { get; set; } = string.Empty;
        private bool UseEnumStatus { get; set; } = false;
        private string PrimaryKey { get; set; } = string.Empty;
        private bool SnakeCaseNamingConvention { get; set; }
        private bool UsePropertyNameAttr { get; set; }
        private bool UseDeleteClause { get; set; }
        private bool EnumAsString { get; set; }

        private List<TEntity> DataSource { get; set; } = [];
        private IDbTransaction? DbTransaction { get; set; }

        private List<string> AllColumns { get; set; } = [];
        private List<string> MergedColumns { get; set; } = [];
        private List<string> UpdatedColumns { get; set; } = [];
        private List<string> IgnoredOnInsertOperation { get; set; } = [];
        private List<string> AllColumnsInDatabaseOrder { get; set; } = [];

        private ResponseType ResponseTypeValue { get; set; } = ResponseType.NONE;

        private List<ConditionBuilder> Conditions { get; set; } = [];

        /// <summary>
        /// Cria uma nova instancia do MergeBuilder.
        /// </summary>
        /// <remarks>
        /// O Tipo <see cref="!:TEntity"/> é a entidade (Banco) onde o merge será executado.
        /// </remarks>
        public MergeBuilder(IDatabaseService databaseService, DatabaseType databaseType)
        {
            _databaseService = databaseService;
            _databaseType = databaseType;
            _tableName = typeof(TEntity).Name;
        }

        /// <summary>
        /// Cria uma nova instancia do MergeBuilder.
        /// </summary>
        /// <param name="databaseService"></param>
        /// <param name="databaseType"></param>
        /// <param name="tableName">Caso necessario voce pode passar o tableName</param>
        /// <remarks>
        /// O Tipo <see cref="!:TEntity"/> é a entidade (Banco) onde o merge será executado.
        /// </remarks>
        public MergeBuilder(IDatabaseService databaseService, DatabaseType databaseType, string tableName)
        {
            _databaseService = databaseService;
            _databaseType = databaseType;
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
        /// Pega o nome das propriedades de acordo com o Attribute <see cref="PropertyNameAttribute"/>. (Default = FALSE).
        /// <br></br>
        /// Em caso de uso do atributo o uso do <see cref="SnakeCaseNamingConvention"/> será ignorado.
        /// </summary>
        public MergeBuilder<TEntity> UsePropertyNameAttribute()
        {
            UsePropertyNameAttr = true;
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
        /// Deleta os dados da tabela destino caso os mesmos nao existam no data source. (Default = FALSE).
        /// </summary>
        public MergeBuilder<TEntity> DeleteWhenDataIsNotInDataSource()
        {
            UseDeleteClause = true;
            return this;
        }

        /// <summary>
        /// <para>[Opcional]</para>
        /// Caso queira usar uma coluna(ENUM BulkStatus) de status para executar o merge. (Default = FALSE).
        /// </summary>
        /// <param name="useStringType">Quando o parametro for true ele salvara na coluna de status uma string quando for false ele salvara um int</param>
        /// <param name="expression">A coluna deve estar dentro de <see cref="!:TEntity"/> e deve ser do tipo <see cref="BulkMergeStatus"/></param>
        /// <returns>
        /// Retorna o MergeBuilder atual.
        /// </returns>
        public MergeBuilder<TEntity> UseStatusConfiguration(bool useStringType, Expression<Func<TEntity, BulkMergeStatus>> expression)
        {
            UseEnumStatus = !useStringType;

            var member = (MemberExpression)expression.Body;
            StatusColumn = member.Member.GetMemberName(UsePropertyNameAttr);

            return this;
        }

        /// <summary>
        /// <para>[Opcional]</para>
        /// Caso queira usar uma coluna(STRING) de status para executar o merge. (Default = FALSE).
        /// </summary>
        /// <param name="useStringType">Quando o parametro for true ele salvara na coluna de status uma string quando for false ele salvara um int</param>
        /// <param name="expression">A coluna deve estar dentro de <see cref="!:TEntity"/></param>
        /// <returns>
        /// Retorna o MergeBuilder atual.
        /// </returns>
        public MergeBuilder<TEntity> UseStatusConfiguration(bool useStringType, Expression<Func<TEntity, string>> expression)
        {
            UseEnumStatus = !useStringType;

            var member = (MemberExpression)expression.Body;
            StatusColumn = member.Member.GetMemberName(UsePropertyNameAttr);

            return this;
        }

        /// <summary>
        /// <para>[Opcional]</para>
        /// Define o tipo de resposta na execução do bulk. (Default = NONE).
        /// 
        /// [POSTGRESQL]
        /// O PostgreSQL tem uma limitação no comando merge, nesse caso o unico suporte para PostgreSQL seria: ResponseType.NONE
        /// 
        /// </summary>
        /// <param name="responseType">Tipo do response deve ser do enumerador <see cref="ResponseType"/>.</param>
        /// <returns>
        /// Retorna o MergeBuilder atual.
        /// </returns>
        public MergeBuilder<TEntity> SetResponseType(ResponseType responseType)
        {
            if(_databaseType == DatabaseType.POSTGRES_SQL && responseType != ResponseType.NONE)
            {
                throw new InvalidConfigurationPostgreSqlException<TEntity>($"The response type: {responseType.DisplayName()} is not supported for postgreSQL. Please use: 'ResponseType.NONE'", "SetResponseType");
            }

            ResponseTypeValue = responseType;
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
        /// <param name="expression">As colunas deve estar dentro de <see cref="!:TEntity"/></param>
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
        /// <param name="expression">As colunas deve estar dentro de <see cref="!:TEntity"/></param>
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
        /// <param name="expression">As colunas deve estar dentro de <see cref="!:TEntity"/></param>
        /// <returns>
        /// Retorna o MergeBuilder atual.
        /// </returns>
        public MergeBuilder<TEntity> WithCondition(ConditionType conditionType, ConditionOperator conditionOperator, Expression<Func<TEntity, object>> expression)
        {
            var columns = GetColumns(expression);

            var condition = new ConditionBuilder(columns ?? [], conditionType, conditionOperator);
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
        /// <param name="expression">As colunas deve estar dentro de <see cref="!:TEntity"/></param>
        /// <returns>
        /// Retorna o MergeBuilder atual.
        /// </returns>
        public MergeBuilder<TEntity> WithCondition<TFieldType>(ConditionType conditionType, Expression<Func<TEntity, TFieldType>> expression)
            where TFieldType : struct
        {
            var member = (MemberExpression)expression.Body;
            var column = member.Member.GetMemberName(UsePropertyNameAttr);

            var condition = new ConditionBuilder([column], conditionType, ConditionOperator.NONE);
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
        /// <param name="expression">As colunas deve estar dentro de <see cref="!:TEntity"/></param>
        /// <returns>
        /// Retorna o MergeBuilder atual.
        /// </returns>
        public MergeBuilder<TEntity> WithCondition(ConditionType conditionType, Expression<Func<TEntity, string>> expression)
        {
            var member = (MemberExpression)expression.Body;
            var column = member.Member.GetMemberName(UsePropertyNameAttr);

            var condition = new ConditionBuilder([column], conditionType, ConditionOperator.NONE);
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
        /// <param name="expression">As colunas deve estar dentro de <see cref="!:TEntity"/></param>
        /// <returns>
        /// Retorna o MergeBuilder atual.
        /// </returns>
        public MergeBuilder<TEntity> SetIgnoreOnInsertOperation(Expression<Func<TEntity, object>> expression)
        {
            IgnoredOnInsertOperation.AddRange(GetColumns(expression));
            return this;
        }

        /// <summary>
        /// <para>[Obrigatório]</para>
        /// Configura os dados que iram ser utilizados no comando merge.
        /// <br>
        /// Os dados devem ser uma lista de <see cref="!:TEntity"/>.
        /// </br>
        /// </summary>
        /// <param name="datasource">Uma lista de entidades do tipo <see cref="!:TEntity"/> (Não persistidas no banco) para realizar o merge.</param>
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
        public virtual MergeBuilder<TEntity> SetTransaction(IDbTransaction transaction)
        {
            DbTransaction = transaction;
            return this;
        }

        /// <summary>
        /// <para>[Opcional]</para>
        /// Torna os enumeradores como string na base.
        /// </summary>
        /// <remarks>
        /// Default = false.
        /// </remarks>
        public MergeBuilder<TEntity> UseEnumAsString()
        {
            EnumAsString = true;
            return this;
        }

        /// <summary>
        /// Executa o comando montado até agora.
        /// </summary>
        /// <returns>
        /// Retorna o resultado do merge.
        /// </returns>
        public virtual async Task<OutputModel> Execute()
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
            if (DbTransaction == null)
                throw new InvalidDbTransactionException<TEntity>("You need to inform the DbTransaction, call the method SetTransaction(IDbTransaction transaction) before execute merge.", typeof(MergeBuilder).ToString());

            if (DbTransaction.Connection == null)
                throw new InvalidDbTransactionException<TEntity>("The DbTransaction informed is without one active Connection.", DbTransaction, typeof(MergeBuilder).ToString());

            if (MergedColumns.Count == 0)
                throw new InvalidMergedColumnsException<TEntity>("You need to inform the MergedColumns, call the method SetMergeColumns(...) before execute merge.", MergedColumns, this);

            if (UpdatedColumns.Count == 0)
                throw new InvalidUpdatedColumnsException<TEntity>("You need to inform the UpdatedColumns, call the method SetUpdatedColumns(...) before execute merge.", UpdatedColumns, this);

            if (DataSource.Count == 0)
                throw new InvalidDataSourceException<TEntity>("You need to inform the DataSource, call the method SetDataSource(...) before execute merge.", DataSource, typeof(MergeBuilder).ToString());

            if (UsePropertyNameAttr && typeof(TEntity).GetProperties().Select(x => x.GetPropName(true)).Any(x => string.IsNullOrWhiteSpace(x)))
                throw new InvalidPropertyNameConfigurationException<TEntity>("You are using the 'UsePropertyNameAttribute' configuration, all attributes of your entity needs be mapped.", typeof(MergeBuilder).ToString());
        }

        private void SetPrimaryKeyColumn(IDbTransaction dbTransaction)
        {
            string query = _databaseType switch
            {
                DatabaseType.MICROSOFT_SQL_SERVER => SqlBuilder.BuildPrimaryKeyQuery(_tableName, DbSchema),
                DatabaseType.POSTGRES_SQL => PostgreSqlBuilder.BuildPrimaryKeyQuery(_tableName, DbSchema),
                _ => SqlBuilder.BuildPrimaryKeyQuery(_tableName, DbSchema),
            };

            var result = _databaseService.ExecuteScalarCommand(dbTransaction, query);

            PrimaryKey = result?.ToString() ?? string.Empty;

            if (SnakeCaseNamingConvention)
                PrimaryKey = PrimaryKey.ToSnakeCase();
        }

        private void SetAllColumnsInDatabaseOrder(IDbTransaction dbTransaction)
        {
            string query = _databaseType switch
            {
                DatabaseType.MICROSOFT_SQL_SERVER => SqlBuilder.BuildAllColumnsDbOrderQuery(_tableName, DbSchema),
                DatabaseType.POSTGRES_SQL => PostgreSqlBuilder.BuildAllColumnsDbOrderQuery(_tableName, DbSchema),
                _ => SqlBuilder.BuildAllColumnsDbOrderQuery(_tableName, DbSchema),
            };
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
                ResponseType = ResponseTypeValue,
                UseDeleteClause = UseDeleteClause
            };

            string stringQuery = _databaseType switch
            {
                DatabaseType.MICROSOFT_SQL_SERVER => SqlBuilder.BuildMerge(mergeBuilderSqlConfiguration),
                DatabaseType.POSTGRES_SQL => PostgreSqlBuilder.BuildMerge(mergeBuilderSqlConfiguration),
                _ => SqlBuilder.BuildMerge(mergeBuilderSqlConfiguration),
            };

            OutputModel result = ResponseTypeValue switch
            {
                ResponseType.NONE => _databaseService.ExecuteMergeCommand(dbTransaction, stringQuery),
                ResponseType.SIMPLE => _databaseService.ExecuteMergeCommandSimple(dbTransaction, stringQuery),
                ResponseType.COMPLETE => _databaseService.ExecuteMergeCommandComplete<TEntity>(dbTransaction, stringQuery, AllColumns, SnakeCaseNamingConvention, UsePropertyNameAttr),
                _ => _databaseService.ExecuteMergeCommandRowCount(dbTransaction, stringQuery),
            };

            if (UseDeleteClause && _databaseType == DatabaseType.POSTGRES_SQL)
            {
                var sql = PostgreSqlBuilder.GetDeleteScript(_tableName, MergedColumns, StatusColumn, UseEnumStatus);
                _databaseService.ExecuteNonQueryCommand(dbTransaction, sql);
            }

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
            if (!SnakeCaseNamingConvention)
                return;

            if (UsePropertyNameAttr)
                return;

            _tableName = _tableName.ToSnakeCase();
            StatusColumn = StatusColumn.ToSnakeCase();

            IgnoredOnInsertOperation = IgnoredOnInsertOperation.Select(x => x.ToSnakeCase()).ToList();
            UpdatedColumns = UpdatedColumns.Select(x => x.ToSnakeCase()).ToList();
            MergedColumns = MergedColumns.Select(x => x.ToSnakeCase()).ToList();

            Conditions = Conditions.Select(x =>
            {
                var fields = x.Fields.Select(y => y.ToSnakeCase()).ToList();
                return new ConditionBuilder(fields, x.ConditionType, x.ConditionOperator);
            }).ToList();
        }

        private void CreateTempTable(IDbTransaction dbTransaction)
        {
            string sqlCommand = _databaseType switch
            {
                DatabaseType.MICROSOFT_SQL_SERVER => SqlBuilder.BuildTempTable(_tableName, DbSchema),
                DatabaseType.POSTGRES_SQL => PostgreSqlBuilder.BuildTempTable(_tableName, DbSchema),
                _ => SqlBuilder.BuildTempTable(_tableName, DbSchema),
            };

            _databaseService.ExecuteNonQueryCommand(dbTransaction, sqlCommand);
        }

        private async Task PopulateTempTable(IDbTransaction dbTransaction)
        {
            switch (_databaseType)
            {
                case DatabaseType.MICROSOFT_SQL_SERVER:
                    await _databaseService.BulkInsert(dbTransaction, DataSource, $"#{_tableName}", DbSchema, AllColumnsInDatabaseOrder, SnakeCaseNamingConvention, UsePropertyNameAttr);
                break;
                case DatabaseType.POSTGRES_SQL:
                    await _databaseService.BulkInsertToPostgre(dbTransaction, DataSource, $"{_tableName}_temp", DbSchema, AllColumnsInDatabaseOrder, SnakeCaseNamingConvention, UsePropertyNameAttr, EnumAsString);
                break;
                default:
                    await _databaseService.BulkInsert(dbTransaction, DataSource, $"#{_tableName}", DbSchema, AllColumnsInDatabaseOrder, SnakeCaseNamingConvention, UsePropertyNameAttr);
                break;
            }
        }

        private void DropTempTable(IDbTransaction dbTransaction)
        {
            string sqlCommand = _databaseType switch
            {
                DatabaseType.MICROSOFT_SQL_SERVER => SqlBuilder.BuildDropTempTable(_tableName, DbSchema),
                DatabaseType.POSTGRES_SQL => PostgreSqlBuilder.BuildDropTempTable(_tableName, DbSchema),
                _ => SqlBuilder.BuildDropTempTable(_tableName, DbSchema),
            };

            _databaseService.ExecuteNonQueryCommand(dbTransaction, sqlCommand);
        }

        private List<string> GetColumns(Expression<Func<TEntity, object>> expressions)
        {
            var names = ExpressionExtension.GetMemberNames(UsePropertyNameAttr, expressions);
            if (names != null && names.Count != 0)
                return names.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();

            return [];
        }
    }
}