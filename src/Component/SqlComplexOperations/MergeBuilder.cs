using System.Data;
using SqlComplexOperations.Models;
using SqlComplexOperations.Services;
using SqlComplexOperations.Extensions;
using SqlComplexOperations.Models.Enumerators;
using System.Linq.Expressions;

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
        private bool UseEnumStatus { get; set; } = false;
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
        /// Caso queira usar uma coluna(ENUM BulkStatus) de status para executar o merge. (Default = FALSE).
        /// </summary>
        /// <param name="expression">A coluna deve estar dentro de <see cref="TEntity"/> e deve ser do tipo <see cref="BulkMergeStatus"/></param>
        /// <returns>
        /// Retorna o MergeBuilder atual.
        /// </returns>
        public MergeBuilder<TEntity> UseStatusConfiguration(Expression<Func<TEntity, BulkMergeStatus>> expression)
        {
            UseEnumStatus = true;

            var member = (MemberExpression)expression.Body;
            StatusColumn = member.Member.Name;

            return this;
        }

        /// <summary>
        /// <para>[Opcional]</para>
        /// Caso queira usar uma coluna(STRING) de status para executar o merge. (Default = FALSE).
        /// </summary>
        /// <param name="expression">A coluna deve estar dentro de <see cref="TEntity"/></param>
        /// <returns>
        /// Retorna o MergeBuilder atual.
        /// </returns>
        public MergeBuilder<TEntity> UseStatusConfiguration(Expression<Func<TEntity, string>> expression)
        {
            UseEnumStatus = true;

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
        public MergeBuilder<TEntity> WithCondition<TConditionType, TConditionOperator>(TConditionType conditionType, TConditionOperator conditionOperator, Expression<Func<TEntity, object>> expression)
            where TConditionType : Enum
            where TConditionOperator : Enum
        {
            var cTypeValue = (ConditionType)Enum.Parse(typeof(TConditionType), conditionType.ToString());
            var cOperatorValue = (ConditionOperator)Enum.Parse(typeof(TConditionOperator), conditionOperator.ToString());
            var columns = GetColumns(expression);

            var condition = new ConditionBuilder(columns ?? new List<string>(), cTypeValue, cOperatorValue);
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
        public MergeBuilder<TEntity> WithCondition<TConditionType, TFieldType>(TConditionType conditionType, Expression<Func<TEntity, TFieldType>> expression)
            where TConditionType : Enum
            where TFieldType : struct
        {
            var cTypeValue = (ConditionType)Enum.Parse(typeof(TConditionType), conditionType.ToString());
            var column = expression.Body.Type.GetProperties().Select(m => m.Name).First();

            var condition = new ConditionBuilder(new List<string> { column }, cTypeValue, ConditionOperator.NONE);
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
        public MergeBuilder<TEntity> WithCondition<TConditionType>(TConditionType conditionType, Expression<Func<TEntity, string>> expression)
            where TConditionType : Enum
        {
            var cTypeValue = (ConditionType)Enum.Parse(typeof(TConditionType), conditionType.ToString());

            var member = (MemberExpression)expression.Body;
            var column = member.Member.Name;

            var condition = new ConditionBuilder(new List<string> { column }, cTypeValue, ConditionOperator.NONE);
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
        public async Task<bool> Execute()
        {
            ValidateBuilderPreExecute();

            SetAllColumns();
            SetPrimaryKeyColumn(DbTransaction!);

            CheckSnakeCaseOnExecuteCommand();

            CreateTempTable(DbTransaction!);
            await PopulateTempTable(DbTransaction!);
            ExecuteMergeCommand(DbTransaction!);

            DropTempTable(DbTransaction!);

            return true;
        }

        private void ValidateBuilderPreExecute()
        {
            if(DbTransaction == null)
                throw new ArgumentException("Transação não informada.");

            if(DbTransaction.Connection == null)
                throw new ArgumentException("Transação não informada.");

            if(!MergedColumns.Any())
                throw new ArgumentException("Informe o parametro MergedColumns");

            if(!UpdatedColumns.Any())
                throw new ArgumentException("Informe o parametro UpdatedColumns");

            if(!DataSource.Any())
                throw new ArgumentException("Informe o parametro Datasource");
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

        private void SetPrimaryKeyColumn(IDbTransaction dbTransaction)
        {
            var query = SqlBuilder.BuildPrimaryKeyQuery(_tableName);

            var result = _databaseService.ExecuteScalarCommand(dbTransaction, query);

            PrimaryKey = result?.ToString() ?? string.Empty;

            if(SnakeCaseNamingConvention)
                PrimaryKey = PrimaryKey.ToSnakeCase();
        }

        private void ExecuteMergeCommand(IDbTransaction dbTransaction)
        {
            var stringBuilderQuery = SqlBuilder.BuildMerge(_tableName, MergedColumns, RemoveIgnoredAndDuplicatedUpdateColumns(UpdatedColumns), RemoveIgnoredAndDuplicatedInsertColumns(AllColumns), Conditions, StatusColumn, UseEnumStatus);
            
            _databaseService.ExecuteNonQueryCommand(dbTransaction, stringBuilderQuery.ToString());
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
            var sqlCommand = SqlBuilder.BuildTempTable(_tableName);

            _databaseService.ExecuteNonQueryCommand(dbTransaction, sqlCommand);
        }

        private async Task PopulateTempTable(IDbTransaction dbTransaction)
        {
            await _databaseService.PopulateTempTable(dbTransaction, DataSource, $"#{_tableName}");
        }

        private void DropTempTable(IDbTransaction dbTransaction)
        {
            var sqlCommand = SqlBuilder.BuildDropTempTable(_tableName);

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