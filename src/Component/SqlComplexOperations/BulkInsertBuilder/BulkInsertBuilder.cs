using System.Data;
using SqlComplexOperations.Common;
using SqlComplexOperations.Services;
using SqlComplexOperations.Exceptions;
using SqlComplexOperations.Extensions;
using SqlComplexOperations.Models.Enumerators;

namespace SqlComplexOperations
{
    /// <summary>
    /// Classe para construir o comando SQL Copy.
    /// </summary>
    public class BulkInsertBuilder : IBulkInsertBuilder
    {
        private readonly IDatabaseService _databaseService;
        private readonly DatabaseType _databaseType;

        /// <summary>
        /// Construtor.
        /// </summary>
        /// <param name="databaseService"></param>
        /// <param name="databaseType"></param>
        public BulkInsertBuilder(IDatabaseService databaseService, DatabaseType databaseType)
        {
            _databaseService = databaseService;
            _databaseType = databaseType;
        }

        /// <summary>
        /// Cria uma nova instancia do BulkInsertBuilder.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <remarks>
        /// O Tipo <see cref="!:TEntity"/> é a entidade (Banco) onde o bulk insert será executado.
        /// </remarks>
        public BulkInsertBuilder<TEntity> Create<TEntity>() where TEntity : class
        {
            return new BulkInsertBuilder<TEntity>(_databaseService, _databaseType);
        }

        /// <summary>
        /// Cria uma nova instancia do BulkInsertBuilder.
        /// </summary>
        /// <param name="tableName">Caso necessario voce pode passar o tableName</param>
        /// <typeparam name="TEntity"></typeparam>
        /// <remarks>
        /// O Tipo <see cref="!:TEntity"/> é a entidade (Banco) onde o bulk insert será executado.
        /// </remarks>
        public BulkInsertBuilder<TEntity> Create<TEntity>(string tableName) where TEntity : class
        {
            return new BulkInsertBuilder<TEntity>(_databaseService, _databaseType, tableName);
        }
    }

    /// <summary>
    /// Classe para construir o comando SQL Merge.
    /// </summary>
    /// <typeparam name="TEntity">
    /// O Tipo <see cref="TEntity"/> é a entidade (Banco) onde o merge será executado.
    /// </typeparam>
    public class BulkInsertBuilder<TEntity> where TEntity : class
    {
        private readonly IDatabaseService _databaseService;
        private readonly DatabaseType _databaseType;
        private string _tableName;

        private List<TEntity> DataSource { get; set; } = [];
        private IDbTransaction? DbTransaction { get; set; }
        private string DbSchema { get; set; } = string.Empty;
        private bool SnakeCaseNamingConvention { get; set; }
        private bool UsePropertyNameAttr { get; set; }
        private bool EnumAsString { get; set; }
        private List<string> AllColumnsInDatabaseOrder { get; set; } = [];


        /// <summary>
        /// Cria uma nova instancia do MergeBuilder.
        /// </summary>
        public BulkInsertBuilder(IDatabaseService databaseService, DatabaseType databaseType)
        {
            _databaseService = databaseService;
            _databaseType = databaseType;

            _tableName = typeof(TEntity).Name;
        }

        /// <summary>
        /// Cria uma nova instancia do MergeBuilder.
        /// </summary>
        /// <param name="tableName">Caso necessario voce pode passar o tableName</param>
        /// <param name="databaseType"></param>
        /// <param name="databaseService"></param>
        public BulkInsertBuilder(IDatabaseService databaseService, DatabaseType databaseType, string tableName)
        {
            _databaseService = databaseService;
            _databaseType = databaseType;
            _tableName = tableName;
        }

        /// <summary>
        /// <para>[Opcional]</para>
        /// Define um database schema para rodar os comandos. (Default = FALSE).
        /// </summary>
        /// <remarks>
        /// Exemplo:
        /// select * from table_name -> select * from schema.table_name 
        /// </remarks>
        public BulkInsertBuilder<TEntity> UseDatabaseSchema(string schema)
        {
            DbSchema = schema;
            return this;
        }

        /// <summary>
        /// <para>[Obrigatório]</para>
        /// Configura os dados que iram ser utilizados no comando merge.
        /// <br>
        /// Os dados devem ser uma lista de <see cref="!:TEntity"/>.
        /// </br>
        /// </summary>
        /// <param name="datasource">Uma lista de entidades do tipo <see cref="!:List{TEntity}>"/> (Não persistidas no banco) para realizar o merge.</param>
        /// <returns>
        /// Retorna o MergeBuilder atual.
        /// </returns>
        public BulkInsertBuilder<TEntity> SetDataSource(List<TEntity> datasource)
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
        public virtual BulkInsertBuilder<TEntity> SetTransaction(IDbTransaction transaction)
        {
            DbTransaction = transaction;
            return this;
        }

        /// <summary>
        /// <para>[Opcional]</para>
        /// Torna todos os nomes de atributos/tabelas do banco snake_case. (Default = FALSE).
        /// </summary>
        /// <remarks>
        /// Exemplo:
        /// Entidade.NomeEntidade => entidade.nome_entidade
        /// </remarks>
        public BulkInsertBuilder<TEntity> UseSnakeCaseNamingConvention()
        {
            SnakeCaseNamingConvention = true;
            return this;
        }

        /// <summary>
        /// <para>[Opcional]</para>
        /// Torna os enumeradores como string na base.
        /// </summary>
        /// <remarks>
        /// Default = false.
        /// </remarks>
        public BulkInsertBuilder<TEntity> UseEnumAsString()
        {
            EnumAsString = true;
            return this;
        }

        /// <summary>
        /// <para>[Opcional]</para>
        /// Pega o nome das propriedades de acordo com o Attribute <see cref="!:PropertyNameAttribute"/>. (Default = FALSE).
        /// <br></br>
        /// Em caso de uso do atributo o uso do <see cref="SnakeCaseNamingConvention"/> será ignorado.
        /// </summary>
        public BulkInsertBuilder<TEntity> UsePropertyNameAttribute()
        {
            UsePropertyNameAttr = true;
            return this;
        }

        /// <summary>
        /// Executa o comando montado até agora.
        /// </summary>
        /// <returns>
        /// Retorna o resultado do merge.
        /// </returns>
        public virtual async Task<bool> Execute()
        {
            ValidateBuilderPreExecute();
            CheckSnakeCaseOnExecuteCommand();
            SetAllColumnsInDatabaseOrder(DbTransaction!);

            await ExecuteBulkInsert(DbTransaction!);

            return true;
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
        }

        private void CheckSnakeCaseOnExecuteCommand()
        {
            if (!SnakeCaseNamingConvention)
                return;

            if (UsePropertyNameAttr)
                return;

            _tableName = _tableName.ToSnakeCase();
        }

        private async Task ExecuteBulkInsert(IDbTransaction dbTransaction)
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

        private void ValidateBuilderPreExecute()
        {
            if (DbTransaction == null)
                throw new InvalidDbTransactionException<TEntity>("You need to inform the DbTransaction, call the method SetTransaction(IDbTransaction transaction) before execute merge.", typeof(BulkInsertBuilder).ToString());

            if (DbTransaction.Connection == null)
                throw new InvalidDbTransactionException<TEntity>("The DbTransaction informed is without one active Connection.", DbTransaction, typeof(BulkInsertBuilder).ToString());

            if (DataSource.Count == 0)
                throw new InvalidDataSourceException<TEntity>("You need to inform the DataSource, call the method SetDataSource(...) before execute merge.", DataSource, typeof(BulkInsertBuilder).ToString());

            if (UsePropertyNameAttr && typeof(TEntity).GetProperties().Select(x => x.GetPropName(true)).Any(x => string.IsNullOrWhiteSpace(x)))
                throw new InvalidPropertyNameConfigurationException<TEntity>("You are using the 'UsePropertyNameAttribute' configuration, all attributes of your entity needs be mapped.", typeof(BulkInsertBuilder).ToString());

        }
    }
}